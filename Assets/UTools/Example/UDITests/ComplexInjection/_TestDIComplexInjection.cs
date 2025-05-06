using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UTools.Example
{
    public class _TestDIComplexInjection : UBehaviour
    {
        [Inject]
        _TestDataEntity dataEntity;
        [Inject]
        _TestComplexMono complexMono;
        [Child]
        TextMeshProUGUI txtWebContent;
        
        // 不要在Start中访问注入的依赖，可能尚未完成注入
        // void Start()
        // {
        //     // ...
        // }
        
        // 使用PostInjection标记的方法会在依赖注入完成后被调用
        [PostInjection]
        void AfterInjection()
        {
            // 确保注入的对象不为null
            if (dataEntity != null)
            {
                Debug.Log("dataEntity注入成功");
                if (txtWebContent != null)
                    txtWebContent.text = dataEntity.WebContent;
            }
            else
            {
                Debug.LogError("dataEntity注入失败，对象为null");
            }
           
            if (complexMono != null)
            {
                Debug.Log("complexMono注入成功");
                complexMono.SayHello();
            }
            else
            {
                Debug.LogError("complexMono注入失败，对象为null");
            }
        }
    }
}

