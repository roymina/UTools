using UnityEngine;

namespace UTools
{

    public class MonoBehaviourHelper : MonoBehaviour
    {
        private static MonoBehaviourHelper _instance;
        public static MonoBehaviourHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("MonoBehaviourHelper");
                    _instance = obj.AddComponent<MonoBehaviourHelper>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }
}

