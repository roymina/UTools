using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools
{
    public class Highlighter : MonoBehaviour
    {

        [Header("闪烁设置")]
        public Color highlightColor = Color.yellow; // 高亮颜色
        public float blinkSpeed = 2f; // 闪烁速度
        public float minIntensity = 0.2f; // 最小亮度
        public float maxIntensity = 1.0f; // 最大亮度
        [Tooltip("是否自动启用材质发光")]
        public bool autoEnableEmission = true;
        [Tooltip("更新频率(帧数)，值越大性能越好")]
        public int updateInterval = 2;
        [Tooltip("发光强度")]
        public float emissiveIntensity = 200f;

        private List<Renderer> renderers = new List<Renderer>();
        private List<Material> materials = new List<Material>();
        private List<Color> initialColors = new List<Color>();
        private float currentIntensity;
        private bool isHighlighting = false;
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
            // 确保初始状态正确
            ResetMaterials();
        }

        // 初始化材质，只执行一次
        private void InitializeMaterials()
        {
            if (materialsInitialized) return;

            // 获取所有渲染器组件
            renderers = GetComponentsInChildren<Renderer>().ToList();
            if (renderers.Count == 0)
            {
                Debug.LogWarning("没有找到任何渲染器组件，无法应用高亮效果");
                return;
            }

            // 收集所有材质
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                materials.AddRange(renderer.materials.ToList());
            }

            // 保存初始颜色
            foreach (Material material in materials)
            {
                initialColors.Add(material.color);
            }

            // 启用发光
            if (autoEnableEmission)
                EnableEmission();

            materialsInitialized = true;
        }

        void Update()
        {
            if (!isHighlighting || !materialsInitialized || materials.Count == 0)
                return;

            // 使用计数器减少更新频率
            frameCounter++;
            if (frameCounter < updateInterval)
                return;

            frameCounter = 0;

            // 使用PingPong函数产生平滑的闪烁效果
            float v = Mathf.PingPong(Time.time * blinkSpeed, 1);
            currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, v);

            // 预计算颜色值
            blinkColor = Color.Lerp(Color.white, highlightColor, v);

            ApplyHighlight(v);
        }

        // 开始高亮闪烁
        public void StartHighlight()
        {
            Debug.Log("StartHighlight");

            // 确保材质已初始化
            if (!materialsInitialized)
                InitializeMaterials();

            isHighlighting = true;
            frameCounter = 0;
        }

        // 停止高亮闪烁
        public void StopHighlight()
        {
            Debug.Log("StopHighlight");
            isHighlighting = false;
            ResetMaterials();
        }

        // 应用高亮效果
        private void ApplyHighlight(float intensity)
        {
            foreach (Material material in materials)
            {
                if (material == null) continue;

                // 设置发光属性
                material.SetColor("_EmissiveColor", blinkColor);
                material.SetFloat("_EmissiveIntensity", emissiveIntensity);
                material.SetFloat("_EmissiveExposureWeight", intensity);
                material.color = blinkColor;
            }
        }

        // 重置材质
        private void ResetMaterials()
        {
            if (!materialsInitialized || materials.Count == 0) return;

            // 恢复材质原始状态
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

        // 启用所有材质的发光功能
        private void EnableEmission()
        {
            if (materials.Count == 0) return;

            foreach (Material material in materials)
            {
                if (material == null) continue;

                // 设置发光属性为初始状态
                material.SetColor("_EmissiveColor", Color.black);
                material.SetFloat("_EmissiveIntensity", 0);
                material.SetFloat("_EmissiveExposureWeight", 1);
            }

            Debug.Log("已启用所有材质的发光功能并重置发光颜色");
        }

        // 当对象被禁用或销毁时重置状态
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