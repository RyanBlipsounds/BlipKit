using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blip
{
    namespace Action
    {
        [System.Serializable]
        public class SpatialBlendByDistance : BlipAction
        {
            public float MinRange = 1f;
            public float MaxRange = 20f;

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] == null) return;

                emitters[emitterIndex].SetSpatialBlendByDistance
                (
                    MinRange, 
                    MaxRange
                );
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                // This is not meant to be used like this.
                return;
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SpatialBlendByDistance))]
        public class BlipActionSpatialBlendByDistanceInspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y, position.width, 20f), 
                    property.FindPropertyRelative("MinRange"), 
                    new GUIContent("Min Range")
                );
                
                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y + 20f, position.width, 20f), 
                    property.FindPropertyRelative("MaxRange"), 
                    new GUIContent("Max Range")
                );

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty minRangeProperty = property.FindPropertyRelative("MinRange");
                SerializedProperty maxRangeProperty = property.FindPropertyRelative("MaxRange");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(minRangeProperty);
                totalHeight += EditorGUI.GetPropertyHeight(maxRangeProperty);

                return totalHeight + 20f;
            }
        }
#endif
    }
}
