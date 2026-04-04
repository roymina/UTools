using TMPro;
using UTools;
namespace UTools.Example
{
    public class _TestDINewScene : UBehaviour
    {
        [Inject] _TestServiceA _testService = null;
        [Inject] _TestMono _testMono = null;

        [Child] TextMeshProUGUI txtServiceInejction = null;
        [Child] TextMeshProUGUI txtMonoInejction = null;

        void Start()
        {
            txtServiceInejction.text = _testService.SayHello();
            txtMonoInejction.text = _testMono.SayHello();
        }


    }
}

