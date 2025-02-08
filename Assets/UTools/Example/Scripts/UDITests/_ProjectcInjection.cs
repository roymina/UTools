using UnityEngine;
using UnityEngine.SceneManagement;
using UTools;

public class _ProjectInjection : MonoBehaviour
{
    [Inject] private ServiceA serviceA;
    [Inject] private _TestMono testMono;
    void Start()
    {
        Invoke("SayHello", 2);

    }

    void SayHello()
    {
        SceneManager.LoadScene("_TestDINewScene");
        serviceA.SayHello();
        testMono.SayHello();
    }

}
