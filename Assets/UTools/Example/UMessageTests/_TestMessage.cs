using TMPro;
using UnityEngine.UI;
using UTools;

public class _TestMessage : UBehaviour
{
    [Child] Button btnPublisher = null;
    [Child] TextMeshProUGUI txtSubscriber1 = null, txtSubscriber2 = null, txtSubscriber3 = null;
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
