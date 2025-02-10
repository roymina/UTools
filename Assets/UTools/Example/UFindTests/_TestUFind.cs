using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public class _TestUFind : UBehaviour
{
    [Child] TextMeshProUGUI childA, childB;
    [Child("customName")] TextMeshProUGUI childC;
    [Child] Image imgUFind;
    [Resource] Sprite ufind;
    void Start()
    {
        childA.color = UnityEngine.Color.cyan;
        childB.color = UnityEngine.Color.cyan;
        childC.color = UnityEngine.Color.cyan;
        imgUFind.sprite = ufind;
    }
}