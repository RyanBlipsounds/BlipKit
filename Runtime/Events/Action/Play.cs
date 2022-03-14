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
        public class Play : BlipAction
        {
            public AudioClip Clip;
            public bool IsLooping;

            public override bool NeedsEmitter { get { return true; } }

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] != null)
                {
                    emitters[emitterIndex].PlayClip(Clip);
                    emitters[emitterIndex].GetSource().loop = IsLooping;
                }
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                audioSource.clip = Clip;
                audioSource.Play();
                audioSource.loop = IsLooping;
                // Note: We may want this split up for more control from Actions.
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Play))]
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
                
                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y + 20f, position.width, 20f), 
                    property.FindPropertyRelative("IsLooping"), 
                    new GUIContent("Looping ")
                );

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty clipProperty = property.FindPropertyRelative("Clip");
                SerializedProperty loopProperty = property.FindPropertyRelative("IsLooping");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(clipProperty);
                totalHeight += EditorGUI.GetPropertyHeight(loopProperty);

                return totalHeight;
            }
        }
#endif
    }
}
