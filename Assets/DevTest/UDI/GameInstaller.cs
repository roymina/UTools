using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        // Bind your game services and dependencies here
        // Example:
        // container.Bind<GameManager>().AsSingle();
        // container.Bind<PlayerController>().AsSingle();
        container.Bind<ILogService>()
            .To<LogService>()
            .AsSingle();
    }
}
