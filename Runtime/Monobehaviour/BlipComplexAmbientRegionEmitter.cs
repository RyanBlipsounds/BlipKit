using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blip
{
    public class BlipComplexAmbientRegionEmitter : MonoBehaviour
    {
        #region Internal Data Classes
        // Could be moved to a core physics class, but right now only used here.

        public class Tri
        {
            public int vertIndexA,vertIndexB,vertIndexC;
            public Vector3 a, b, c;

            public Tri
            (
                int vertIndexA, 
                int vertIndexB, 
                int vertIndexC, 
                Vector3 a, 
                Vector3 b, 
                Vector3 c
                )
            {
                this.vertIndexA = vertIndexA;
                this.vertIndexB = vertIndexB;
                this.vertIndexC = vertIndexC;
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public Tri(Tri tri)
            {
                this.vertIndexA = tri.vertIndexA;
                this.vertIndexB = tri.vertIndexB;
                this.vertIndexC = tri.vertIndexC;
                this.a = tri.a;
                this.b = tri.b;
                this.c = tri.c;
            }
        }

        public class Line
        {
            public Vector3 a;
            public Vector3 b;

            public Line(Vector3 a, Vector3 b)
            {
                this.a = a;
                this.b = b;
            }
        }

        public class Plane
        {
            // Plane made up of three points.
            private Vector3 a;
            private Vector3 b;
            private Vector3 c;
            private Vector3 normal;
            private float distanceToOrigin;

            public Plane(Vector3 a, Vector3 b, Vector3 c)
            {
                this.a = a;
                this.b = b;
                this.c = c;

                // Compute normal & distance to origin.
                normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
                distanceToOrigin = Vector3.Dot(normal, a);
            }

            public Vector3 GetNormal()
            {
                return normal;
            }

            public float GetDistanceToOrigin()
            {
                return distanceToOrigin;
            }

            // Note: Right now plane can never be changed. Will add that if ever needed.
        }

        #endregion

        private const float MAX_INTERSECT_RAY_LENGTH = 1000f;

        public enum TrackingOverrideMode
        {
            NoOverride,
            ListenerHeight,
            FixedHeight
        }

        #region Inspector Fields

        [Header("References")]

        [Tooltip("Required. The gameobject acting as the emitter, which will be moved when " +
            "tracking the listener along the edges and inside the complex ambient region. Likely " +
            "a BlipAmbientEmitter but can be anything.")]
        public GameObject EmitterObject;

        [Tooltip("Required. All regions must include a convex mesh collider, including non-" +
            "convex mesh. In that case, include a separate non-convex version of the mesh in the " +
            "next field.")]
        public MeshCollider AreaMeshColliderConvex;
        
        [Tooltip("Optional. If included, more CPU-intensive calculations will be used, but a non-" +
            "convex mesh can be used. If the region can be concave, use only that.")]
        public MeshCollider AreaMeshColliderNonConvex;

        [Space(10)]
        [Header("Listener Tracking")]

        [Tooltip("How many times per second the listener's position is tracked. Higher ticks " +
            "per second costs more performance, but are more accurate especially if the listener " +
            "is moving quickly.")]
        public float TicksPerSecond = 10;

        [Tooltip("The tracking mode for height (y axis). 'No Override' is the default. If the " +
            "region is intended to be two-dimensional, set this to fixed height which maintains " +
            "the emitter height (y) at 0. If the player cannot approach the region from " +
            "different heights, consider setting this to Listener Height, which keeps the " +
            "emitter at player height. ")]
        public TrackingOverrideMode HeightTracking = TrackingOverrideMode.NoOverride;
        public float heightOffset = 0f;

        [Tooltip("The speed at which the emitter moves around to track the player (when not " +
            "attached).")]
        public float TrackingSpeed = 10f;

        [Space(5)]

        [Tooltip("If true, will use high precision tracking on a non-convex mesh, even if " + 
            "a convex approximation is possible. Only use this if you can spare the CPU " +
            "performance and a concave section of the area mesh is resulting in sudden changes " +
            "in tracking volume in some areas.")]
        public bool UseHighPrecisionConcave;

        [Tooltip("If using high precision concave, set a range away from the emitter for it to " +
            "be active, in meters. If out of range but still in activation range, revert to " +
            "convex approximation. If this value is larger than the activation range then " +
            "concave approximation is never used.")]
        public float HighPerformanceRange = 10f;

        [Space(10)]

        [Header("Activation Range")]

        public bool UseActiveRange = true;
        public Vector3 OffsetActiveCenter;
        public float ActiveRange = 100f;

        [Tooltip("Time needed in or out of active range to change area activation.")]
        public float ActiveTimeNeeded = 3f;

        #endregion

        #region Runtime Fields

        private float tickTimer = 0f;
        private float activeTimer = 0f;
        private Vector3 targetEmitterPosition;
        private float[] vertexDistances;
        private List<Tri> candidateTris = new List<Tri>();
        private float closestVertexDistance;
        private List<int> closestVertexIndices = new List<int>();
        private float currentEmitterDistance;
        private int verticesPolledPerTick;
        private int verticesPolledThisRound;
        private bool isNonconvex;
        private bool isPlayerInActiveRange = false;
        private bool isActive = false;
        private bool isAttached = false;
        private bool isPollingVertices = false;

        #region Rays to Check Non-convex Intersection

        private Ray rayUp;
        private Ray rayDown;
        private Ray rayLeft;
        private Ray rayRight;
        private Ray rayForward;
        private Ray rayBack;
        private Ray rayTemp;
    
        private RaycastHit hitUp      = new RaycastHit();
        private RaycastHit hitDown    = new RaycastHit();
        private RaycastHit hitLeft    = new RaycastHit();
        private RaycastHit hitRight   = new RaycastHit();
        private RaycastHit hitForward = new RaycastHit();
        private RaycastHit hitBack    = new RaycastHit();
        private RaycastHit hitTemp    = new RaycastHit();

        #endregion

        #endregion

        private void Start()
        {
            targetEmitterPosition = transform.position;

            if (!UseActiveRange)
            {
                isActive = true;
            }

            // Set up intersection rays.
            rayUp       = new Ray (Vector3.zero , -Vector3.up);
            rayDown     = new Ray (Vector3.zero , -Vector3.down);
            rayLeft     = new Ray (Vector3.zero , -Vector3.left);
            rayRight    = new Ray (Vector3.zero , -Vector3.right);
            rayForward  = new Ray (Vector3.zero , -Vector3.forward);
            rayBack     = new Ray (Vector3.zero , -Vector3.back);
            rayTemp     = new Ray ();

            // Must have a convex mesh for either convex or non-convex operation.
            Assert.IsTrue(AreaMeshColliderConvex != null && AreaMeshColliderConvex.convex);

            isNonconvex = AreaMeshColliderNonConvex != null;
            if (isNonconvex)
            {
                // If a non-convex mesh is provided, ensure it is non-convex.
                Assert.IsFalse(AreaMeshColliderNonConvex.convex);
            }

            // Used to poll non-convex vertices, if one exists.
            if (isNonconvex)
            {
                vertexDistances = new float[AreaMeshColliderNonConvex.sharedMesh.vertices.Length];

                // Assuming 30fps, a TicksPerSecond of 30 means all vertices are polled each frame.
                //t*t*t * (t * (6f*t - 15f) + 10f)
                verticesPolledPerTick = Mathf.Clamp
                (
                    (int)(vertexDistances.Length / (30.0f / TicksPerSecond)),
                    1,
                    vertexDistances.Length
                );

                verticesPolledThisRound = 0;
            }
        }

        private void LateUpdate()
        {
            currentEmitterDistance = Vector3.Distance
            (
                targetEmitterPosition, 
                Blip.Statics.GetListenerGameobject().transform.position
            );

            if (isPollingVertices)
            {
                PollVertices();
            }

            if (isAttached)
            {
                targetEmitterPosition = Blip.Statics.GetListenerGameobject().transform.position;
            }

            // Smooth follow the target emitter position.
            Vector3 resultEmitterPosition = Vector3.Lerp
            (
                EmitterObject.transform.position, 
                targetEmitterPosition, 
                TrackingSpeed * Time.deltaTime
            );
            
            switch (HeightTracking)
            {
                case TrackingOverrideMode.ListenerHeight:
                    resultEmitterPosition.y = 
                        Blip.Statics.GetListenerGameobject().transform.position.y; 
                    break;

                case TrackingOverrideMode.FixedHeight:
                    resultEmitterPosition.y = 0f;
                    break;
            }

            EmitterObject.transform.position = resultEmitterPosition + (Vector3.up * heightOffset);
        }

        private void FixedUpdate()
        {
            CheckActiveRange();

            if (!isActive)
            {
                return;
            }

            // Tick.
            tickTimer += Time.deltaTime;

            if (tickTimer >= 1f / TicksPerSecond)
            {
                tickTimer = 0;
            }
            else
            {
                return;
            }

            targetEmitterPosition = isNonconvex ? 
                ClosestPointInsideNonConvex() :
                ClosestPointInsideConvex();
        }

        private void OnDrawGizmosSelected()
        {
            // Active Range.
            if (!UseActiveRange)
            {
                return;
            }

            if (isActive)
            {
                 Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }
            Gizmos.DrawWireSphere(transform.position + OffsetActiveCenter, ActiveRange);

            /* Debug Only
            // Closest vertex.
            Gizmos.color = Color.magenta;

            foreach (int vertexIndex in closestVertexIndices)
            {
                Gizmos.DrawSphere
                (
                    AreaMeshColliderNonConvex.transform.TransformPoint
                    (
                        AreaMeshColliderNonConvex.sharedMesh.vertices[vertexIndex]
                    ), 
                    0.5f
                );
            }

            // Draw triangles that are candidates for closest (share nearest vertex).
            if (AreaMeshColliderNonConvex == null || candidateTris == null) return;

            Gizmos.color = Color.red;

            foreach (Tri tri in candidateTris)
            {
                // A to B
                Gizmos.DrawLine(tri.a, tri.b);

                // B to C
                Gizmos.DrawLine(tri.b, tri.c);

                // C to A
                Gizmos.DrawLine(tri.c, tri.a);
            }
            */
        }

        private void PollVertices()
        {
            if (!isNonconvex)
            {
                return;
            }

            //Vector3 colliderMeshPos = AreaMeshColliderNonConvex.transform.position;
            Vector3 listenerPos = Blip.Statics.GetListenerGameobject().transform.position;
            Vector3 tempVertexPos;

            int polledThisTick = 0;
            for (int i=verticesPolledThisRound; i<vertexDistances.Length; i++)
            {
                tempVertexPos = AreaMeshColliderNonConvex.transform.TransformPoint
                (
                    AreaMeshColliderNonConvex.sharedMesh.vertices[i]
                );

                vertexDistances[i] = Vector3.Distance
                (
                    listenerPos, 
                    tempVertexPos
                );

                if (vertexDistances[i] <= closestVertexDistance)
                {
                    if (vertexDistances[i] != closestVertexDistance)
                    {
                        closestVertexDistance = vertexDistances[i];
                        closestVertexIndices.Clear();
                    }

                    closestVertexIndices.Add(i);
                }

                polledThisTick++;
                if (polledThisTick > verticesPolledPerTick)
                {
                    break;
                }
            }

            verticesPolledThisRound += polledThisTick;

            if (verticesPolledThisRound >= vertexDistances.Length)
            {
                // All done. Wait for this to be set again in the processing tick.
                isPollingVertices = false;
            }
        }

        private void CheckActiveRange()
        {
            if (!UseActiveRange)
            { 
                isActive = true;
                return;
            }

            float listenerDistance = Vector3.Distance
            (
                Blip.Statics.GetListenerGameobject().transform.position,
                transform.position
            );

            if (listenerDistance < ActiveRange && !isPlayerInActiveRange)
            {
                activeTimer = 0f;
                isPlayerInActiveRange = true;
            }
            else if (listenerDistance >= ActiveRange && isPlayerInActiveRange)
            {
                activeTimer = 0f;
                isPlayerInActiveRange = false;
            }
            else
            {
                activeTimer += Time.deltaTime;

                if (activeTimer >= ActiveTimeNeeded && isActive != isPlayerInActiveRange)
                {
                    isActive = isPlayerInActiveRange;
                }
            }

            return;
        }

        // Checks a ray cast from the listener to determine if it hits within the concave hull
        // of the collider mesh that it's inside, returning true on a success and false on a miss.
        // Used as part of the algorithm to check if a point (the listener) is inside of a non-
        // convex shape.
        private bool CheckConcaveHull(Ray ray, RaycastHit hit)
        {
            rayTemp.origin = Blip.Statics.GetListenerGameobject().transform.position;
            rayTemp.direction = -ray.direction;

            float customDistance = MAX_INTERSECT_RAY_LENGTH - hit.distance;
            int lastPoint = hit.triangleIndex;
    
            while(AreaMeshColliderNonConvex.Raycast(rayTemp, out hitTemp, customDistance))
            {
                if(hitTemp.triangleIndex == lastPoint)
                {
                    break;
                }

                lastPoint = hitTemp.triangleIndex;
                customDistance = hitTemp.distance;
                ray.origin = -ray.direction * customDistance + transform.position;
    
                if(!AreaMeshColliderNonConvex.Raycast(ray, out hitTemp, customDistance)) 
                {
                    return true;
                }
    
                if(hitTemp.triangleIndex == lastPoint)
                {
                    break;
                } 

                lastPoint = hitTemp.triangleIndex;
                customDistance -= hitTemp.distance;
            }
    
            return false;
        }

        // Returns the cloest point from the Blip Statics listener position to a mesh collider 
        // which is convex. This is more performant than ClosestPointInsideNonConvex().
        private Vector3 ClosestPointInsideConvex()
        {
            Vector3 closestPoint = AreaMeshColliderConvex.ClosestPoint
            (
                Blip.Statics.GetListenerGameobject().transform.position
            );

            isAttached = closestPoint == Blip.Statics.GetListenerGameobject().transform.position;

            return closestPoint;
        }

        // Returns the cloest point from the Blip Statics listener position to a mesh collider
        // which is non-convex. If the mesh collider is convex, call ClosestPointInsideConvex() instead
        // for performance reasons.
        private Vector3 ClosestPointInsideNonConvex()
        {
            isPollingVertices = false; // Gets set to true in this function later if polling continues.

            Vector3 listenerPos = Blip.Statics.GetListenerGameobject().transform.position;

            // Starts with a convex check. This is a big part of why this version is way less
            // performant.
            Vector3 closestPointNonConvex = ClosestPointInsideConvex();

            // Check if a convex approximation should be done, to save us some processing. This can
            // also be bypassed with a high-precision mode, which is checked in this conditional.
            // Note: currentEmitterDistance is set in LateUpdate.
            if (closestPointNonConvex != listenerPos && 
                (!UseHighPrecisionConcave || currentEmitterDistance >= HighPerformanceRange))
            {
                // We're outside of the convex mesh, so it's impossible we're inside the non-
                // convex mesh. Cast a ray from the listener to the convex point to 
                // APPROXIMATE a closest point on the outside of the non-convex.
                isAttached = false;

                rayTemp.origin = listenerPos;
                rayTemp.direction = closestPointNonConvex - listenerPos;  

                if(AreaMeshColliderNonConvex.Raycast(rayTemp, out hitTemp, MAX_INTERSECT_RAY_LENGTH)) 
                {
                    return hitTemp.point;
                }
                // Failure case on this raycast results in a full concave check, below.
            }
            // Otherwise, we need to check if the listener is inside of the non-convex mesh.

            // Attach mode allows emitter object to follow in LateUpdate instead of only during
            // interior ticks.
            isAttached = CheckIntersectionNonConvex(listenerPos);

            if (isAttached)
            {
                // If inside, return the BlipStatics position.
                return listenerPos;
            }
            // Otherwise, we are in a worst-case scenario: the listener is in the concave area
            // between the convex and non-convex meshes. This uses a piece-meal approach to
            // check distances to all vertices in the non-convex mesh across the updates that
            // make up a tick (in LateUpdate) then putting them together here.

            // Turn on vertex polling. Will continue polling vertices in LateUpdate.
            isPollingVertices = true;

            // Is a round of vertices finished being polled? This should only be happening when
            // first switching to a concave calculation the first frame that isPollingVertices
            // is true. If it's happening each tick, something is wrong with the tickrate 
            // polling distribution. 
            if (verticesPolledThisRound < vertexDistances.Length)
            {
                // Finish polling.
                Vector3 tempVertexPos;

                for (int i=verticesPolledThisRound; i<vertexDistances.Length; i++)
                {
                    tempVertexPos = AreaMeshColliderNonConvex.transform.TransformPoint
                    (
                        AreaMeshColliderNonConvex.sharedMesh.vertices[i]
                    );

                    vertexDistances[i] = Vector3.Distance
                    (
                        listenerPos, 
                        tempVertexPos
                    );

                    if (vertexDistances[i] <= closestVertexDistance)
                    {
                        if (vertexDistances[i] != closestVertexDistance)
                        {
                            closestVertexDistance = vertexDistances[i];
                            closestVertexIndices.Clear();
                        }

                        closestVertexIndices.Add(i);
                    }
                }
            }

            // Get all triangles that share the closest vertex index.
            SetCandidateTrisWithSharedVertex();

            // Get nearest point on all triangles and set the emitter pos to the closest.
            Vector3 closestPoint = targetEmitterPosition;  // This shouldn't ever stay.
            float closestPointLengthSqr = float.MaxValue;

            Vector3 tempPoint;
            float tempLengthSqr;
            
            for (int i=0; i<candidateTris.Count; i++)
            {
                tempPoint = GetClosestPointToTri(candidateTris[i], listenerPos);
                tempLengthSqr = (listenerPos - tempPoint).sqrMagnitude;

                if (tempLengthSqr < closestPointLengthSqr)
                {
                    closestPointLengthSqr = tempLengthSqr;
                    closestPoint = tempPoint;
                }
            }

            // Reset.
            closestVertexIndices.Clear();
            closestVertexDistance = float.MaxValue;
            verticesPolledThisRound = 0;

            return closestPoint;
        }

        private void SetCandidateTrisWithSharedVertex()
        {
            candidateTris = new List<Tri>();

            foreach (int vertexIndex in closestVertexIndices)
            {
                for (int i=0; i<AreaMeshColliderNonConvex.sharedMesh.triangles.Length; i+=3)
                {
                    if (vertexIndex == AreaMeshColliderNonConvex.sharedMesh.triangles[i]   || 
                        vertexIndex == AreaMeshColliderNonConvex.sharedMesh.triangles[i+1] || 
                        vertexIndex == AreaMeshColliderNonConvex.sharedMesh.triangles[i+2])
                    {
                        candidateTris.Add
                        (
                            new Tri
                            (
                                i,
                                i+1,
                                i+2,
                                AreaMeshColliderNonConvex.transform.TransformPoint
                                (
                                    AreaMeshColliderNonConvex.sharedMesh.vertices
                                        [AreaMeshColliderNonConvex.sharedMesh.triangles[i]]
                                ), 
                                AreaMeshColliderNonConvex.transform.TransformPoint
                                (
                                    AreaMeshColliderNonConvex.sharedMesh.vertices
                                        [AreaMeshColliderNonConvex.sharedMesh.triangles[i+1]]
                                ), 
                                AreaMeshColliderNonConvex.transform.TransformPoint
                                (
                                    AreaMeshColliderNonConvex.sharedMesh.vertices
                                        [AreaMeshColliderNonConvex.sharedMesh.triangles[i+2]]
                                )
                            )
                        );
                    }
                }
            }
        }

        private bool CheckIntersectionNonConvex(Vector3 targetPoint)
        {
            // Cast rays from each cardinal direction, out of target position.
            rayUp.origin      = -rayUp.direction      * MAX_INTERSECT_RAY_LENGTH + targetPoint;
            rayDown.origin    = -rayDown.direction    * MAX_INTERSECT_RAY_LENGTH + targetPoint;
            rayLeft.origin    = -rayLeft.direction    * MAX_INTERSECT_RAY_LENGTH + targetPoint;
            rayRight.origin   = -rayRight.direction   * MAX_INTERSECT_RAY_LENGTH + targetPoint;
            rayForward.origin = -rayForward.direction * MAX_INTERSECT_RAY_LENGTH + targetPoint;
            rayBack.origin    = -rayBack.direction    * MAX_INTERSECT_RAY_LENGTH + targetPoint;

            // Directional rays must all hit to be inside.
            if
            (
                AreaMeshColliderNonConvex.Raycast(rayUp,      out hitUp,      MAX_INTERSECT_RAY_LENGTH) &&
                AreaMeshColliderNonConvex.Raycast(rayDown,    out hitDown,    MAX_INTERSECT_RAY_LENGTH) &&
                AreaMeshColliderNonConvex.Raycast(rayLeft,    out hitLeft,    MAX_INTERSECT_RAY_LENGTH) &&
                AreaMeshColliderNonConvex.Raycast(rayRight,   out hitRight,   MAX_INTERSECT_RAY_LENGTH) &&
                AreaMeshColliderNonConvex.Raycast(rayForward, out hitForward, MAX_INTERSECT_RAY_LENGTH) &&
                AreaMeshColliderNonConvex.Raycast(rayBack,    out hitBack,    MAX_INTERSECT_RAY_LENGTH)
            )
            {
                // Further, check if hits are within the concave hull of each direction. If any of
                // of them aren't then we can stop immediately.
                if      (CheckConcaveHull(rayUp,hitUp))           return false;
                else if (CheckConcaveHull(rayDown,hitDown))       return false;
                else if (CheckConcaveHull(rayLeft,hitLeft))       return false;
                else if (CheckConcaveHull(rayRight,hitRight))     return false;
                else if (CheckConcaveHull(rayForward,hitForward)) return false;
                else if (CheckConcaveHull(rayBack,hitBack))       return false;
                else                                              return true;                
    
            } 
            else 
            {
                return false;
            }
        }

        private Vector3 GetClosestPointToTri(Tri tri, Vector3 point)
        {
            Plane plane = new Plane(tri.a, tri.b, tri.c);
            point = ClosestPointOnPlane(plane, point);

            // Check if point is inside the triangle.
            if (IsPointInTriangle(tri, point))
            {
                // If so return the point as the result.
                return point;
            }

            // If the point isn't inside the triangle, find the closest point to each of the three 
            // edges of the triangle.
            Vector3 closestToAB = ClosestPointOnLine(tri.a, tri.b, point);
            Vector3 closestToBC = ClosestPointOnLine(tri.b, tri.c, point);
            Vector3 closestToCA = ClosestPointOnLine(tri.c, tri.a, point);

            // Return the line-point with the shortest distance. We can use squared to avoid 
            // needing to perform a square root for actual length, since we're only comparing.
            float sqrDistanceFromClosestPointAB = (point - closestToAB).sqrMagnitude;
            float sqrDistanceFromClosestPointBC = (point - closestToBC).sqrMagnitude;
            float sqrDistanceFromClosestPointCA = (point - closestToCA).sqrMagnitude;

            if (sqrDistanceFromClosestPointAB < sqrDistanceFromClosestPointBC &&
                sqrDistanceFromClosestPointAB < sqrDistanceFromClosestPointCA)
            {
                return closestToAB;
            }
            
            if (sqrDistanceFromClosestPointBC < sqrDistanceFromClosestPointAB &&
                sqrDistanceFromClosestPointBC < sqrDistanceFromClosestPointCA)
            {
                return closestToBC;
            }

            // Otherwise.
            return closestToCA;
        }

        // Returns the closest point on a plane to a target point.
        private Vector3 ClosestPointOnPlane(Plane plane, Vector3 point)
        {
            // The plane's normal is aassumed to be normalized.
            float distanceToPlane = Vector3.Dot(plane.GetNormal(), point) - plane.GetDistanceToOrigin();

            return point - distanceToPlane * plane.GetNormal();
        }

        // Returns the nearest point on a line between two points, relative to a target point.
        private Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 lineVector = lineEnd - lineStart;
            float t = Mathf.Clamp01
            (
                Vector3.Dot(point - lineStart, lineVector) / 
                Vector3.Dot(lineVector, lineVector)
            );

            return lineStart + t * lineVector;
        }

        // Returns true if a target point is within the plane of a triangle.
        private bool IsPointInTriangle(Tri tri, Vector3 point)
        {
            Tri tempTri = new Tri(tri);

            // Translate temp triangle so point is its origin.
            tempTri.a -= point;
            tempTri.b -= point;
            tempTri.c -= point;

            // Imagine creating triangles between each pair of vertices and the target point.
            // Find those normals.
            Vector3 normalPBC = Vector3.Cross(tempTri.b, tempTri.c);
            Vector3 normalPCA = Vector3.Cross(tempTri.c, tempTri.a);
            Vector3 normalPAB = Vector3.Cross(tempTri.a, tempTri.b);

            // If all of the normals are facing the same direction, the target point is within 
            // the triangle.
            if (Vector3.Dot(normalPBC, normalPCA) < 0f) 
            {
                return false;
            }

            if (Vector3.Dot(normalPBC, normalPAB) < 0.0f) 
            {
                return false;
            }

            return true;
        }
    }  
}
