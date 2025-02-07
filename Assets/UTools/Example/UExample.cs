using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class UExample : MonoBehaviour
{
    [Inject] private MyService myService;
    void Start()
    {

        UMessageCenter.Instance.Subscribe<MyCustomMessage>(msg => Debug.Log(msg.ToString()));
        UMessageCenter.Instance.Publish(new MyCustomMessage { Name = "Hello World", Count = 20 });

        UInjector.Inject(this);
        myService.DoSomething();
    }


}

public class MyCustomMessage
{
    public string Name { get; set; }
    public int Count { get; set; }

    public override string ToString()
    {
        return $"Name:{Name}, Count:{Count}";
    }
}


public class MyService
{
    public void DoSomething()
    {
        Debug.Log("MyService is doing something.");
    }
}
