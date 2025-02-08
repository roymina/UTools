using UnityEngine;
using UTools;

public class _SceneInstaller : UDIInstaller
{
    protected override void RegisterServices()
    {
        Container.Register<ServiceA>();
    }

}

public class ServiceA
{
    public void SayHello()
    {
        Debug.Log("hello world");
    }
}

