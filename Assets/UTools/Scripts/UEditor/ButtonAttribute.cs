//-----------------------------------------------------------------------
// <copyright file="ButtonAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Attribute for adding buttons to the Unity Inspector and a custom editor to render them.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTools
{
    // Defines the ButtonAttribute
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string ButtonName;

        public ButtonAttribute() : this(null) { }

        public ButtonAttribute(string buttonName)
        {
            ButtonName = buttonName;
        }
    }

#if UNITY_EDITOR
    // Custom editor drawer for ButtonAttribute
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Retrieves all methods of the current script
            var methods = target.GetType().GetMethods(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                // Checks if a method has the ButtonAttribute
                var buttonAttribute = (ButtonAttribute)Attribute.GetCustomAttribute(
                    method, typeof(ButtonAttribute));

                if (buttonAttribute != null)
                {
                    // Creates a button and invokes the method when clicked
                    string buttonName = string.IsNullOrEmpty(buttonAttribute.ButtonName)
                        ? method.Name
                        : buttonAttribute.ButtonName;

                    if (GUILayout.Button(buttonName))
                    {
                        method.Invoke(target, null);
                    }
                }
            }
        }
    }
#endif
}
