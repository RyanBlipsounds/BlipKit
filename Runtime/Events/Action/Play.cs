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

            public override bool NeedsEmitter { get { return true; } }

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] != null)
                {
                    emitters[emitterIndex].PlayClip(Clip);
                }
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Play))]
        public class BlipActionPlayInspector : PropertyDrawer
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
