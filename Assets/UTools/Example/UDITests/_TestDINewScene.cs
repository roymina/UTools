using TMPro;
using UTools;
public class _TestDINewScene : UBehaviour
{
    [Inject] _TestServiceA _testService;
    [Inject] _TestMono _testMono;

    [Child] TextMeshProUGUI txtServiceInejction;
    [Child] TextMeshProUGUI txtMonoInejction;

    void Start()
    {
        txtServiceInejction.text = _testService.SayHello();
        txtMonoInejction.text = _testMono.SayHello();
    }


}
