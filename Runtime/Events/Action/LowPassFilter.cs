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
        public class LowPassFilter : BlipAction
        {
            [Range(10, 22000)]
            public int CutoffFrequency = 5000;

            [Range(1.0f, 10.0f)]
            public float ResonanceQ = 1.0f;

            public override void Apply(BlipEmitter[] emitters, int emitterIndex)
            {
                if (!emitters[emitterIndex])
                {
                    return;
                }

                emitters[emitterIndex].SetLowPassFilter(CutoffFrequency, ResonanceQ);
            }

            public override void ApplyToSingleAudioSource(AudioSource audioSource)
            {
                // Check for an existing highpass filter component.
                var lowPassFilterComponent = audioSource.gameObject.GetComponent<AudioLowPassFilter>();

                if (!lowPassFilterComponent)
                {
                    // If performance becomes a concern for this sort of action, pre-warm the 
                    // effect if possible, or we can look into holding/priming emitters with 
                    // components such as these.
                    lowPassFilterComponent = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
                }

                lowPassFilterComponent.enabled = true;
                
                lowPassFilterComponent.cutoffFrequency = CutoffFrequency;
                lowPassFilterComponent.lowpassResonanceQ = ResonanceQ;
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(LowPassFilter))]
        public class BlipActionLowPassFilterInspector : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y, position.width, 20f), 
                    property.FindPropertyRelative("CutoffFrequency"), 
                    new GUIContent("Cutoff Frequency ")
                );

                EditorGUI.PropertyField
                (
                    new Rect(position.x, position.y + 20f, position.width, 20f), 
                    property.FindPropertyRelative("ResonanceQ"), 
                    new GUIContent("Resonance Q ")
                );

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                SerializedProperty cutoffFrequencyProperty = property.FindPropertyRelative("CutoffFrequency");
                SerializedProperty resonanceQProperty = property.FindPropertyRelative("ResonanceQ");

                float totalHeight = 0;
                totalHeight += EditorGUI.GetPropertyHeight(cutoffFrequencyProperty);
                totalHeight += EditorGUI.GetPropertyHeight(resonanceQProperty);

                return totalHeight;
            }
        }
#endif
    }
}
