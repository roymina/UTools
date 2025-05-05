using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UTools;
public class _TestInjection : UBehaviour
{
    [Inject] _TestServiceA _testService;
    [Inject] _TestMono _testMono;

    [Child] TextMeshProUGUI txtServiceInejction;
    [Child] TextMeshProUGUI txtMonoInejction;
    [Child] Button btnLoadNewLevel, btnInstantiateButton;
    [SerializeField]
    GameObject InjectedButton;
    void Start()
    {
        txtServiceInejction.text = _testService.SayHello();
        txtMonoInejction.text = _testMono.SayHello();
        btnLoadNewLevel.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("_TestDINewScene");
        });
        btnInstantiateButton.onClick.AddListener(() =>
        {
            var go = UGameObjectFactory.InstantiateWithDependency(
                InjectedButton, btnInstantiateButton.transform.parent);
        });
    }


}
