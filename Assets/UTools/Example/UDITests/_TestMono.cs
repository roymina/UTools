using UnityEngine;

public class _TestMono : MonoBehaviour
{
    public string SayHello()
    {
        string str = "Hello from Mono Class";
        Debug.Log(str);
        return str;
    }
}
