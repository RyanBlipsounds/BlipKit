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
        // Stops an event from playing anywhere in the scene, using blip statics.
        [System.Serializable]
        public class GlobalStopEvent : BlipAction
        {
            public BlipEvent[] Events;

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                // Emitters ignored.

                foreach (BlipEvent eventToStop in Events)
                {
                    Blip.Statics.StopEvent(eventToStop);
                }
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                // Note: Not global. Unlikely to be needed in this context until multiple-event-
                // preview in editor is implemented.
                audioSource.Stop();
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(GlobalStopEvent))]
        public class BlipActionGlobalStopEventInspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y, position.width, 20f), 
                    property.FindPropertyRelative("Events"), 
                    label
                );

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty eventsProperty = property.FindPropertyRelative("Events");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(eventsProperty);

                return totalHeight;
            }
        }
#endif
    }
}
