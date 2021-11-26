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
                // TODO
                Debug.Log("Not yet implemented.");
                //EditorStatics.PlayClipInPreviewAudioSource();    
            }

            GUILayout.Space(10f);

            DrawDefaultInspector();
        }
    }
}
