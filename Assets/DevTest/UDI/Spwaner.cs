using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class Spwaner : MonoBehaviour
{
    [Inject]
    private ILogService _logService;

    void Start()
    {
        _logService.Log("Spwaner has started and is using the LogService!");
    }

    public void Spawn()
    {
        _logService.Log("Spwaner Spawn method called.");
    }


}
