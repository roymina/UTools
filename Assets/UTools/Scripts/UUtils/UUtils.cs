//-----------------------------------------------------------------------
// <copyright file="UUtils.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// General utility functions
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UTools
{
    public static class UUtils
    {
        #region Common
        /// <summary>
        /// Maps a value from one range to another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromMin">The minimum value of the original range.</param>
        /// <param name="fromMax">The maximum value of the original range.</param>
        /// <param name="toMin">The minimum value of the target range.</param>
        /// <param name="toMax">The maximum value of the target range.</param>
        /// <returns>The mapped value.</returns>
        public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // Ensure the value is within the original range
            value = Mathf.Clamp(value, fromMin, fromMax);

            // Map the value to the new range
            float mappedValue = (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;

            return mappedValue;
        }
        #endregion

        #region Strings

        /// <summary>
        /// Checks if the string is null or empty.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>True if the string is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Checks if the string is not null or empty.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>True if the string is not null or empty; otherwise, false.</returns>
        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Checks if the input string consists only of letters, digits, and underscores.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the string meets the criteria; otherwise, false.</returns>
        public static bool CheckUserName(this string input)
        {
            return Regex.IsMatch(input, @"^[a-zA-Z0-9_][A-Za-z0-9_]*$");
        }

        /// <summary>
        /// Trims the string to a specified length.
        /// </summary>
        /// <param name="self">The input string.</param>
        /// <param name="length">The desired length.</param>
        /// <returns>The trimmed string with an ellipsis if truncated.</returns>
        public static string TrimLength(this string self, int length)
        {
            if (length > self.Length) return self;
            string result = $"{self.Substring(0, length - 3)}...";
            return result;
        }

        /// <summary>
        /// Checks if the input string consists only of Chinese characters.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the string contains only Chinese characters; otherwise, false.</returns>
        public static bool CheckStrChinese(this string input)
        {
            return Regex.IsMatch(input, @"^[\u4e00-\u9fa5]*$");
        }
        /// <summary>
        /// Checks if the input string is a valid IP address using regex.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsIPAddress(this string input)
        {
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                             @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                             @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                             @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            return !string.IsNullOrEmpty(input) &&
                   Regex.IsMatch(input.Trim(), pattern);
        }

        /// <summary>
        /// Converts the string to a Base64 string.
        /// </summary>
        /// <param name="self">The input string.</param>
        /// <returns>The Base64-encoded string.</returns>
        public static string ToBase64String(this string self)
        {
            if (string.IsNullOrEmpty(self)) return "";
            byte[] bytes = Encoding.UTF8.GetBytes(self);
            return Convert.ToBase64String(bytes);
        }
        #endregion

        #region  Date & Time
        /// <summary>
        /// Converts an integer (seconds) to a time string.
        /// </summary>
        /// <param name="seconds">The number of seconds.</param>
        /// <param name="useChinese">Whether to use Chinese format.</param>
        /// <param name="format">Custom time format (e.g., "hh:mm:ss").</param>
        /// <returns>The formatted time string.</returns>
        public static string ToTimeString(this int seconds, bool useChinese = false, string format = null)
        {
            string result = string.Empty;
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.Hours > 0)
            {
                result = time.ToString(format == null ? useChinese ? @"hh\小时mm\分ss\秒" : @"hh\:mm\:ss" : format);
            }
            else
            {
                result = time.ToString(format == null ? useChinese ? @"mm\分ss\秒" : @"mm\:ss" : format);
            }

            return result;
        }

        /// <summary>
        /// Converts a TimeSpan to a formatted string.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan object.</param>
        /// <param name="useChinese">Whether to use Chinese format.</param>
        /// <returns>The formatted time string.</returns>
        public static string Tohhmmss(this TimeSpan timeSpan, bool useChinese = false)
        {
            string result = string.Empty;
            if (!useChinese)
            {
                result = timeSpan.ToString(timeSpan.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");
            }
            else
            {
                result = timeSpan.ToString(timeSpan.Hours > 0 ? @"hh\小时mm\分ss\秒" : @"mm\分ss\秒");
            }

            return result;
        }

        /// <summary>
        /// Calculates the TimeSpan between two time strings.
        /// </summary>
        /// <param name="startTimeStr">The start time string.</param>
        /// <param name="endTimeStr">The end time string.</param>
        /// <returns>The TimeSpan difference.</returns>
        public static TimeSpan CalculateTimeSpan(this string startTimeStr, string endTimeStr)
        {
            try
            {
                DateTime startTime = DateTime.Parse(startTimeStr);
                DateTime endTime = DateTime.Parse(endTimeStr);
                TimeSpan timeDifference = endTime - startTime;
                return timeDifference;
            }
            catch (System.Exception)
            {
                return TimeSpan.FromSeconds(0);
            }
        }
        public static string ToLocalizedString(this Enum value)
        {
            System.Reflection.FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute)) as System.ComponentModel.DescriptionAttribute;
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return value.ToString(); // 如果没有标注，返回默认枚举名称
        }
        #endregion
        #region File & Data
        /// <summary>
        /// Reads file content from the persistent data path.
        /// </summary>
        /// <param name="filename">The file name, with multi-level paths separated by commas.</param>
        /// <param name="createNewIfNotExist">Whether to create a new file if it does not exist.</param>
        /// <returns>The file content as a string, or null if the file does not exist.</returns>
        public static string ReadFromPersistDataPath(string filename, bool createNewIfNotExist = true)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogWarning("Filename is null or empty.");
                return string.Empty;
            }
            string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filename);


            if (!System.IO.File.Exists(fullPath))
            {
                if (createNewIfNotExist)
                {
                    System.IO.File.Create(fullPath).Dispose();
                    Debug.Log($"File not found: {fullPath}, creating a new file.");
                }
                else
                {
                    Debug.LogWarning($"File not found: {fullPath}");
                }

                return string.Empty;
            }
            return System.IO.File.ReadAllText(fullPath);
        }

        public static void WriteToPersistDataPath(string content, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogWarning("Filename is null or empty.");
                return;
            }
            if (content == null)
            {
                Debug.LogWarning("Content is null.");
                return;
            }
            string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            if (!System.IO.File.Exists(fullPath))
            {
                System.IO.File.Create(fullPath).Dispose();
            }
            System.IO.File.WriteAllText(fullPath, content);
        }
        /// <summary>
        /// get the project root folder path
        /// </summary>
        /// <returns></returns>
        public static string GetProjectRootFolder()
        {

            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
            return projectRoot;
        }
        #endregion
        #region GameObject & Components 
        /// <summary>
        /// Finds a child GameObject by name.
        /// </summary>
        /// <param name="self">The parent GameObject.</param>
        /// <param name="searchText">The name of the child.</param>
        /// <param name="allDecendents">Whether to search all descendants.</param>
        /// <param name="fuzzySearch">Whether to perform a fuzzy search.</param>
        /// <returns>The child GameObject, or null if not found.</returns>
        public static GameObject FindChild(this GameObject self, string searchText, bool allDescendants = true, bool fuzzySearch = false)
        {
            Transform[] children = allDescendants ? self.GetComponentsInChildren<Transform>(true) : self.transform.Cast<Transform>().ToArray();

            foreach (var child in children)
            {
                if (fuzzySearch ? child.name.Contains(searchText) : child.name == searchText)
                    return child.gameObject;
            }

            Debug.LogWarning($"{self.name} has no matching children.");
            return null;
        }


        /// <summary>
        /// Finds a child GameObject by name (multiple possible names).
        /// </summary>
        /// <param name="self">The parent GameObject.</param>
        /// <param name="allDecendents">Whether to search all descendants.</param>
        /// <param name="searchTexts">The possible names of the child.</param>
        /// <returns>The child GameObject, or null if not found.</returns>
        public static GameObject FindChild(this GameObject self, bool allDescendants = true, params string[] searchTexts)
        {
            if (searchTexts == null || searchTexts.Length == 0)
            {
                Debug.LogWarning("Search texts are empty.");
                return null;
            }

            Transform[] children = allDescendants ? self.GetComponentsInChildren<Transform>(true) : self.transform.Cast<Transform>().ToArray();

            foreach (var child in children)
            {
                if (searchTexts.Contains(child.name))
                    return child.gameObject;
            }

            Debug.LogWarning($"{self.name} has no matching children.");
            return null;
        }
        /// <summary>
        /// get all children & decendents, includs deactived
        /// </summary>
        /// <returns></returns>
        public static GameObject[] GetAllDecendents(this GameObject self)
        {
            Transform[] childTransforms = self.GetComponentsInChildren<Transform>(true);
            return Array.FindAll(childTransforms, transform => transform.gameObject != self)
                        .Select(transform => transform.gameObject)
                        .ToArray();
        }

        // 获取一级子物体的列表
        public static List<GameObject> GetFirstLevelChildren(this GameObject self)
        {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < self.transform.childCount; i++)
            {
                children.Add(self.transform.GetChild(i).gameObject);
            }
            return children;
        }


        /// <summary>
        /// Checks if a child GameObject with the specified name exists.
        /// </summary>
        /// <param name="self">The parent GameObject.</param>
        /// <param name="searchText">The name of the child.</param>
        /// <param name="fuzzySearch">Whether to perform a fuzzy search.</param>
        /// <returns>True if the child exists; otherwise, false.</returns>
        public static bool ExistsChild(this GameObject self, string searchText, bool fuzzySearch = false)
        {
            return FindChild(self, searchText, fuzzySearch) != null;
        }
        /// <summary>
        /// Displays only one child GameObject.
        /// </summary>
        /// <param name="self">The parent GameObject.</param>
        /// <param name="childName">The name of the child to display.</param>
        /// <returns>The displayed child GameObject.</returns>
        public static GameObject ShowOneChild(this GameObject self, string childName)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            foreach (GameObject child in children)
            {
                child.SetActive(child.name == childName);
            }

            return children.FirstOrDefault(x => x.activeInHierarchy);
        }
        public static GameObject HideOneChild(this GameObject self, string childName)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            foreach (GameObject child in children)
            {
                child.SetActive(child.name != childName);
            }

            return children.FirstOrDefault(x => !x.activeInHierarchy);
        }

        public static GameObject ShowOneChild(this GameObject self, int childIndex)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            Transform targetChild = self.transform.GetChild(childIndex);
            if (!targetChild)
            {
                return null;
            }

            self.ToggleAllChildren(false);
            targetChild.gameObject.SetActive(true);
            return targetChild.gameObject;
        }
        public static GameObject HideOneChild(this GameObject self, int childIndex)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            Transform targetChild = self.transform.GetChild(childIndex);
            if (!targetChild)
            {
                return null;
            }

            self.ToggleAllChildren(true);
            targetChild.gameObject.SetActive(false);
            return targetChild.gameObject;
        }

        public static GameObject ShowOneChild(this GameObject self, GameObject childGo)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            foreach (GameObject child in children)
            {
                child.SetActive(child == childGo);
            }

            return children.FirstOrDefault(x => x.activeInHierarchy);
        }
        public static GameObject HideOneChild(this GameObject self, GameObject childGo)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return null;
            }

            foreach (GameObject child in children)
            {
                child.SetActive(child != childGo);
            }

            return children.FirstOrDefault(x => !x.activeInHierarchy);
        }
        /// <summary>
        /// Toggles the visibility of all children GameObjects.
        /// </summary>
        /// <param name="self">The parent GameObject.</param>
        /// <param name="show">Whether to show or hide the children.</param>
        public static void ToggleAllChildren(this GameObject self, bool show)
        {
            GameObject[] children = self.GetAllDecendents();
            if (!children.Any())
            {
                return;
            }

            foreach (GameObject child in children)
            {
                child.SetActive(show);
            }
        }


        /// <summary>
        /// Toggles the visibility of MeshRenderers and SkinnedMeshRenderers.
        /// </summary>
        /// <param name="self">The GameObject to toggle.</param>
        /// <param name="show">Whether to show or hide the meshes.</param>
        /// <param name="selfOnly">Whether to toggle only the GameObject itself or include its children.</param>
        public static void ToggleMesh(this GameObject self, bool show = false, bool selfOnly = false)
        {
            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            List<SkinnedMeshRenderer> skinedMeshRenderers = new List<SkinnedMeshRenderer>();
            if (selfOnly)
            {
                var comp = self.GetComponent<MeshRenderer>();
                if (comp != null)
                {
                    meshRenderers.Add(comp);
                }
                var skinComp = self.GetComponent<SkinnedMeshRenderer>();
                if (skinComp != null)
                {
                    skinedMeshRenderers.Add(skinComp);
                }
            }
            else
            {
                var comps = self.GetComponentsInChildren<MeshRenderer>(true);
                if (comps != null && comps?.Length > 0)
                {
                    meshRenderers.AddRange(comps);
                }

                var skinComps = self.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (skinComps != null && skinComps?.Length > 0)
                {
                    skinedMeshRenderers.AddRange(skinComps);
                }

            }

            if (meshRenderers.Count > 0)
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.enabled = show;
                }
            }

            if (skinedMeshRenderers.Count > 0)
            {
                foreach (SkinnedMeshRenderer meshRenderer in skinedMeshRenderers)
                {
                    meshRenderer.enabled = show;
                }
            }
        }

        /// <summary>
        /// Clones the mesh of the GameObject, optionally applying a new material and name.
        /// </summary>
        /// <param name="self">The GameObject to clone.</param>
        /// <param name="material">The material to apply to the cloned mesh. If null, the original material is used.</param>
        /// <param name="name">The name for the cloned GameObject. If null, a default name is used.</param>
        /// <returns>The cloned GameObject with the mesh.</returns>
        public static GameObject CloneMesh(this GameObject self, Material material = null, string name = null)
        {
            //TODO: SkinedMesh
            string n = name ?? $"{self.name}_Highliter";
            GameObject cloneMesh = new(n);
            if (self.GetComponent<MeshFilter>())
            {
                MeshFilter meshFilter = cloneMesh.AddComponent<MeshFilter>();
                meshFilter.mesh = self.GetComponent<MeshFilter>().mesh;
                Renderer ren = cloneMesh.AddComponent<MeshRenderer>();
                ren.material = material ?? ren.material;
            }
            else if (self.GetComponent<SkinnedMeshRenderer>())
            {
                MeshFilter meshFilter = cloneMesh.AddComponent<MeshFilter>();
                SkinnedMeshRenderer smr = self.GetComponent<SkinnedMeshRenderer>();
                meshFilter.mesh = smr.sharedMesh;
                Renderer ren = cloneMesh.AddComponent<MeshRenderer>();
                ren.material = material ?? ren.material;
            }

            cloneMesh.transform.SetParent(self.transform.parent);
            cloneMesh.transform.localPosition = self.transform.localPosition;
            cloneMesh.transform.localEulerAngles = self.transform.localEulerAngles;
            cloneMesh.transform.localScale = self.transform.localScale;
            cloneMesh.transform.SetParent(self.transform);
            return cloneMesh;
        }
        /// <summary>
        /// Combines the meshes of all child GameObjects into a single mesh.
        /// </summary>
        /// <param name="self">The parent GameObject containing the meshes to combine.</param>
        /// <param name="name">The name for the combined GameObject. If null, a default name is used.</param>
        /// <param name="parent">The parent Transform for the combined GameObject. If null, the original parent is used.</param>
        /// <returns>The GameObject with the combined mesh.</returns>
        public static GameObject CombineMesh(this GameObject self, string name = null, Transform parent = null)
        {
            // Get all child GameObjects of the current GameObject
            MeshFilter[] meshFilters = self.GetComponentsInChildren<MeshFilter>();
            if (meshFilters?.Length == 0)
            {
                Debug.LogError("No MeshFilter found");
                return self;
            }
            // Create a new list of CombineInstance to store the meshes of child GameObjects
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            // Iterate through the MeshFilters of child GameObjects and add them to the CombineInstance list
            foreach (MeshFilter meshFilter in meshFilters)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilter.sharedMesh;
                combineInstance.transform = meshFilter.transform.localToWorldMatrix;
                combineInstances.Add(combineInstance);
            }

            // Create a new Mesh to store the combined mesh
            Mesh combinedMesh = new Mesh();

            // Use the CombineMeshes function to combine all child GameObject meshes into one mesh
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            // Create a new GameObject to display the combined mesh
            GameObject combinedObject = new GameObject("CombinedMesh");
            MeshFilter combinedMeshFilter = combinedObject.AddComponent<MeshFilter>();
            combinedMeshFilter.sharedMesh = combinedMesh;
            MeshRenderer combinedMeshRenderer = combinedObject.AddComponent<MeshRenderer>();

            combinedObject.name = name ?? $"{self.name}_Combined";
            combinedObject.transform.SetParent(parent ?? self.transform.parent);
            combinedObject.transform.localScale = Vector3.one;
            return combinedObject;
        }



        /// <summary>
        /// Calculates the bounds of a MeshFilter attached to the Transform, optionally including all child Transforms.
        /// </summary>
        /// <param name="objectTransform">The Transform to calculate bounds for.</param>
        /// <param name="includeChildren">Whether to include bounds of child Transforms.</param>
        /// <returns>The calculated bounds.</returns>
        public static Bounds GetMeshFilterBounds(this Transform objectTransform, bool includeChildren = true)
        {
            MeshFilter meshFilter = objectTransform.GetComponent<MeshFilter>();
            Bounds result = meshFilter != null ? meshFilter.mesh.bounds : new Bounds();
            if (includeChildren)
            {
                foreach (Transform transform in objectTransform)
                {
                    Bounds bounds = GetMeshFilterBounds(transform);
                    result.Encapsulate(bounds.min);
                    result.Encapsulate(bounds.max);
                }
            }

            Vector3 scaledMin = result.min;
            scaledMin.Scale(objectTransform.localScale);
            result.min = scaledMin;
            Vector3 scaledMax = result.max;
            scaledMax.Scale(objectTransform.localScale);
            result.max = scaledMax;
            return result;
        }


        /// <summary>
        /// Finds the nearest GameObject with a component of type T.
        /// </summary>
        /// <param name="self">The reference GameObject to measure distance from.</param>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <returns>The nearest GameObject with the specified component, or null if none found.</returns>
        public static GameObject FindNearestObject<T>(this GameObject self) where T : Component
        {
            var allComs = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
            if (allComs == null || allComs?.Length == 0) return null;
            float closestDistance = Mathf.Infinity;
            GameObject closestObject = null;
            foreach (var comp in allComs)
            {
                float distanceToCollider = Vector3.Distance(self.transform.position, comp.transform.position);

                if (distanceToCollider < closestDistance)
                {
                    closestDistance = distanceToCollider;
                    closestObject = comp.gameObject;
                }
            }
            return closestObject;
        }

        /// <summary>
        /// Finds the parent GameObject by name, traversing upwards.
        /// </summary>
        /// <param name="self">The starting GameObject.</param>
        /// <param name="name">The name of the parent to find.</param>
        /// <returns>The parent GameObject if found; otherwise, null.</returns>
        public static GameObject FindParent(this GameObject self, string name)
        {
            Transform current = self.transform.parent;
            while (current != null)
            {
                if (current.name == name)
                {
                    return current.gameObject;
                }
                current = current.parent;
            }
            return null;
        }

        /// <summary>
        /// Finds the first component of type T in the parent hierarchy.
        /// If a name is provided, it finds the parent GameObject with the specified name and returns the component of type T.
        /// </summary>
        /// <typeparam name="T">The type of component to find.</typeparam>
        /// <param name="self">The starting GameObject.</param>
        /// <param name="name">The name of the parent GameObject to find (optional).</param>
        /// <returns>The component of type T if found; otherwise, null.</returns>
        public static T FindComponentInParent<T>(this GameObject self, string name = null) where T : Component
        {
            Transform current = self.transform.parent;
            while (current != null)
            {
                if (name == null || current.name == name)
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

        private static void SetInteractiveNow(this RectTransform self, bool active, bool hide)
        {
            CanvasGroup cg = self.gameObject.EnsureComponent<CanvasGroup>();
            cg.interactable = active;
            if (hide)
            {
                cg.alpha = active ? 1 : 0;
            }
        }
        /// <summary>
        /// Checks if a GameObject is active and its MeshRenderer is enabled.
        /// </summary>
        /// <param name="self">The GameObject.</param>
        /// <returns>True if the GameObject is active and the MeshRenderer is enabled; otherwise, false.</returns>
        public static bool IsActiveAndMeshEnabled(this GameObject self)
        {
            bool success = false;
            if (self.activeInHierarchy)
            {
                var meshRenderer = self.GetComponent<MeshRenderer>();
                if (meshRenderer != null && meshRenderer.enabled)
                {
                    success = true;
                }
            }
            return success;
        }

        /// <summary>
        /// Checks if a GameObject has a specific component.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="self">The GameObject.</param>
        /// <returns>True if the component is found; otherwise, false.</returns>
        public static bool HasComponent<T>(this GameObject self) where T : Component
        {
            T comp = self.GetComponent<T>();
            return comp != null;
        }

        /// <summary>
        /// Ensures that a GameObject has a specific component, if not, add component to the GameObject
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="self">The GameObject.</param>
        /// <returns>The component instance, either existing or newly added.</returns>
        public static T EnsureComponent<T>(this GameObject self) where T : Component
        {
            T comp = self.GetComponent<T>();
            if (comp == null)
            {
                comp = self.AddComponent<T>();
            }

            return comp;
        }

        /// <summary>
        /// Gets a component from the GameObject or its parent.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <param name="self">The GameObject.</param>
        /// <returns>The component instance, or null if not found.</returns>
        public static T GetComponentInSelfThenParent<T>(this GameObject self) where T : Component
        {
            T comp = self.GetComponent<T>();
            if (comp == null)
            {
                comp = self.GetComponentInParent<T>();
            }

            return comp;
        }

        /// <summary>
        /// Finds a child GameObject by name.
        /// </summary>
        /// <param name="self">The parent Transform.</param>
        /// <param name="searchText">The name of the child.</param>
        /// <returns>The child Transform, or null if not found.</returns>
        public static Transform FindChildByName(this Transform self, string searchText)
        {
            Transform[] transforms = self.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in transforms)
            {
                if (searchText == item.name)
                {
                    return item;
                }
            }

            return null;
        }


        /// <summary>
        /// Converts a Color to a Color32.
        /// </summary>
        /// <param name="color">The Color to convert.</param>
        /// <returns>The converted Color32.</returns>
        public static Color32 ConvertColorToColor32(this Color color)
        {
            byte r = (byte)(color.r * 255f);
            byte g = (byte)(color.g * 255f);
            byte b = (byte)(color.b * 255f);
            byte a = (byte)(color.a * 255f);

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Converts a Color32 to a Color.
        /// </summary>
        /// <param name="color32">The Color32 to convert.</param>
        /// <returns>The converted Color.</returns>
        public static Color ConvertColor32ToColor(this Color32 color32)
        {
            float r = color32.r / 255f;
            float g = color32.g / 255f;
            float b = color32.b / 255f;
            float a = color32.a / 255f;

            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Toggles the blinking effect on a 3D object.
        /// </summary>
        /// <param name="self">The GameObject to toggle blinking on.</param>
        /// <param name="blink">Whether to enable or disable blinking.</param>
        public static void ToggleBlink(this GameObject self, bool blink)
        {
            Highlighter blink3D = self.EnsureComponent<Highlighter>();
            if (blink)
            {
                blink3D.StartHighlight();
            }
            else
            {
                blink3D.StopHighlight();
            }
        }
        /// <summary>
        /// set layer
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layerName"></param>
        public static void SetLayer(this GameObject self, string layerName)
        {
            int roomSurfaceLayer = LayerMask.NameToLayer("RoomSurface");
            if (roomSurfaceLayer == -1)
            {
                Debug.LogError($"Layer '{layerName}' not found.");
                return;
            }
            self.layer = roomSurfaceLayer;
        }

        #endregion
        #region Mesh

        public static GameObject GenerateSkirtingLine(Vector3 center, Vector2 extent, Material material,
           float standardWidth = 10f, float standardHeight = 10f, float fixedWidth = 0.01f, float fixedHeight = 0.08f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentY = extent.y * 0.5f;
            var realCenter = center - new Vector3(0, halfExtentY, 0) + new Vector3(0, fixedHeight * 0.5f, fixedWidth * 0.5f);
            var realHalfExtentX = extent.x * 0.5f;
            var realHalfExtentY = fixedHeight * 0.5f;
            var realHalfExtentZ = fixedWidth * 0.5f;

            var bottomY = realCenter.y - realHalfExtentY;
            var topY = realCenter.y + realHalfExtentY;
            var leftX = realCenter.x - realHalfExtentX;
            var rightX = realCenter.x + realHalfExtentX;
            var farZ = realCenter.z - realHalfExtentZ;
            var nearZ = realCenter.z + realHalfExtentZ;

            var node1 = new Vector3(rightX, topY, nearZ);
            var node2 = new Vector3(rightX, topY, farZ);
            var node3 = new Vector3(leftX, topY, farZ);
            var node4 = new Vector3(leftX, topY, nearZ);
            var node5 = new Vector3(leftX, topY, nearZ);
            var node6 = new Vector3(leftX, bottomY, nearZ);
            var node7 = new Vector3(rightX, bottomY, nearZ);
            var node8 = new Vector3(rightX, topY, nearZ);

            points.Add(node1);
            points.Add(node2);
            points.Add(node3);
            points.Add(node4);
            points.Add(node5);
            points.Add(node6);
            points.Add(node7);
            points.Add(node8);

            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = fixedHeight / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(w, 0);
            uvs[1] = new Vector2(w, h);
            uvs[2] = new Vector2(0, h);
            uvs[3] = new Vector2(0, 0);
            uvs[4] = new Vector2(0, h);
            uvs[5] = new Vector2(0, 0);
            uvs[6] = new Vector2(w, 0);
            uvs[7] = new Vector2(w, h);

            var meshObject = new GameObject("SkirtingLine");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = new[] { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4 };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;

        }
        public static GameObject GenerateQuadMesh(Vector3 center, Vector2 extent, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentX = extent.x * 0.5f;
            var halfExtentY = extent.y * 0.5f;
            var expand = 0.01f;
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y - halfExtentY - expand, center.z));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y - halfExtentY - expand, center.z));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y + halfExtentY + expand, center.z));
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y + halfExtentY + expand, center.z));
            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = extent.y / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(w, 0);
            uvs[2] = new Vector2(w, h);
            uvs[3] = new Vector2(0, h);

            var meshObject = new GameObject("QuadMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = tr.Triangulate();

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
        public static GameObject GeneratePolygonMesh(IList<Vector3> points, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            var meshObject = new GameObject("GenerateMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            var pArray = points.ToArray();
            var minX = points[0].x;
            var maxX = points[0].x;
            var minY = points[0].y;
            var maxY = points[0].y;
            foreach (var point in points)
            {
                if (point.x < minX)
                    minX = point.x;
                if (maxX < point.x)
                    maxX = point.x;
                if (point.y < minY)
                    minY = point.y;
                if (maxY < point.y)
                    maxY = point.y;
            }
            var u = (maxX - minX) / standardWidth;
            var v = (maxY - minY) / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2((pArray[i].x - minX) / (maxX - minX) * u, (pArray[i].y - minY) / (maxY - minY) * v);
            }
            mesh.vertices = pArray;
            mesh.uv = uvs;

            var tr = new Triangulator(pArray);
            int[] triangles = tr.Triangulate();

            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
        public static GameObject GeneratePlane(Vector3 center, Vector2 extent, Material material, float standardWidth = 10f, float standardHeight = 10f)
        {
            IList<Vector3> points = new List<Vector3>();
            var halfExtentX = extent.x * 0.5f;
            var halfExtentY = extent.y * 0.5f;
            var expand = 0.00f;
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y, center.z - halfExtentY - expand));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y, center.z - halfExtentY - expand));
            points.Add(new Vector3(center.x + halfExtentX + expand, center.y, center.z + halfExtentY + expand));
            points.Add(new Vector3(center.x - halfExtentX - expand, center.y, center.z + halfExtentY + expand));
            var pArray = points.ToArray();

            var w = extent.x / standardWidth;
            var h = extent.y / standardHeight;
            Vector2[] uvs = new Vector2[pArray.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(w, 0);
            uvs[2] = new Vector2(w, h);
            uvs[3] = new Vector2(0, h);

            var meshObject = new GameObject("PlaneMesh");
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = pArray;
            mesh.uv = uvs;

            // var tr = new Triangulator(pArray);
            int[] triangles = new int[] { 0, 3, 2, 2, 1, 0 };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            return meshObject;
        }
        #endregion
        #region UI
        /// <summary>
        /// Toggles the visibility of a RectTransform as a CanvasGroup.
        /// </summary>
        /// <param name="rect">The RectTransform.</param>
        /// <param name="isActive">Whether the element should be active.</param>
        /// <param name="useTween">Whether to use tweening for the transition.</param>
        /// <param name="tweenDuration">The duration of the tween.</param>
        /// <param name="cb">Callback function after the transition.</param>
        /// <param name="changeInteractive">Whether to change interactivity.</param>
        /// <param name="ignoreParentGroups">Whether to ignore parent CanvasGroups.</param>
        public static void ToggleAsCanvasGroup(this RectTransform rect, bool isActive, bool useTween = true,
     float tweenDuration = 0.5f, Action cb = null, bool changeInteractive = true, bool ignoreParentGroups = false)
        {
            CanvasGroup cg = rect.gameObject.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();

            if (changeInteractive)
            {
                cg.interactable = isActive;
                cg.blocksRaycasts = isActive;
            }

            cg.ignoreParentGroups = ignoreParentGroups;

            if (useTween)
            {
                MonoBehaviourHelper.Instance.StartCoroutine(FadeCanvasGroup(cg, isActive ? 1 : 0, tweenDuration, cb));
            }
            else
            {
                cg.alpha = isActive ? 1 : 0;
                cb?.Invoke();
            }
        }

        private static IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, Action cb)
        {
            float startAlpha = cg.alpha;
            float time = 0f;

            while (time < duration)
            {
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            cg.alpha = targetAlpha;
            cb?.Invoke();
        }


        /// <summary>
        /// Toggles the visibility of a RectTransform as a CanvasGroup automatically.
        /// </summary>
        /// <param name="rect">The RectTransform.</param>
        /// <param name="useTween">Whether to use tweening for the transition.</param>
        /// <param name="tweenDuration">The duration of the tween.</param>
        /// <param name="cb">Callback function after the transition.</param>
        public static void ToggleAsCanvasGroupAuto(this RectTransform rect, bool useTween = true,
            float tweenDuration = 0.5f, Action cb = null)
        {
            CanvasGroup cg = rect.gameObject.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
            bool isActive = Mathf.Approximately(1, cg.alpha);

            if (useTween)
            {
                MonoBehaviourHelper.Instance.StartCoroutine(FadeCanvasGroup(cg, isActive ? 0 : 1, tweenDuration, cb));
            }
            else
            {
                cg.alpha = isActive ? 0 : 1;
                cb?.Invoke();
            }
        }

        /// <summary>
        /// Shows or hides a Transform by setting its local scale.
        /// </summary>
        /// <param name="rect">The Transform.</param>
        /// <param name="isActive">Whether the element should be visible.</param>
        public static void Show(this Transform rect, bool isActive)
        {
            if (isActive)
            {
                rect.localScale = Vector3.one;
            }
            else
            {
                rect.localScale = Vector3.zero;
            }
        }

        /// <summary>
        /// Shows or hides a RectTransform by setting its local scale.
        /// </summary>
        /// <param name="rect">The RectTransform.</param>
        /// <param name="isActive">Whether the element should be visible.</param>
        public static void Show(this RectTransform rect, bool isActive)
        {
            if (isActive)
            {
                rect.localScale = Vector3.one;
            }
            else
            {
                rect.localScale = Vector3.zero;
            }
        }

        /// <summary>
        /// Converts a Texture2D to a Sprite.
        /// </summary>
        /// <param name="tex">The Texture2D.</param>
        /// <returns>The resulting Sprite.</returns>
        public static Sprite ToSprite(this Texture2D tex)
        {
            Vector2 pivot = new(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), pivot, 100.0f);
            return sprite;
        }

        /// <summary>
        /// Converts a Sprite to a Texture2D.
        /// </summary>
        /// <param name="sprite">The Sprite.</param>
        /// <returns>The resulting Texture2D.</returns>
        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            Texture2D targetTex = new((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            targetTex.SetPixels(pixels);
            targetTex.Apply();
            return targetTex;
        }

        /// <summary>
        /// Converts a Base64 string to a Texture2D.
        /// </summary>
        /// <param name="Base64STR">The Base64 string.</param>
        /// <returns>The resulting Texture2D.</returns>
        public static Texture2D ToTexture2D(this string Base64STR)
        {
            Texture2D pic = new(190, 190, TextureFormat.RGBA32, false);
            byte[] data = Convert.FromBase64String(Base64STR);
            pic.LoadImage(data);
            return pic;
        }

        /// <summary>
        /// Converts a Texture2D to a Base64 string.
        /// </summary>
        /// <param name="texture">The Texture2D.</param>
        /// <returns>The Base64-encoded string.</returns>
        public static string ToBase64(this Texture2D texture)
        {
            byte[] bytesArr = texture.EncodeToPNG();
            string strbaser64 = Convert.ToBase64String(bytesArr);
            return strbaser64;
        }

        /// <summary>
        /// Converts a Texture to a Texture2D.
        /// </summary>
        /// <param name="texture">The Texture.</param>
        /// <returns>The resulting Texture2D.</returns>
        public static Texture2D ToTexture2D(this Texture texture)
        {
            Texture2D texture2D = new(texture.width, texture.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture2D;
        }

        /// <summary>
        /// Decodes a Base64 string to a Texture2D.
        /// </summary>
        /// <param name="base64String">The Base64 string.</param>
        /// <returns>The resulting Texture2D.</returns>
        public static Texture2D DecodeBase64Image(string base64String)
        {
            byte[] imageData = System.Convert.FromBase64String(base64String);

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageData);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Tweens the color of a UI Image.
        /// </summary>
        /// <param name="self">The Image component.</param>
        /// <param name="targetColor">The target color.</param>
        /// <param name="loopTime">The number of loops.</param>
        /// <param name="cb">Callback function after the tween completes.</param>
        public static void TweenColor(this Image self, Color targetColor, int loopTime = 3, Action cb = null)
        {
            self.StartCoroutine(TweenColorCoroutine(self, targetColor, loopTime, cb));
        }
        private static IEnumerator TweenColorCoroutine(Image self, Color targetColor, int loopTime, Action cb)
        {
            Color initColor = self.color;
            float duration = 0.5f;

            for (int i = 0; i < loopTime; i++)
            {
                yield return LerpColor(self, initColor, targetColor, duration);
                yield return LerpColor(self, targetColor, initColor, duration);
            }

            self.color = initColor;
            cb?.Invoke();
        }

        private static IEnumerator LerpColor(Image self, Color from, Color to, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                self.color = Color.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            self.color = to;
        }
        /// <summary>
        /// Moves a UI element out of the screen.
        /// </summary>
        /// <param name="self">The RectTransform.</param>
        /// <param name="hideTo">The direction to hide the element (e.g., Top, Left).</param>
        /// <param name="extraOffset">Additional offset for the position.</param>
        /// <param name="useTween">Whether to use tweening for the transition.</param>
        /// <param name="tweenDuration">The duration of the tween.</param>
        public static void MoveOutOfScreen(this RectTransform self, Placement hideTo = Placement.Top,
            float extraOffset = 0, bool useTween = true, float tweenDuration = 0.5f)
        {
            float targetPosX = self.anchoredPosition.x;
            float targetPosY = self.anchoredPosition.y;

            switch (hideTo)
            {
                case Placement.Top:
                    targetPosY = Screen.height / 2f + self.rect.height / 2f + extraOffset;
                    break;
                case Placement.Left:
                    targetPosX = -Screen.width / 2f - self.rect.width / 2f - extraOffset;
                    break;
                case Placement.Right:
                    targetPosX = Screen.width / 2f + self.rect.width / 2f + extraOffset;
                    break;
                case Placement.Bottom:
                    targetPosY = -Screen.height / 2f - self.rect.height / 2f - extraOffset;
                    break;
            }

            Vector2 targetAnchorPos = new(targetPosX, targetPosY);

            if (useTween)
            {
                MonoBehaviourHelper.Instance.StartCoroutine(MoveCoroutine(self, targetAnchorPos, tweenDuration));
            }
            else
            {
                self.anchoredPosition = targetAnchorPos;
            }
        }
        private static IEnumerator MoveCoroutine(RectTransform self, Vector2 targetPos, float duration)
        {
            Vector2 startPos = self.anchoredPosition;
            float time = 0f;

            while (time < duration)
            {
                self.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            self.anchoredPosition = targetPos;
        }
        #endregion


    }

    public enum Placement
    {
        Top,
        Left,
        Bottom,
        Right
    }

    internal class Triangulator
    {
        private List<Vector3> m_points = new List<Vector3>();

        public Triangulator(Vector3[] points)
        {
            m_points = new List<Vector3>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            // indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }

}
