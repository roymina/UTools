//-----------------------------------------------------------------------
// <copyright file="UMonoBehaviourHelper.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Provides a singleton MonoBehaviour helper for managing coroutines.
// </summary>
//-----------------------------------------------------------------------

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

