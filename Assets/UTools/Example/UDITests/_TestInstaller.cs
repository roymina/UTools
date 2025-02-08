using UTools;

public class _TestInstaller : UDIInstallerBase
{


    protected override void RegisterGlobalServices()
    {
        Container.Register<_TestService>();
        Container.Register<_TestMono>();
    }

    protected override void RegisterSceneServices()
    {
        Container.Register<_TestService>();
        Container.Register<_TestMono>();
    }
}
