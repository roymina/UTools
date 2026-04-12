using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public class UFindTest : UBehaviour
{
    [Child] private GameObject Slot_01;
    [Child("Slot_02")] private GameObject Slot02;
    [Child] private GameObject DisabledContainer;
    [Child] Light LightProbe;
    [Resource("Prefabs/Sphere")] GameObject SpherePrefab;
    [Children(includeDescendants = true)] private List<GameObject> SlotGrid;
    [Children("SlotGrid")] private List<Button> childButtons;

    private
    void Start()
    {
        Slot_01.SetActive(false);
        Slot02.SetActive(false);
        DisabledContainer.SetActive(true);
        LightProbe.intensity = 50f;
        Instantiate(SpherePrefab, Vector3.zero, Quaternion.identity);

        foreach (var item in SlotGrid)
        {
            Debug.Log("SlotGrid: " + item.name);
        }
        // foreach (var item in childButtons)
        // {
        //     Debug.Log("Child Button: " + item.gameObject.name);
        // }
    }


}
