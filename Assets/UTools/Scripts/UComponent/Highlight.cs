using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools
{
    public class Highlight : MonoBehaviour
    {

        [Header("闪烁设置")]
        public Color highlightColor = Color.yellow; // 高亮颜色
        public float blinkSpeed = 3.5f; // 闪烁速度
        public float minIntensity = 0.2f; // 最小亮度
        public float maxIntensity = 1.0f; // 最大亮度
        [Tooltip("是否自动启用材质发光")]
        public bool autoEnableEmission = true;
        [Tooltip("更新频率(帧数)，值越大性能越好")]
        public int updateInterval = 2;

        private Renderer[] renderers;
        private MaterialPropertyBlock propertyBlock;
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
            renderers = GetComponentsInChildren<Renderer>();

            // 创建一次PropertyBlock
            propertyBlock = new MaterialPropertyBlock();

            // 启用发光
            if (autoEnableEmission)
                EnableEmission();

            materialsInitialized = true;
        }

        void Update()
        {
            if (!isHighlighting)
                return;

            // 使用计数器减少更新频率
            frameCounter++;
            if (frameCounter < updateInterval)
                return;

            frameCounter = 0;

            // 使用正弦函数产生平滑的闪烁效果
            currentIntensity = Mathf.Lerp(minIntensity, maxIntensity,
                (Mathf.Sin(Time.time * blinkSpeed) + 1) * 0.5f);

            // 预计算颜色值
            blinkColor = highlightColor * currentIntensity;

            ApplyHighlight();
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
        private void ApplyHighlight()
        {
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning("没有找到任何渲染器组件，无法应用高亮效果");
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                // 一次性获取属性块并修改所有材质
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_EmissionColor", blinkColor);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        // 重置材质
        private void ResetMaterials()
        {
            if (renderers == null || renderers.Length == 0) return;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                // 一次性获取属性块并修改所有材质
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_EmissionColor", Color.black);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        // 启用所有材质的发光功能
        private void EnableEmission()
        {
            if (renderers == null || renderers.Length == 0) return;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    // 确保材质支持发光
                    material.EnableKeyword("_EMISSION");

                    // 设置全局光照发光颜色为黑色（无发光）
                    material.SetColor("_EmissionColor", Color.black);

                    // 设置全局光照发光颜色
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
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