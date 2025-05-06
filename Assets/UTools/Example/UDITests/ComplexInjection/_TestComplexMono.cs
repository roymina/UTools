using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace UTools.Example
{
    public class _TestComplexMono : MonoBehaviour
    {
        // 注入数据实体，用于共享网页内容
        [Inject]
        private _TestDataEntity dataEntity;

  

        [PostInjection]
        void StartRequest()
        {
            if (dataEntity == null)
            {
                Debug.LogError("_TestComplexMono: dataEntity注入失败");
                return;
            }
            
            Debug.Log("_TestComplexMono: dataEntity注入成功，启动网络请求");
            // 启动协程获取网页内容
            StartCoroutine(GetUnityWebContent());
        }

        // 获取网页内容的协程方法
        IEnumerator GetUnityWebContent()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://bing.com/"))
            {
                // 发送请求
                yield return webRequest.SendWebRequest();

                // 检查是否有错误
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    dataEntity.WebContent = "Error loading website: " + webRequest.error;
                }
                else
                {
                    // 获取网页内容并保存到字段中
                    string content = webRequest.downloadHandler.text;

                    // 将网页内容存储到数据实体中，以便其他组件可以访问
                    if (dataEntity != null)
                    {
                        dataEntity.WebContent = "Website Content (Length: " + content.Length + " characters)";
                        Debug.Log("成功更新dataEntity中的WebContent");
                    }
                    else
                    {
                        Debug.LogError("dataEntity为null，无法更新WebContent");
                    }

                    Debug.Log("Website content loaded successfully!");
                }
            }
        }

        // 提供给外部调用的方法
        public void SayHello()
        {
            Debug.Log("Hello from _TestComplexMono!");
        }
    }
}

