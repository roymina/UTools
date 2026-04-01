using UnityEngine;

namespace UTools
{
    // 一些示例服务
    public interface IDataService
    {
        string GetData();
    }

    public class DataService : IDataService
    {
        public string GetData() => "Some data";
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class UnityLogger : ILogger, IInitializable
    {
        public void Initialize()
        {
            Debug.Log("Logger initialized");
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }
    }

    public class GameManager : IInitializable, ITickable
    {
        [Inject] private ILogger _logger = null;
        [Inject] private IDataService _dataService = null;

        public void Initialize()
        {
            _logger.Log("GameManager initialized");
            _logger.Log(_dataService.GetData());
        }

        public void Tick()
        {
            // 每帧执行的逻辑
        }
    }

    // 示例安装器
    public class ExampleInstaller : MonoInstaller
    {
        [SerializeField]
        private Transform _spawnPoint;

        public override void InstallBindings(UDIContainer container)
        {
            // 基本服务绑定
            container.Bind<ILogger>()
                    .To<UnityLogger>()
                    .AsSingle();

            container.Bind<IDataService>()
                    .To<DataService>()
                    .AsSingle();

            // 使用字段注入的服务
            container.Bind<GameManager>()
                    .ToSelf()
                    .AsSingle()
                    .NonLazy();

            // GameObject工厂绑定示例（需要先实现 BindFactory 方法）
            // container.Bind<IFactory<string, GameObject>>()
            //         .To<GameObjectFactory>()
            //         .AsSingle();
        }
    }
}
