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
        public class StopAllInEvent : BlipAction
        {
            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] != null)
                {
                    emitters[emitterIndex].Stop();
                }
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                audioSource.Stop();
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(StopAllInEvent))]
        public class BlipActionStopAllInEventInspector : PropertyDrawer
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
