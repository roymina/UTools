using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UTools;
namespace UTools.Example
{
    public class _TestInjection : UBehaviour
    {
        [Inject] _TestServiceA _testServiceA;
        [Inject] _TestServiceC _testServiceC;
        [Inject] _TestMono _testMono;

        [Child] TextMeshProUGUI txtServiceInjection, txtNestedInjection, txtPostInjection, txtMonoInjection;
        [Child] Button btnLoadNewLevel, btnInstantiateButton;
        [SerializeField] GameObject InjectedButton;
        void Start()
        {
            txtServiceInjection.text = _testServiceA.SayHello();

            txtMonoInjection.text = _testMono.SayHello();

            txtNestedInjection.text = _testServiceA.TestServiceB.SayHello();

            txtPostInjection.text = _testServiceC.Message;


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
}
