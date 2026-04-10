using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

[CreateAssetMenu(menuName = "Game/ScObjInstaller")]
public class ScObjInstaller : ScriptableObjectInstaller
{
    [SerializeField] private GameData _gameData;
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<GameData>()
            .FromInstance(_gameData)
            .AsSingle();

    }


}
