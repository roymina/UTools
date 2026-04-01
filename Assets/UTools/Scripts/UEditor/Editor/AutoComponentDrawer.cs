//-----------------------------------------------------------------------
// <copyright file="AutoComponentDrawer.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Custom property drawer for the AutoComponentAttribute to automatically add or remove components in the Unity Editor.
// </summary>
//-----------------------------------------------------------------------

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UTools.Editor
{
    [CustomPropertyDrawer(typeof(AutoComponentAttribute))]
    public class AutoComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            if (Application.isPlaying)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AutoComponentAttribute attr = (AutoComponentAttribute)attribute;

            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(position, label, property.boolValue);

            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = value;
                property.serializedObject.ApplyModifiedProperties();
                MonoBehaviour target = property.serializedObject.targetObject as MonoBehaviour;

                if (target != null)
                {
                    if (value)
                    {
                        // Adds all specified components
                        foreach (var type in attr.ComponentTypes)
                        {
                            if (target.GetComponent(type) == null)
                            {
                                Undo.AddComponent(target.gameObject, type);
                            }
                        }
                    }
                    else
                    {
                        // Removes all specified components
                        foreach (var type in attr.ComponentTypes)
                        {
                            Component comp = target.GetComponent(type);
                            if (comp != null)
                            {
                                Undo.DestroyObjectImmediate(comp);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif
