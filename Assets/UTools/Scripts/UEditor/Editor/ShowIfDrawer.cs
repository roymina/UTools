//-----------------------------------------------------------------------
// <copyright file="ShowIfDrawer.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Custom property drawer for the ShowIfAttribute to conditionally display fields in the Unity Inspector.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace UTools.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool enabled = ShouldShowProperty(showIf, property);

            if (enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool enabled = ShouldShowProperty(showIf, property);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return 0f;
            }
        }

        private bool ShouldShowProperty(ShowIfAttribute showIf, SerializedProperty property)
        {
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

            if (conditionProperty == null)
            {
                // Logs an error if the condition field is not found
                Debug.LogError($"Condition field {showIf.ConditionField} not found");
                return true;
            }

            bool conditionValue = false;

            // Retrieves the value of the condition field based on its type
            switch (conditionProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    conditionValue = conditionProperty.boolValue;
                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                    conditionValue = conditionProperty.floatValue != 0;
                    break;
                case SerializedPropertyType.String:
                    conditionValue = !string.IsNullOrEmpty(conditionProperty.stringValue);
                    break;
                case SerializedPropertyType.ObjectReference:
                    conditionValue = conditionProperty.objectReferenceValue != null;
                    break;
                default:
                    // Logs an error if the condition field type is unsupported
                    Debug.LogError($"Unsupported condition field type: {conditionProperty.propertyType}");
                    return true;
            }

            return showIf.Inverse ? conditionValue : !conditionValue;
        }
    }
}