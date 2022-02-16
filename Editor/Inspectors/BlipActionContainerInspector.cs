using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Blip
{
    [CustomPropertyDrawer(typeof(BlipActionContainer))]
    public class BlipActionContainerInspector : PropertyDrawer
    {
        private int actionType;
        private float drawerHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty actionTypeProperty = property.FindPropertyRelative("Type");

            float totalHeight = 0f;

            switch ((BlipActionContainer.ActionType)actionTypeProperty.intValue)
            {
                case BlipActionContainer.ActionType.PlayClip:
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsPlayClip"));
                    break;

                case BlipActionContainer.ActionType.PlayClipFromRandomSet:
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsPlayClipFromRandomSet"));
                    break;

                case BlipActionContainer.ActionType.GlobalStopEvent: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsGlobalStopEvent"));
                    break;

                case BlipActionContainer.ActionType.GlobalStopAllEvents: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsGlobalStopAllEvents"));
                    break;

                case BlipActionContainer.ActionType.HighPassFilter: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsHighPassFilter"));
                    break;

                case BlipActionContainer.ActionType.LowPassFilter: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsLowPassFilter"));
                    break;
            }

            return totalHeight +30f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty actionTypeProperty = property.FindPropertyRelative("Type");

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            string[] actionTypeNames = actionTypeProperty.enumDisplayNames;
            actionTypeNames[0] = "<Choose an action>";

            actionType = EditorGUI.Popup
            (
                new Rect(position.x, position.y, position.width, 20f), 
                actionTypeProperty.intValue, 
                actionTypeNames
            );

            if( EditorGUI.EndChangeCheck() )
            {
                actionTypeProperty.intValue = actionType;
            }

            switch ((BlipActionContainer.ActionType)actionTypeProperty.intValue)
            {
                case BlipActionContainer.ActionType.PlayClip:
                    Draw_PlayClip(position, property);
                    break;

                case BlipActionContainer.ActionType.PlayClipFromRandomSet:
                    Draw_PlayClipFromRandomSet(position, property);
                    break;

                case BlipActionContainer.ActionType.GlobalStopEvent:
                    Draw_GlobalStopEvent(position, property);
                    break;

                case BlipActionContainer.ActionType.GlobalStopAllEvents:
                    Draw_GlobalStopAllEvents(position, property);
                    break;

                case BlipActionContainer.ActionType.HighPassFilter:
                    Draw_HighPassFilter(position, property);
                    break;

                case BlipActionContainer.ActionType.LowPassFilter:
                    Draw_LowPassFilter(position, property);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private void Draw_PlayClip(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsPlayClip"), 
                GUIContent.none
            );
        }

        private void Draw_PlayClipFromRandomSet(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsPlayClipFromRandomSet"), 
                GUIContent.none
            );
        }

        private void Draw_GlobalStopEvent(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsGlobalStopEvent"), 
                GUIContent.none
            );
        }

        private void Draw_GlobalStopAllEvents(Rect position, SerializedProperty property)
        {
            position.height += 10f;
        }

        private void Draw_HighPassFilter(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsHighPassFilter"), 
                GUIContent.none
            );
        }

        private void Draw_LowPassFilter(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsLowPassFilter"), 
                GUIContent.none
            );
        }
    }
}
