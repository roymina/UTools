using System;
using System.Collections;
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
            FindChildrenCollections(cachedFields.ChildrenFields);
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
                    fields.Where(field => Attribute.IsDefined(field, typeof(ChildrenAttribute))).ToArray(),
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
                children = gameObject.GetAllDescendants();
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

        private void FindChildrenCollections(FieldInfo[] fields)
        {
            if (fields.Length == 0 || transform.childCount == 0)
            {
                return;
            }

            if (children == null || children.Length == 0)
            {
                children = gameObject.GetAllDescendants();
            }

            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(this) != null)
                {
                    continue;
                }

                if (!TryCreateChildrenCollection(field, out Type elementType, out IList values))
                {
                    Debug.LogError($"ChildrenAttribute on {field.Name} must target List<GameObject> or List<Component>");
                    continue;
                }

                ChildrenAttribute childrenAttr = field.GetCustomAttribute<ChildrenAttribute>();
                string parentName = string.IsNullOrEmpty(childrenAttr.parentName) ? field.Name : childrenAttr.parentName;
                GameObject parent = ResolveChildTarget(parentName);
                if (parent == null)
                {
                    Debug.LogError($"GameObject '{parentName}' not found under {gameObject.name}");
                    continue;
                }

                IEnumerable<GameObject> targets = EnumerateChildren(parent, childrenAttr.includeDescendants, childrenAttr.includeInactive);
                if (elementType == typeof(GameObject))
                {
                    foreach (GameObject target in targets)
                    {
                        values.Add(target);
                    }
                }
                else
                {
                    foreach (GameObject target in targets)
                    {
                        Component component = target.GetComponent(elementType);
                        if (component != null)
                        {
                            values.Add(component);
                        }
                    }
                }

                field.SetValue(this, values);
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

        private static bool TryCreateChildrenCollection(FieldInfo field, out Type elementType, out IList values)
        {
            elementType = null;
            values = null;

            if (!field.FieldType.IsGenericType || field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
            {
                return false;
            }

            elementType = field.FieldType.GetGenericArguments()[0];
            if (elementType != typeof(GameObject) && !typeof(Component).IsAssignableFrom(elementType))
            {
                return false;
            }

            values = (IList)Activator.CreateInstance(field.FieldType);
            return true;
        }

        private static IEnumerable<GameObject> EnumerateChildren(GameObject parent, bool includeDescendants, bool includeInactive)
        {
            IEnumerable<GameObject> targets = includeDescendants
                ? parent.GetAllDescendants()
                : parent.GetDirectChildren();

            if (!includeInactive)
            {
                targets = targets.Where(child => child.activeInHierarchy);
            }

            return targets;
        }

        private sealed class CachedFields
        {
            public CachedFields(FieldInfo[] compFields, FieldInfo[] childFields, FieldInfo[] childrenFields, FieldInfo[] resourceFields)
            {
                CompFields = compFields;
                ChildFields = childFields;
                ChildrenFields = childrenFields;
                ResourceFields = resourceFields;
            }

            public FieldInfo[] CompFields { get; }
            public FieldInfo[] ChildFields { get; }
            public FieldInfo[] ChildrenFields { get; }
            public FieldInfo[] ResourceFields { get; }
        }
    }
}
