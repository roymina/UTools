using UnityEngine;
using System.Collections.Generic;

namespace UTools
{
    public class UDIContext : MonoBehaviour
    {
        [SerializeField]
        private List<MonoInstaller> _installers = new List<MonoInstaller>();
        [SerializeField]
        private List<ScriptableObjectInstaller> _scriptableObjectInstallers = new List<ScriptableObjectInstaller>();

        private UDIContainer _container;
        private LifecycleManager _lifecycleManager;
        private bool _hasInitialized;

        public UDIContainer Container => _container;
        public LifecycleManager LifecycleManager => _lifecycleManager;

        protected virtual void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_hasInitialized) return;
            _hasInitialized = true;

            // 创建并配置生命周期管理器
            _lifecycleManager = gameObject.AddComponent<LifecycleManager>();

            // 获取父级Context的容器作为父容器
            var parentContext = GetParentContext();
            var parentContainer = parentContext?.Container;

            // 创建当前Context的容器
            _container = new UDIContainer(parentContainer);

            // 注册生命周期管理器
            _container.Bind<LifecycleManager>()
                     .FromInstance(_lifecycleManager)
                     .AsSingle();

            // 安装所有配置的安装器
            InstallBindings();

            // 初始化所有实现了IInitializable的服务
            _lifecycleManager.Initialize();
        }

        private void InstallBindings()
        {
            // 首先安装ScriptableObject安装器
            foreach (var installer in _scriptableObjectInstallers)
            {
                if (installer != null)
                {
                    installer.InstallBindings(_container);
                }
            }

            // 然后安装MonoBehaviour安装器
            foreach (var installer in _installers)
            {
                if (installer != null)
                {
                    installer.InstallBindings(_container);
                }
            }
        }

        private UDIContext GetParentContext()
        {
            var parent = transform.parent;
            while (parent != null)
            {
                var context = parent.GetComponent<UDIContext>();
                if (context != null)
                {
                    return context;
                }
                parent = parent.parent;
            }
            return null;
        }
    }

    // MonoBehaviour形式的安装器基类
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void InstallBindings(UDIContainer container);
    }

    // ScriptableObject形式的安装器基类
    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void InstallBindings(UDIContainer container);
    }

    // 安装器接口
    public interface IInstaller
    {
        void InstallBindings(UDIContainer container);
    }
}