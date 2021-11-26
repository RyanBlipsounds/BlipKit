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
        public class PlayFromRandomSet : BlipAction
        {
            public AudioClip[] Clips;

            /**
             * If true, the same sound will never be picked twice. 
             * NOTE: We may want a more sophisticated weighting system. This doesnt prevent,
             *       for example, two sounds ping-ponging back and forth and ignoring others.
             */ 
            public bool PreventDuplicatePicks = true;

            private int lastPick = -1;

            public override bool NeedsEmitter { get { return true; } }

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] == null) return;

                int randomPick = Random.Range(0, Clips.Length);;

                if (PreventDuplicatePicks && Clips.Length > 1)
                {
                    while (randomPick == lastPick)
                    {
                        randomPick = Random.Range(0, Clips.Length);
                    }
                }

                lastPick = randomPick;

                emitters[emitterIndex].PlayClip(Clips[randomPick]);
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(PlayFromRandomSet))]
        public class BlipActionPlayFromRandomSetInspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                SerializedProperty prop = property.FindPropertyRelative("PreventDuplicatePicks");
                Rect pos = new Rect(position.x - 15f, position.y + 10f, position.width, EditorGUI.GetPropertyHeight(prop));

                EditorGUI.PropertyField
                (
                    pos, 
                    prop, 
                    label
                );

                pos.x += 20f;
                EditorGUI.LabelField(pos, "Prevent duplicate picks");

                prop = property.FindPropertyRelative("Clips");
                pos = new Rect(position.x, position.y + 40f, position.width, EditorGUI.GetPropertyHeight(prop));
            
                //pos.x += 20f;

                EditorGUI.PropertyField
                (
                    pos,
                    prop, 
                    label,
                    true
                );

                EditorGUI.LabelField
                (
                    new Rect(position.x + 5f, position.y + 40f, position.width, 20f), 
                    "Clips to choose from"
                );

                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty clipsProperty = property.FindPropertyRelative("Clips");
                SerializedProperty preventDuplicatePicksProperty = property.FindPropertyRelative("PreventDuplicatePicks");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(clipsProperty);
                totalHeight += EditorGUI.GetPropertyHeight(preventDuplicatePicksProperty);

                return totalHeight + 20f;
            }
        }
#endif
    }
}
