using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

[CreateAssetMenu(menuName = "UTools/Global Installer")]
public class GameGlobalInstaller : GlobalInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<CountService>()
            .ToSelf()
            .AsSingle()
            .AsGlobal();

    }


}
