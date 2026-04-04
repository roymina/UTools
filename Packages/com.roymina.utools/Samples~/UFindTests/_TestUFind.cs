using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public class _TestUFind : UBehaviour
{
    [Child] TextMeshProUGUI childA = null, childB = null;
    [Child("customName")] TextMeshProUGUI childC = null;
    [Child] Image imgUFind = null;
    [Resource] Sprite ufind = null;
    public bool UseDragGet = false;
    [ShowIf("UseDragGet")]
    public GameObject MyGameObject = null;
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
