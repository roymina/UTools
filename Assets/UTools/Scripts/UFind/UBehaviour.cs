using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UTools
{
    public class UBehaviour : MonoBehaviour
    {
        private static readonly Dictionary<Type, CachedFields> CachedFieldsByType = new();

        protected GameObject[] children;

        protected virtual void Awake()
        {
            CachedFields cachedFields = GetCachedFields(GetType());
            GetComp(cachedFields.CompFields);
            FindChildren(cachedFields.ChildFields);
            GetResources(cachedFields.ResourceFields);
        }

        private static CachedFields GetCachedFields(Type type)
        {
            if (!CachedFieldsByType.TryGetValue(type, out CachedFields cachedFields))
            {
                FieldInfo[] fields = ReflectionCache.GetInstanceFields(type);
                cachedFields = new CachedFields(
                    fields.Where(field => Attribute.IsDefined(field, typeof(CompAttribute))).ToArray(),
                    fields.Where(field => Attribute.IsDefined(field, typeof(ChildAttribute))).ToArray(),
                    fields.Where(field => Attribute.IsDefined(field, typeof(ResourceAttribute))).ToArray());
                CachedFieldsByType[type] = cachedFields;
            }

            return cachedFields;
        }

        private void GetResources(FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(this) != null)
                {
                    continue;
                }

                ResourceAttribute resourceAttr = field.GetCustomAttribute<ResourceAttribute>();
                string path = string.IsNullOrEmpty(resourceAttr.Path) ? field.Name : resourceAttr.Path;
                UnityEngine.Object resource = Resources.Load(path, field.FieldType);
                if (resource != null)
                {
                    field.SetValue(this, resource);
                }
                else
                {
                    Debug.LogWarning($"Failed to load resource at {path}");
                }
            }
        }

        private void GetComp(FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(this) != null)
                {
                    continue;
                }

                Type fieldType = field.FieldType;
                Component component = GetComponent(fieldType);
                if (component != null)
                {
                    field.SetValue(this, component);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} cannot find component {fieldType}");
                }
            }
        }

        private void FindChildren(FieldInfo[] fields)
        {
            if (fields.Length == 0 || transform.childCount == 0)
            {
                return;
            }

            if (children == null || children.Length == 0)
            {
                children = UUtils.GetAllDecendents(gameObject);
            }

            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(this) != null)
                {
                    continue;
                }

                ChildAttribute childAttr = field.GetCustomAttribute<ChildAttribute>();
                string targetName = string.IsNullOrEmpty(childAttr.Name) ? field.Name : childAttr.Name;
                GameObject target = ResolveChildTarget(targetName);
                if (target == null)
                {
                    Debug.LogError($"GameObject '{targetName}' not found under {gameObject.name}");
                    continue;
                }

                if (field.FieldType == typeof(GameObject))
                {
                    field.SetValue(this, target);
                    continue;
                }

                Component component = target.GetComponent(field.FieldType);
                if (component != null)
                {
                    field.SetValue(this, component);
                }
                else
                {
                    Debug.LogError($"Component {field.FieldType} not found on {target.name}");
                }
            }
        }

        private GameObject ResolveChildTarget(string targetName)
        {
            if (targetName.Contains("/"))
            {
                Transform byPath = transform.Find(targetName);
                return byPath != null ? byPath.gameObject : null;
            }

            GameObject[] matches = children
                .Where(child => child != null && child.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matches.Length > 1)
            {
                Debug.LogError($"Multiple GameObjects named '{targetName}' found under {gameObject.name}, please use a path such as Parent/{targetName}.");
                return null;
            }

            return matches.FirstOrDefault();
        }

        private sealed class CachedFields
        {
            public CachedFields(FieldInfo[] compFields, FieldInfo[] childFields, FieldInfo[] resourceFields)
            {
                CompFields = compFields;
                ChildFields = childFields;
                ResourceFields = resourceFields;
            }

            public FieldInfo[] CompFields { get; }
            public FieldInfo[] ChildFields { get; }
            public FieldInfo[] ResourceFields { get; }
        }
    }
}
