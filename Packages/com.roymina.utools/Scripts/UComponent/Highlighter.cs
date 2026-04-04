using System.Collections.Generic;
using UnityEngine;

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

        private readonly List<RendererBinding> _rendererBindings = new();
        private int _frameCounter;
        private bool _materialsInitialized;

        private void Awake()
        {
            InitializeMaterials();
            ResetMaterials();
        }

        private void Update()
        {
            if (!isHighlighting || !_materialsInitialized || _rendererBindings.Count == 0)
            {
                return;
            }

            _frameCounter++;
            if (_frameCounter < Mathf.Max(1, updateInterval))
            {
                return;
            }

            _frameCounter = 0;
            float normalized = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, normalized);
            Color pulseColor = Color.Lerp(Color.white, highlightColor, normalized);
            ApplyHighlight(pulseColor, intensity);
        }

        public void StartHighlight()
        {
            if (!_materialsInitialized)
            {
                InitializeMaterials();
            }

            isHighlighting = true;
            _frameCounter = 0;
        }

        public void StopHighlight()
        {
            isHighlighting = false;
            ResetMaterials();
        }

        private void InitializeMaterials()
        {
            if (_materialsInitialized)
            {
                return;
            }

            _rendererBindings.Clear();
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Material[] sharedMaterials = renderer.sharedMaterials;
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    Material material = sharedMaterials[i];
                    if (material == null)
                    {
                        continue;
                    }

                    _rendererBindings.Add(new RendererBinding(renderer, i, material));
                }
            }

            _materialsInitialized = _rendererBindings.Count > 0;
            if (!_materialsInitialized)
            {
                Debug.LogWarning("No valid renderer components found, unable to apply highlight effect");
            }
        }

        private void ApplyHighlight(Color pulseColor, float intensity)
        {
            foreach (RendererBinding binding in _rendererBindings)
            {
                binding.Renderer.GetPropertyBlock(binding.PropertyBlock, binding.MaterialIndex);

                if (!string.IsNullOrEmpty(binding.ColorPropertyName))
                {
                    Color color = pulseColor;
                    if (maintainTransparency && binding.IsTransparent)
                    {
                        color.a = binding.InitialColor.a;
                    }

                    binding.PropertyBlock.SetColor(binding.ColorPropertyName, color);
                }

                if (autoEnableEmission && !string.IsNullOrEmpty(binding.EmissionColorPropertyName))
                {
                    binding.PropertyBlock.SetColor(binding.EmissionColorPropertyName, pulseColor * emissiveIntensity * intensity);
                }

                if (!string.IsNullOrEmpty(binding.EmissionIntensityPropertyName))
                {
                    binding.PropertyBlock.SetFloat(binding.EmissionIntensityPropertyName, autoEnableEmission ? emissiveIntensity : 0f);
                }

                if (!string.IsNullOrEmpty(binding.EmissionExposurePropertyName))
                {
                    binding.PropertyBlock.SetFloat(binding.EmissionExposurePropertyName, intensity);
                }

                binding.Renderer.SetPropertyBlock(binding.PropertyBlock, binding.MaterialIndex);
            }
        }

        private void ResetMaterials()
        {
            if (!_materialsInitialized)
            {
                return;
            }

            foreach (RendererBinding binding in _rendererBindings)
            {
                binding.Renderer.GetPropertyBlock(binding.PropertyBlock, binding.MaterialIndex);

                if (!string.IsNullOrEmpty(binding.ColorPropertyName))
                {
                    binding.PropertyBlock.SetColor(binding.ColorPropertyName, binding.InitialColor);
                }

                if (!string.IsNullOrEmpty(binding.EmissionColorPropertyName))
                {
                    binding.PropertyBlock.SetColor(binding.EmissionColorPropertyName, Color.black);
                }

                if (!string.IsNullOrEmpty(binding.EmissionIntensityPropertyName))
                {
                    binding.PropertyBlock.SetFloat(binding.EmissionIntensityPropertyName, 0f);
                }

                if (!string.IsNullOrEmpty(binding.EmissionExposurePropertyName))
                {
                    binding.PropertyBlock.SetFloat(binding.EmissionExposurePropertyName, 1f);
                }

                binding.Renderer.SetPropertyBlock(binding.PropertyBlock, binding.MaterialIndex);
            }
        }

        private void OnDisable()
        {
            if (isHighlighting)
            {
                isHighlighting = false;
                ResetMaterials();
            }
        }

        private sealed class RendererBinding
        {
            public RendererBinding(Renderer renderer, int materialIndex, Material material)
            {
                Renderer = renderer;
                MaterialIndex = materialIndex;
                PropertyBlock = new MaterialPropertyBlock();
                ColorPropertyName = ResolveColorProperty(material);
                EmissionColorPropertyName = ResolveFirstProperty(material, "_EmissionColor", "_EmissiveColor");
                EmissionIntensityPropertyName = ResolveFirstProperty(material, "_EmissionIntensity", "_EmissiveIntensity");
                EmissionExposurePropertyName = ResolveFirstProperty(material, "_EmissionExposureWeight", "_EmissiveExposureWeight");
                IsTransparent = IsTransparentMaterial(material);
                InitialColor = !string.IsNullOrEmpty(ColorPropertyName) ? material.GetColor(ColorPropertyName) : Color.white;
            }

            public Renderer Renderer { get; }
            public int MaterialIndex { get; }
            public MaterialPropertyBlock PropertyBlock { get; }
            public string ColorPropertyName { get; }
            public string EmissionColorPropertyName { get; }
            public string EmissionIntensityPropertyName { get; }
            public string EmissionExposurePropertyName { get; }
            public bool IsTransparent { get; }
            public Color InitialColor { get; }

            private static string ResolveColorProperty(Material material)
            {
                return ResolveFirstProperty(material, "_BaseColor", "_Color");
            }

            private static string ResolveFirstProperty(Material material, params string[] propertyNames)
            {
                foreach (string propertyName in propertyNames)
                {
                    if (material.HasProperty(propertyName))
                    {
                        return propertyName;
                    }
                }

                return null;
            }

            private static bool IsTransparentMaterial(Material material)
            {
                if (material.HasProperty("_Mode"))
                {
                    int mode = (int)material.GetFloat("_Mode");
                    if (mode == 2 || mode == 3)
                    {
                        return true;
                    }
                }

                if (material.renderQueue >= 3000)
                {
                    return true;
                }

                if (material.IsKeywordEnabled("_ALPHABLEND_ON") ||
                    material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON") ||
                    material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))
                {
                    return true;
                }

                string colorProperty = ResolveColorProperty(material);
                return !string.IsNullOrEmpty(colorProperty) && material.GetColor(colorProperty).a < 0.99f;
            }
        }
    }
}
