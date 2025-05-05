//-----------------------------------------------------------------------
// <copyright file="ChildAttribute.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd.. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
/// <summary>
/// The Highlighter class provides a dynamic blinking highlight effect for game objects.
/// It adjusts the material's color and emission properties to create a visual effect
/// that draws attention to the object. The class supports transparent materials and
/// includes performance optimizations for real-time updates.
/// 
/// Key Features:
/// - Dynamic blinking highlight effect.
/// - Automatic material initialization and management.
/// - Support for transparent materials.
/// - Adjustable emission intensity and blinking speed.
/// - Performance optimization through update interval control.
/// 
/// Usage:
/// - Call StartHighlight() to begin the highlight effect.
/// - Call StopHighlight() to end the highlight effect and reset materials.
/// </summary>
//-----------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UTools
{
    public class Highlighter : MonoBehaviour
    {
        [Header("Blink Settings")]
        public Color highlightColor = Color.yellow;
        public float blinkSpeed = 2f;
        public float minIntensity = 0.2f;
        public float maxIntensity = 1.0f;
        [Tooltip("Automatically enable material emission")]
        public bool autoEnableEmission = true;
        [Tooltip("Update frequency (frames), higher values improve performance")]
        public int updateInterval = 2;
        [Tooltip("Emission intensity")]
        public float emissiveIntensity = 200f;
        [Tooltip("Maintain material transparency")]
        public bool maintainTransparency = true;
        [Tooltip("toggle highlight")]
        public bool isHighlighting = false;

        private List<Renderer> renderers = new List<Renderer>();
        private List<Material> materials = new List<Material>();
        private List<Color> initialColors = new List<Color>();
        private List<float> initialAlphaValues = new List<float>();
        private List<bool> isTransparentMaterial = new List<bool>();
        private float currentIntensity;


        private int frameCounter = 0;
        private Color blinkColor;
        private bool materialsInitialized = false;

        void Awake()
        {
            InitializeMaterials();
            ResetMaterials();
        }

        void Start()
        {
            // Ensure the initial state is correct
            ResetMaterials();
        }

        // Check if the material is transparent
        private bool IsMaterialTransparent(Material material)
        {
            if (material == null) return false;

            // Check rendering mode
            if (material.HasProperty("_Mode"))
            {
                int mode = (int)material.GetFloat("_Mode");
                // Mode 2=Fade, 3=Transparent
                return mode == 2 || mode == 3;
            }

            // Check rendering queue
            if (material.renderQueue >= 3000)
                return true;

            // Check keywords
            if (material.IsKeywordEnabled("_ALPHABLEND_ON") ||
                material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON") ||
                material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))
                return true;

            // Check alpha value of the color
            if (material.HasProperty("_Color") && material.color.a < 0.99f)
                return true;

            return false;
        }

        // Initialize materials, only executed once
        private void InitializeMaterials()
        {
            if (materialsInitialized) return;

            renderers = GetComponentsInChildren<Renderer>().ToList();
            initialAlphaValues.Clear();
            isTransparentMaterial.Clear();

            if (renderers.Count == 0)
            {
                Debug.LogWarning("No valid renderer components found, unable to apply highlight effect");
                return;
            }

            // Collect all materials
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                Material[] rendererMaterials = renderer.materials;
                foreach (Material mat in rendererMaterials)
                {
                    materials.Add(mat);
                    bool isTransparent = IsMaterialTransparent(mat);
                    isTransparentMaterial.Add(isTransparent);

                    // Save color and transparency information
                    initialColors.Add(mat.color);
                    initialAlphaValues.Add(mat.color.a);

                    Debug.Log($"Material: {mat.name}, Is Transparent: {isTransparent}, Initial Alpha Value: {mat.color.a}");
                }
            }

            // Enable emission
            if (autoEnableEmission)
                EnableEmission();

            materialsInitialized = true;
        }

        void Update()
        {
            if (!isHighlighting || !materialsInitialized || materials.Count == 0)
                return;

            // Use counter to reduce update frequency
            frameCounter++;
            if (frameCounter < updateInterval)
                return;

            frameCounter = 0;

            // Use PingPong function to create smooth blinking effect
            float v = Mathf.PingPong(Time.time * blinkSpeed, 1);
            currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, v);

            // Precompute color value
            blinkColor = Color.Lerp(Color.white, highlightColor, v);

            ApplyHighlight(v);
        }

        // Start blinking highlight
        public void StartHighlight()
        {
            Debug.Log("StartHighlight");

            // Ensure materials are initialized
            if (!materialsInitialized)
                InitializeMaterials();

            isHighlighting = true;
            frameCounter = 0;
        }

        // Stop blinking highlight
        public void StopHighlight()
        {
            Debug.Log("StopHighlight");
            isHighlighting = false;
            ResetMaterials();
        }

        // Apply highlight effect
        private void ApplyHighlight(float intensity)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                Material material = materials[i];
                if (material == null) continue;

                // Set emission properties
                material.SetColor("_EmissiveColor", blinkColor);
                material.SetFloat("_EmissiveIntensity", emissiveIntensity);
                material.SetFloat("_EmissiveExposureWeight", intensity);

                // Preserve transparency when setting color
                if (maintainTransparency && isTransparentMaterial[i] && i < initialAlphaValues.Count)
                {
                    // Create a new color, preserving the original alpha value
                    Color newColor = blinkColor;
                    newColor.a = initialAlphaValues[i];
                    material.color = newColor;

                    // Ensure transparent rendering mode remains unchanged
                    if (material.HasProperty("_SrcBlend"))
                        material.SetFloat("_SrcBlend", 1); // SrcAlpha

                    if (material.HasProperty("_DstBlend"))
                        material.SetFloat("_DstBlend", 10); // OneMinusSrcAlpha

                    if (material.HasProperty("_ZWrite"))
                        material.SetFloat("_ZWrite", 0); // Disable depth writing

                    if (material.renderQueue < 3000)
                        material.renderQueue = 3000; // Transparent rendering queue
                }
                else
                {
                    material.color = blinkColor;
                }
            }
        }

        // Reset materials
        private void ResetMaterials()
        {
            if (!materialsInitialized || materials.Count == 0) return;

            // Restore original state of materials
            for (int i = 0; i < materials.Count; i++)
            {
                if (i < initialColors.Count)
                {
                    materials[i].color = initialColors[i];
                }
                materials[i].SetColor("_EmissiveColor", Color.black);
                materials[i].SetFloat("_EmissiveIntensity", 0);
                materials[i].SetFloat("_EmissiveExposureWeight", 1);
            }
        }

        // Enable emission for all materials
        private void EnableEmission()
        {
            if (materials.Count == 0) return;

            foreach (Material material in materials)
            {
                if (material == null) continue;

                // Set emission properties to initial state
                material.SetColor("_EmissiveColor", Color.black);
                material.SetFloat("_EmissiveIntensity", 0);
                material.SetFloat("_EmissiveExposureWeight", 1);
            }

            Debug.Log("Emission enabled for all materials and reset emission color");
        }

        // Reset state when the object is disabled or destroyed
        private void OnDisable()
        {
            if (isHighlighting)
            {
                isHighlighting = false;
                ResetMaterials();
            }
        }
    }
}