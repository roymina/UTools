using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace UTools
{
    public class UDIManager : MonoBehaviour
    {
        private static UDIManager _instance;

        public static UDIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UDIManager>();
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("UDIManager");
                        _instance = managerObject.AddComponent<UDIManager>();
                        DontDestroyOnLoad(managerObject);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            } 
        }
    } 
}
