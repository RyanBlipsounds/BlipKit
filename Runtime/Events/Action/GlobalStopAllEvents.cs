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
        public class GlobalStopAllEvents : BlipAction
        {
            public BlipEvent[] Exceptions;

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                // Emitters ignored.

                Blip.Statics.StopAllEvents(Exceptions);
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                // Should we check for exceptions here?

                audioSource.Stop();
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(GlobalStopAllEvents))]
        public class BlipActionGlobalStopAllEventsInspector : PropertyDrawer
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
