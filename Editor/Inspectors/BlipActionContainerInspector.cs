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
                case BlipActionContainer.ActionType.Play:
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsPlay"));
                    break;

                case BlipActionContainer.ActionType.PlayFromRandomSet:
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsPlayFromRandomSet"));
                    break;

                case BlipActionContainer.ActionType.Stop: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsStop"));
                    break;

                case BlipActionContainer.ActionType.StopAllInEvent: 
                    totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OptionsStopAllInEvent"));
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
                case BlipActionContainer.ActionType.Play:
                    Draw_Play(position, property);
                    break;

                case BlipActionContainer.ActionType.PlayFromRandomSet:
                    Draw_PlayFromRandomSet(position, property);
                    break;

                case BlipActionContainer.ActionType.Stop:
                    Draw_Stop(position, property);
                    break;

                case BlipActionContainer.ActionType.StopAllInEvent:
                    Draw_StopAllInEvent(position, property);
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

        private void Draw_Play(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsPlay"), 
                GUIContent.none
            );
        }

        private void Draw_PlayFromRandomSet(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsPlayFromRandomSet"), 
                GUIContent.none
            );
        }

        private void Draw_Stop(Rect position, SerializedProperty property)
        {
            position.height += 40f;

            EditorGUI.PropertyField
            (
                new Rect(position.x, position.y + 20f, position.width, 20f), 
                property.FindPropertyRelative("OptionsStop"), 
                GUIContent.none
            );
        }

        private void Draw_StopAllInEvent(Rect position, SerializedProperty property)
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
