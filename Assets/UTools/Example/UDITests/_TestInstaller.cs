using UTools;
namespace UTools.Example
{
    public class _TestInstaller : UDIInstallerBase
    {
        protected override void RegisterServices()
        {
            Container.Register<_TestServiceA>();
            Container.Register<_TestServiceC>();
            Container.Register<_TestMono>();
        }
    }
}
