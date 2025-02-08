using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UTools;

public class _SceneInjection : MonoBehaviour
{
    [Inject] private ServiceA serviceA;
    [Inject] private _TestMono testMono;
    void Start()
    {
        serviceA.SayHello();
        testMono.SayHello();
    }


}
