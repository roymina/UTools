//-----------------------------------------------------------------------
// <copyright file="UBehaviour.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Base MonoBehaviour class for automatically finding components, children, and resources.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UTools
{
    public class UBehaviour : MonoBehaviour
    {
        protected GameObject[] children;

        protected virtual void Awake()
        {
            GetComp();
            FindChildren();
            GetResources();
        }

        private void GetResources()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => Attribute.IsDefined(field, typeof(ResourceAttribute)));

            foreach (var field in fields)
            {
                var resourceAttr = field.GetCustomAttribute<ResourceAttribute>();
                string path = string.IsNullOrEmpty(resourceAttr.Path) ? field.Name : resourceAttr.Path;
                var resource = Resources.Load(path, field.FieldType);
                if (resource != null) field.SetValue(this, resource);
                else Debug.LogWarning($"Failed to load resource at {path}");
            }
        }

        private void GetComp()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => Attribute.IsDefined(field, typeof(CompAttribute)));

            foreach (var field in fields)
            {
                if (field.GetValue(this) == null)
                {
                    var compAttr = field.GetCustomAttribute<CompAttribute>();
                    Type fieldType = field.FieldType;
                    var component = GetComponent(fieldType);
                    if (component != null) field.SetValue(this, component);
                    else Debug.LogWarning($"{gameObject.name} cannot find component {fieldType}");
                }
            }
        }

        private void FindChildren()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => Attribute.IsDefined(field, typeof(ChildAttribute)));

            if (!fields.Any() || transform.childCount == 0) return;
            if (children == null || children.Length == 0) children = UUtils.GetAllDecendents(gameObject);

            foreach (var field in fields)
            {
                if (field.GetValue(this) != null) continue;
                var childAttr = field.GetCustomAttribute<ChildAttribute>();
                string targetName = string.IsNullOrEmpty(childAttr.Name) ? field.Name : childAttr.Name;

                var target = children.FirstOrDefault(x => x.name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                if (target != null)
                {
                    if (field.FieldType == typeof(GameObject))
                        field.SetValue(this, target);
                    else
                    {
                        var component = target.GetComponent(field.FieldType);
                        if (component != null) field.SetValue(this, component);
                        else Debug.LogError($"Component {field.FieldType} not found on {target.name}");
                    }
                }
                else Debug.LogError($"GameObject '{targetName}' not found under {gameObject.name}");
            }
        }
    }
}
