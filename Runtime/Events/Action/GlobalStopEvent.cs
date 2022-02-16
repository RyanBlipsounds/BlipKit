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
            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                // TODO
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

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return 0f;
            }
        }
#endif
    }
}
