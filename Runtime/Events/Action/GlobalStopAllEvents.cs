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
            // TODO: Add exceptions.

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                // TODO
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                // TODO: Add self-exception.
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
