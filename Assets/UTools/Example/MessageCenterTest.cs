using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTools;

public class MessageCenterTest : MonoBehaviour
{

    void Start()
    {

        UMessageCenter.Instance.Subscribe<MyCustomMessage>(msg => Debug.Log(msg.ToString()));
        UMessageCenter.Instance.Publish(new MyCustomMessage { Name = "Hello World", Count = 20 });
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
