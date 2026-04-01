namespace UTools.Example
{
    public class _TestInstaller2 : MonoInstaller
    {
        public override void InstallBindings(UDIContainer container)
        {
            container.Bind<_TestDataEntity>()
                .AsSingle();
        }
    }
}

