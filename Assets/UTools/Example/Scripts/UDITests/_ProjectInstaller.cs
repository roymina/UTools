using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class _ProjectInstaller : UDIInstaller
{

    protected override void RegisterServices()
    {
        Container.Register<ServiceA>();
    }

}
