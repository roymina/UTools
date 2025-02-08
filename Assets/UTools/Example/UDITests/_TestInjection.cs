using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UTools;
public class _TestInjection : UBehaviour
{
    [Inject] _TestService _testService;
    [Inject] _TestMono _testMono;

    [Child] TextMeshProUGUI txtServiceInejction;
    [Child] TextMeshProUGUI txtMonoInejction;
    [Child] Button btnLoadNewLevel;
    void Start()
    {
        txtServiceInejction.text = _testService.SayHello();
        txtMonoInejction.text = _testMono.SayHello();
        btnLoadNewLevel.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("_TestDINewScene");
        });
    }


}
