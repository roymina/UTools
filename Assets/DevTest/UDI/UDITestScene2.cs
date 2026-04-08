using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class UDITestScene2 : MonoBehaviour
{
    [Inject]
    private CountService _countService;
    void Start()
    {
        Debug.Log("new scene:_countService.count: " + _countService.Count);
    }


}
