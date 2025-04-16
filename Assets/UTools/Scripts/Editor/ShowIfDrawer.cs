using UnityEngine;
using UnityEditor;
using System.Reflection;

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
                Debug.LogError($"条件字段 {showIf.ConditionField} 未找到");
                return true;
            }

            bool conditionValue = false;

            // 根据字段类型获取值
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
                    Debug.LogError($"不支持的条件字段类型: {conditionProperty.propertyType}");
                    return true;
            }

            return showIf.Inverse ? conditionValue : !conditionValue;
        }
    }
}