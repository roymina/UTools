//-----------------------------------------------------------------------
// <copyright file="UDIInstallerBase.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// Base class for dependency injection installers, managing global and scene-specific services.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
namespace UTools
{
    public abstract class UDIInstallerBase : MonoBehaviour
    {
        protected static readonly UDIContainer Container = new UDIContainer();

        protected abstract void RegisterServices();


        private void Awake()
        {
            // 设置全局容器引用，供 UGameObjectFactory 使用
            UGameObjectFactory.SetContainer(Container);

            //execute methods in inherited classes
            RegisterServices();
            InjectAll();
        }
        private void InjectAll()
        {
            //inject mono behaviors  
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Container.InjectDependencies(allComponents);
            //inject services
            Container.InjectDependencies();
        }
    }
}

