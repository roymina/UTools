using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
#endif

namespace UTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string ButtonName;

        public ButtonAttribute() : this(null)
        {
        }

        public ButtonAttribute(string buttonName)
        {
            ButtonName = buttonName;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<Type, MethodInfo[]> CachedMethods = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (MethodInfo method in GetButtonMethods(target.GetType()))
            {
                ButtonAttribute buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
                string buttonName = string.IsNullOrEmpty(buttonAttribute.ButtonName)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : buttonAttribute.ButtonName;

                if (GUILayout.Button(buttonName))
                {
                    foreach (UnityEngine.Object currentTarget in targets)
                    {
                        try
                        {
                            method.Invoke(currentTarget, null);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                    }
                }
            }
        }

        private static MethodInfo[] GetButtonMethods(Type type)
        {
            if (!CachedMethods.TryGetValue(type, out MethodInfo[] methods))
            {
                methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(method => method.GetCustomAttribute<ButtonAttribute>() != null && method.GetParameters().Length == 0)
                    .ToArray();
                CachedMethods[type] = methods;
            }

            return methods;
        }
    }
#endif
}
