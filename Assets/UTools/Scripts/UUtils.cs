//-----------------------------------------------------------------------
// <copyright file="UUtils.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     Static helper method class.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

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
        public static GameObject[] GetAllChildren(this GameObject self)
        {
            Transform[] childTransforms = self.GetComponentsInChildren<Transform>(true);
            return Array.FindAll(childTransforms, transform => transform.gameObject != self)
                        .Select(transform => transform.gameObject)
                        .ToArray();
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
        ///     只显示一个child
        /// </summary>
        /// <param name="self"></param>
        /// <param name="childName"></param>
        public static GameObject ShowOneChild(this GameObject self, string childName)
        {
            GameObject[] children = self.Children().ToArray();
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
            GameObject[] children = self.Children().ToArray();
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
            GameObject[] children = self.Children().ToArray();
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
            GameObject[] children = self.Children().ToArray();
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
            GameObject[] children = self.Children().ToArray();
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
            GameObject[] children = self.Children().ToArray();
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

        private static void SetInteractiveNow(this GameObject self, bool active, bool hide)
        {
            Collider collider = self.GetComponent<Collider>();
            if (!collider)
            {
                collider = self.GetComponent<BoxCollider>();
            }

            if (collider)
            {
                collider.enabled = active;
            }

            PointerEventListener el = self.GetComponent<PointerEventListener>();
            if (el != null)
            {
                el.enabled = active;
                if (hide)
                {
                    el.gameObject.ToggleMesh(active);
                }
            }
        }
        /// <summary>
        /// 显示隐藏mesh
        /// </summary>
        /// <param name="self"></param>
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
        public static GameObject CombineMesh(this GameObject self, string name = null, Transform parent = null)
        {
            // 获取当前GameObject的所有子物体
            MeshFilter[] meshFilters = self.GetComponentsInChildren<MeshFilter>();
            if (meshFilters?.Length == 0)
            {
                Debug.LogError("未找到任何MeshFilter");
                return self;
            }
            // 创建一个新的CombineInstance列表，用于存储子物体的Mesh
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            // 遍历子物体的MeshFilter并添加到CombineInstance列表中
            foreach (MeshFilter meshFilter in meshFilters)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilter.sharedMesh;
                combineInstance.transform = meshFilter.transform.localToWorldMatrix;
                combineInstances.Add(combineInstance);
            }

            // 创建一个新的Mesh用于存储合并后的Mesh
            Mesh combinedMesh = new Mesh();

            // 使用CombineMeshes函数将所有子物体的Mesh合并到一个Mesh中
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            // 创建一个新的GameObject来显示合并后的Mesh
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
        /// 闪烁3d物体（物体必须具有材质）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="blink"></param>
        public static void ToggleBlink(this GameObject self, bool blink)
        {
            Debug.Log("是否高亮：" + IsNeedHit);
            if (!IsNeedHit) return;
            Blink3D blink3D = self.EnsureComponent<Blink3D>();
            blink3D.IsBlink = blink;
        }
        /// <summary>
        /// 闪烁3d物体(不包括子物体)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="blink"></param>
        public static void ToggleBlinkSelf(this GameObject self, bool blink)
        {
            Debug.Log("是否高亮：" + IsNeedHit);
            if (!IsNeedHit) return;
            Blink3DSelf blink3D = self.EnsureComponent<Blink3DSelf>();
            blink3D.IsBlink = blink;
        }

        public static void ToggleBlinkChild(this GameObject self, string childName, bool blink)
        {
            Debug.Log("是否高亮：" + IsNeedHit);
            if (!IsNeedHit) return;
            if (string.IsNullOrEmpty(childName))
            {
                return;
            }

            GameObject child = self.FindChild(childName);
            if (!child)
            {
                return;
            }

            child.EnsureComponent<Blink3D>().IsBlink = blink;
        }

        public static void ToggleBlinkUpdateMat(this GameObject self, bool blink, Material mat, int matIndex)
        {
            Debug.Log("是否高亮：" + IsNeedHit);
            if (!IsNeedHit) return;
            Debug.Log(mat.name);
            self.EnsureComponent<Blink3D>().UpdateMatAndBlink(blink, mat, matIndex);
        }
        /// <summary>
        /// 闪烁decal
        /// </summary>
        /// <param name="self"></param>
        /// <param name="blink"></param>
        public static void ToggleBlink(this DecalProjector self, bool blink, float initFadeFactor = 1)
        {
            Debug.Log("是否高亮：" + IsNeedHit);
            if (!IsNeedHit) return;
            PingPongFloat compPingPong = self.gameObject.EnsureComponent<PingPongFloat>();
            if (blink)
            {
                compPingPong.IsRun = true;
                compPingPong.OnValueChanged += (v) =>
                {
                    self.fadeFactor = v;
                };
            }
            else
            {
                compPingPong.IsRun = false;
                self.fadeFactor = initFadeFactor;
            }


        }

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
        ///     震动
        /// </summary>
        /// <param name="strong"></param>
        public static void DoShake(this GameObject self, UnityAction cb = null)
        {
            self.transform.DOShakeScale(1, 0.2f).OnComplete(() => { cb?.Invoke(); });
        }

        public static PointerEventListener GetEventListener(this GameObject self)
        {
            return self.EnsureComponent<PointerEventListener>();
        }

        public static PointerEventListener GetEventListener(this Component self)
        {
            return self.gameObject.EnsureComponent<PointerEventListener>();
        }
        /// <summary>
        /// 查找离得最近的、具有T类型组件的物体
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        public static void ActiveEmission(this Material self, float intensity, float activeWeight = -1)
        {
            self.EnableKeyword("_EMISSION");
            // self.SetFloat("_EmissiveIntensity", intensity);
            // self.SetFloat("_EmissiveExposureWeight", activeWeight == -1 ? 0 : activeWeight);
            Color emissionColor = self.GetColor("_EmissiveColor");
            emissionColor.a = activeWeight == -1 ? 1 : activeWeight;
            self.SetColor("_EmissiveColor", emissionColor);
        }

        public static void DeactiveEmission(this Material self)
        {
            self.SetFloat("_EmissiveIntensity", 0);
            self.SetFloat("_EmissiveExposureWeight", 1);
        }
        // 将Color转换为Color32
        public static Color32 ConvertColorToColor32(this Color color)
        {
            byte r = (byte)(color.r * 255f);
            byte g = (byte)(color.g * 255f);
            byte b = (byte)(color.b * 255f);
            byte a = (byte)(color.a * 255f);

            return new Color32(r, g, b, a);
        }

        // 将Color32转换为Color
        public static Color ConvertColor32ToColor(this Color32 color32)
        {
            float r = color32.r / 255f;
            float g = color32.g / 255f;
            float b = color32.b / 255f;
            float a = color32.a / 255f;

            return new Color(r, g, b, a);
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

}
