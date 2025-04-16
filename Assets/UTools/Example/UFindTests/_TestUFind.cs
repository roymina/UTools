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
    public bool UseDragGet = false;
    [ShowIf("UseDragGet")]
    public GameObject MyGameObject;
    void Start()
    {
        childA.color = UnityEngine.Color.cyan;
        childB.color = UnityEngine.Color.cyan;
        childC.color = UnityEngine.Color.cyan;
        imgUFind.sprite = ufind;
        if (!UseDragGet)
        {
            MyGameObject = gameObject;
        }
        Debug.Log(MyGameObject);
    }
}