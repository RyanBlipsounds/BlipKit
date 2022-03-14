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

            public bool IsLooping;

            private int lastPick = -1;

            public override bool NeedsEmitter { get { return true; } }

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (emitters[emitterIndex] == null) return;

                int randomPick = RollForIndex();

                emitters[emitterIndex].PlayClip(Clips[randomPick]);
                emitters[emitterIndex].GetSource().loop = IsLooping;
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                int randomPick = RollForIndex();

                audioSource.clip = Clips[randomPick];
                audioSource.Play();
                audioSource.loop = IsLooping;
            }

            private int RollForIndex()
            {
                int randomPick = Random.Range(0, Clips.Length);;

                if (PreventDuplicatePicks && Clips.Length > 1)
                {
                    while (randomPick == lastPick)
                    {
                        randomPick = Random.Range(0, Clips.Length);
                    }
                }

                lastPick = randomPick;

                return lastPick;
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

                pos = new Rect(position.x - 15f, position.y + 30f, position.width, EditorGUI.GetPropertyHeight(prop));
                prop = property.FindPropertyRelative("IsLooping");
                EditorGUI.PropertyField
                (
                    pos, 
                    prop, 
                    label
                );

                pos.x += 20f;
                EditorGUI.LabelField(pos, "Looping ");

                prop = property.FindPropertyRelative("Clips");
                pos = new Rect(position.x, position.y + 60f, position.width, EditorGUI.GetPropertyHeight(prop));
            
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
                    new Rect(position.x + 5f, position.y + 60f, position.width, 20f), 
                    "Clips to choose from"
                );

                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty clipsProperty = property.FindPropertyRelative("Clips");
                SerializedProperty preventDuplicatePicksProperty = property.FindPropertyRelative("PreventDuplicatePicks");
                SerializedProperty loopProperty = property.FindPropertyRelative("IsLooping");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(clipsProperty);
                totalHeight += EditorGUI.GetPropertyHeight(preventDuplicatePicksProperty);
                totalHeight += EditorGUI.GetPropertyHeight(loopProperty);

                return totalHeight + 20f;
            }
        }
#endif
    }
}
