using UTools;
namespace UTools.Example
{
    public class _TestInstaller : MonoInstaller
    {
        public override void InstallBindings(UDIContainer container)
        {
            container.Bind<_TestServiceA>()
                .AsSingle();

            container.Bind<_TestServiceC>()
                .AsSingle();

            container.Bind<_TestMono>()
                .AsSingle();
        }
    }
}
