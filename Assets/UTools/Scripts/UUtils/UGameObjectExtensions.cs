using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools
{
    public static class UGameObjectExtensions
    {
        public static GameObject FindChild(
            this GameObject self,
            string searchText,
            bool allDescendants = true,
            bool fuzzySearch = false)
        {
            if (self == null || string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            IEnumerable<Transform> children = allDescendants
                ? self.transform.EnumerateDescendantTransforms()
                : self.transform.EnumerateDirectChildTransforms();

            foreach (Transform child in children)
            {
                bool isMatch = fuzzySearch
                    ? child.name.IndexOf(searchText, StringComparison.Ordinal) >= 0
                    : child.name.Equals(searchText, StringComparison.Ordinal);

                if (isMatch)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        public static GameObject FindChild(this GameObject self, bool allDescendants = true, params string[] searchTexts)
        {
            if (self == null || searchTexts == null || searchTexts.Length == 0)
            {
                return null;
            }

            HashSet<string> targetNames = new(searchTexts.Where(name => !string.IsNullOrEmpty(name)));
            if (targetNames.Count == 0)
            {
                return null;
            }

            IEnumerable<Transform> children = allDescendants
                ? self.transform.EnumerateDescendantTransforms()
                : self.transform.EnumerateDirectChildTransforms();

            Transform target = children.FirstOrDefault(child => targetNames.Contains(child.name));
            return target == null ? null : target.gameObject;
        }

        public static GameObject[] GetAllDescendants(this GameObject self)
        {
            if (self == null)
            {
                return Array.Empty<GameObject>();
            }

            return self.transform
                .EnumerateDescendantTransforms()
                .Select(child => child.gameObject)
                .ToArray();
        }

        public static List<GameObject> GetFirstLevelChildren(this GameObject self)
        {
            return self.GetDirectChildren();
        }

        public static List<GameObject> GetDirectChildren(this GameObject self)
        {
            if (self == null)
            {
                return new List<GameObject>();
            }

            return self.transform
                .EnumerateDirectChildTransforms()
                .Select(child => child.gameObject)
                .ToList();
        }

        public static bool ExistsChild(this GameObject self, string searchText, bool fuzzySearch = false)
        {
            return self.FindChild(searchText, true, fuzzySearch) != null;
        }

        public static GameObject ShowOnlyDescendantNamed(this GameObject self, string childName)
        {
            GameObject target = self.FindChild(childName, true);
            return self.ShowOnlyDescendant(target);
        }

        public static GameObject HideDescendantNamed(this GameObject self, string childName)
        {
            GameObject target = self.FindChild(childName, true);
            return self.HideDescendant(target);
        }

        public static GameObject ShowOnlyDirectChildAt(this GameObject self, int childIndex)
        {
            if (!self.TryGetDirectChild(childIndex, out Transform targetChild))
            {
                return null;
            }

            for (int i = 0; i < self.transform.childCount; i++)
            {
                Transform child = self.transform.GetChild(i);
                child.gameObject.SetActive(child == targetChild);
            }

            return targetChild.gameObject;
        }

        public static GameObject HideDirectChildAt(this GameObject self, int childIndex)
        {
            if (!self.TryGetDirectChild(childIndex, out Transform targetChild))
            {
                return null;
            }

            for (int i = 0; i < self.transform.childCount; i++)
            {
                Transform child = self.transform.GetChild(i);
                child.gameObject.SetActive(child != targetChild);
            }

            return targetChild.gameObject;
        }

        public static GameObject ShowOnlyDescendant(this GameObject self, GameObject target)
        {
            if (self == null || target == null || !target.transform.IsChildOf(self.transform))
            {
                return null;
            }

            foreach (GameObject child in self.GetAllDescendants())
            {
                child.SetActive(child == target);
            }

            return target;
        }

        public static GameObject HideDescendant(this GameObject self, GameObject target)
        {
            if (self == null || target == null || !target.transform.IsChildOf(self.transform))
            {
                return null;
            }

            target.SetActive(false);
            return target;
        }

        public static void ToggleAllChildren(this GameObject self, bool show)
        {
            if (self == null)
            {
                return;
            }

            foreach (GameObject child in self.GetAllDescendants())
            {
                child.SetActive(show);
            }
        }

        public static GameObject FindNearestObject<T>(this GameObject self, IEnumerable<T> candidates)
            where T : Component
        {
            if (self == null || candidates == null)
            {
                return null;
            }

            float closestSqrDistance = Mathf.Infinity;
            GameObject closestObject = null;
            Vector3 origin = self.transform.position;

            foreach (T candidate in candidates)
            {
                if (candidate == null || candidate.gameObject == self)
                {
                    continue;
                }

                float sqrDistance = (candidate.transform.position - origin).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestObject = candidate.gameObject;
                }
            }

            return closestObject;
        }

        public static GameObject FindParent(this GameObject self, string name)
        {
            if (self == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            Transform current = self.transform.parent;
            while (current != null)
            {
                if (current.name.Equals(name, StringComparison.Ordinal))
                {
                    return current.gameObject;
                }

                current = current.parent;
            }

            return null;
        }

        public static T FindComponentInParent<T>(this GameObject self, string name = null)
            where T : Component
        {
            if (self == null)
            {
                return null;
            }

            Transform current = self.transform.parent;
            while (current != null)
            {
                if (name == null || current.name.Equals(name, StringComparison.Ordinal))
                {
                    T component = current.GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }

                current = current.parent;
            }

            return null;
        }

        public static bool IsActiveAndMeshEnabled(this GameObject self)
        {
            return self != null
                && self.activeInHierarchy
                && self.TryGetComponent(out MeshRenderer meshRenderer)
                && meshRenderer.enabled;
        }

        public static bool HasComponent<T>(this GameObject self)
            where T : Component
        {
            return self != null && self.TryGetComponent(out T _);
        }

        public static T EnsureComponent<T>(this GameObject self)
            where T : Component
        {
            if (self == null)
            {
                return null;
            }

            return self.TryGetComponent(out T component)
                ? component
                : self.AddComponent<T>();
        }

        public static T GetComponentInSelfThenParent<T>(this GameObject self)
            where T : Component
        {
            if (self == null)
            {
                return null;
            }

            return self.TryGetComponent(out T component)
                ? component
                : self.GetComponentInParent<T>();
        }

        public static Transform FindChildByName(this Transform self, string searchText)
        {
            if (self == null || string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            return self
                .EnumerateDescendantTransforms()
                .FirstOrDefault(child => child.name.Equals(searchText, StringComparison.Ordinal));
        }

        public static void ToggleBlink(this GameObject self, bool blink)
        {
            if (self == null)
            {
                return;
            }

            Highlighter highlighter = self.EnsureComponent<Highlighter>();
            if (blink)
            {
                highlighter.StartHighlight();
            }
            else
            {
                highlighter.StopHighlight();
            }
        }

        public static bool SetLayer(this GameObject self, string layerName)
        {
            if (self == null || string.IsNullOrEmpty(layerName))
            {
                return false;
            }

            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogError($"Layer '{layerName}' not found.");
                return false;
            }

            self.layer = layer;
            return true;
        }

        public static bool SetLayerRecursively(this GameObject self, string layerName)
        {
            if (!self.SetLayer(layerName))
            {
                return false;
            }

            foreach (GameObject child in self.GetAllDescendants())
            {
                child.layer = self.layer;
            }

            return true;
        }

        private static bool TryGetDirectChild(this GameObject self, int childIndex, out Transform child)
        {
            child = null;
            if (self == null || childIndex < 0 || childIndex >= self.transform.childCount)
            {
                return false;
            }

            child = self.transform.GetChild(childIndex);
            return true;
        }

        internal static IEnumerable<Transform> EnumerateDirectChildTransforms(this Transform self)
        {
            if (self == null)
            {
                yield break;
            }

            for (int i = 0; i < self.childCount; i++)
            {
                yield return self.GetChild(i);
            }
        }

        internal static IEnumerable<Transform> EnumerateDescendantTransforms(this Transform self)
        {
            if (self == null)
            {
                yield break;
            }

            Stack<Transform> stack = new();
            for (int i = self.childCount - 1; i >= 0; i--)
            {
                stack.Push(self.GetChild(i));
            }

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();
                yield return current;

                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
        }
    }
}
