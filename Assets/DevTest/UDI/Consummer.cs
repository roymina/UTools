using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UTools;

public class Consummer : MonoBehaviour, IInitializable, ITickable
{
    [Inject]
    private ILogService _logService;
    [Inject]
    private CountService _countService;

    [Inject]
    private RemoteConfigService _remoteConfigService;

    [SerializeField]
    private GameObject _spwanerPrefab;

    void Start()
    {
        _logService.Log("Consummer has started and is using the LogService!");
    }

    [PostInjection]
    void PostInjection()
    {
        _logService.Log("PostInjection method called in Consummer.");
    }

    public void Initialize()
    {
        _logService.Log("Consummer has been initialized.");
    }
    Task counterTask;
    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //报错
            // GameObject enemy = Instantiate(_spwanerPrefab, Vector3.zero, Quaternion.identity);
            // enemy.GetComponent<Spwaner>().Spawn();

            //正确的做法是通过容器来实例化对象，以确保依赖注入正常工作
            GameObject enemy = UGameObjectFactory.InstantiateWithDependency(_spwanerPrefab, Vector3.zero, Quaternion.identity);
            enemy.GetComponent<Spwaner>().Spawn();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            counterTask = Task.Run(() =>
            {
                _countService.Count++;
                _logService.Log($"Count incremented to: {_countService.Count}");
            });
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (counterTask != null)
            {
                counterTask.Dispose();
                counterTask = null;
            }

        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SceneManager.LoadScene("UDITestScene2");
        }
    }


}
