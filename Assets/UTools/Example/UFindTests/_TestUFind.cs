using TMPro;
using UTools;

public class _TestUFind : UBehaviour
{
    [Child] TextMeshProUGUI childA, childB;
    void Start()
    {
        childA.color = UnityEngine.Color.cyan;
        childB.color = UnityEngine.Color.cyan;
    }
}