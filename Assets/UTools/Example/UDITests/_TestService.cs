using UnityEngine;

public class _TestService
{
    public string SayHello()
    {
        string str = "Hello from ServiceA";
        Debug.Log(str);
        return str;
    }
}
