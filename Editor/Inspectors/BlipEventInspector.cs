using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Blip
{
    [CustomEditor(typeof(BlipEvent))]
    public class EventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BlipEvent targetEvent = (BlipEvent)target;

            serializedObject.Update();

            if(GUILayout.Button("Hear It"))
            {
                EditorStatics.PlayEventInPreviewAudioSource(targetEvent);   
            }

            if (EditorStatics.IsPlaying)
            {
                if(GUILayout.Button("Stop"))
                {
                    EditorStatics.StopPreviewAudioSource();
                }
                GUILayout.Space(5f);
            }
            else
            {
                GUILayout.Space(26f);
            }

            DrawDefaultInspector();
        }
    }
}
