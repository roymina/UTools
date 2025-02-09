using TMPro;
using UnityEngine.UI;
using UTools;

public class _TestMessage : UBehaviour
{
    [Child] Button btnPublisher;
    [Child] TextMeshProUGUI txtSubscriber1, txtSubscriber2, txtSubscriber3;
    void Start()
    {
        UMessageCenter.Instance.Subscribe<MyCustomMessage>(msg =>
        {
            txtSubscriber1.text = txtSubscriber2.text = txtSubscriber3.text = msg.ToString();
        });

        btnPublisher.onClick.AddListener(() =>
        {
            UMessageCenter.Instance.Publish(new MyCustomMessage { Name = "MyCustomMessage" });
        });
    }


}

public class MyCustomMessage
{
    public string Name { get; set; }

    public override string ToString()
    {
        return $"Hello from : {Name}";
    }
}
