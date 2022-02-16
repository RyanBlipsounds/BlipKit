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
        public class PlayClip : BlipAction
        {
            public AudioClip Clip;

            public override bool NeedsEmitter { get { return true; } }

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] != null)
                {
                    emitters[emitterIndex].PlayClip(Clip);
                }
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                audioSource.clip = Clip;
                audioSource.Play();
                // Note: We may want this split up for more control from Actions.
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(PlayClip))]
        public class BlipActionPlayClipInspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y, position.width, 20f), 
                    property.FindPropertyRelative("Clip"), 
                    label
                );

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty clipProperty = property.FindPropertyRelative("Clip");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(clipProperty);

                return totalHeight;
            }
        }
#endif
    }
}
