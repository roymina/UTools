using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTools;
namespace UTools.Example
{
    public class _TestInstantiateInjection : MonoBehaviour
    {
        [Inject]
        _TestServiceA serviceA = null;
        void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(() =>
                GetComponentInChildren<TextMeshProUGUI>().text = serviceA.SayHello()
            );
        }
    }
}
