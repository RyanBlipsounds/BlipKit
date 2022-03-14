using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blip
{
    public class BlipComplexAmbientRegionEmitter : MonoBehaviour
    {
        private const float MAX_INTERSECT_RAY_LENGTH = 1000f;

        #region Inspector Fields
        public GameObject EmitterObject;
        public MeshCollider AreaMeshColliderConvex;
        public MeshCollider AreaMeshColliderNonConvex;

        [Space(10)]

        public Vector3[] ConcaveProbes;

        [Space(10)]

        public bool UseActiveRange = true;
        public Vector3 OffsetActiveCenter;
        public float ActiveRange = 100f;

        [Tooltip("Time needed in or out of active range to change area activation.")]
        public float ActiveTimeNeeded = 3f;

        [Space(10)]

        public float TicksPerSecond = 10;

        #endregion

        #region Runtime Fields

        private float tickTimer = 0f;
        private float activeTimer = 0f;

        bool checkNonConvex;
        private bool isPlayerInActiveRange = false;
        public bool isPlayerInside = false;
        private bool isActive = false;
        
        private bool isAttached = false;

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

            checkNonConvex = AreaMeshColliderNonConvex != null;
            if (checkNonConvex)
            {
                // If a non-convex mesh is provided, ensure it is non-convex.
                Assert.IsFalse(AreaMeshColliderNonConvex.convex);
            }
        }

        private void LateUpdate()
        {
            if (isAttached)
            {
                EmitterObject.transform.position = Blip.Statics.GetListenerGameobject().transform.position;
            }
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

            EmitterObject.transform.position = checkNonConvex ? 
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

            // Probes.
            Gizmos.color = Color.red;

            // Active center is always a probe, as a fallback.
            Gizmos.DrawSphere(transform.position + OffsetActiveCenter, 0.25f);

            if (ConcaveProbes.Length < 1) return;

            foreach (Vector3 probe in ConcaveProbes)
            {
                Gizmos.DrawSphere(transform.position + probe, 0.25f);
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

        private bool CheckConcaveHull(Ray ray, RaycastHit hit)
        {
            rayTemp.origin = Blip.Statics.GetListenerGameobject().transform.position;;
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
            Vector3 listenerPos = Blip.Statics.GetListenerGameobject().transform.position;

            // Starts with a convex check. This is a big part of why this version is way less
            // performant.
            Vector3 closestPointNonConvex = ClosestPointInsideConvex();

            if (closestPointNonConvex != listenerPos)
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
            // between the convex and non-convex meshes. 

            // Using probe approximation. 
            // First probe is always active center.
            Vector3 nearestProbe = transform.position + OffsetActiveCenter;
            float nearestProbeDistance = Vector3.Distance(nearestProbe, listenerPos);
            float tempDist = nearestProbeDistance;

            // Check probes (if any exist)
            for (int i=0; i<ConcaveProbes.Length; i++)
            {
                tempDist = Vector3.Distance(transform.position + ConcaveProbes[i], listenerPos);

                if (tempDist < nearestProbeDistance)
                {
                    nearestProbeDistance = tempDist;
                    nearestProbe = transform.position + ConcaveProbes[i];
                }
            }

            // Cast ray to probe and report hit on non-convex mesh.
            isAttached = false;

            rayTemp.origin = listenerPos;
            rayTemp.direction = nearestProbe - listenerPos;  

            if(AreaMeshColliderNonConvex.Raycast(rayTemp, out hitTemp, MAX_INTERSECT_RAY_LENGTH)) 
            {
                return hitTemp.point;
            }

            // This shouldn't ever happen. The ray misses. Don't do anything, in this case.
            return EmitterObject.gameObject.transform.position;
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
    }   
}
