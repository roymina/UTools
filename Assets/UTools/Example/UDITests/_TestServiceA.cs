using UnityEngine;
using UTools;

public class _TestServiceA
{
    [Inject]
    public void InjectServiceB(_TestServiceB _testServiceB)
    {
        _testServiceB.SayHello();
    }

    public string SayHello()
    {
        string str = "Hello from ServiceA";
        Debug.Log(str);
        return str;
    }
}
public class _TestServiceB
{
    public string SayHello()
    {
        string str = "Hello from ServiceB";
        Debug.Log(str);
        return str;
    }
}
