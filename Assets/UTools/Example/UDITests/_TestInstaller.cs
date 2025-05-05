using UTools;

public class _TestInstaller : UDIInstallerBase
{
    protected override void RegisterServices()
    {
        Container.Register<_TestServiceA>();
        Container.Register<_TestMono>();
    }


}
