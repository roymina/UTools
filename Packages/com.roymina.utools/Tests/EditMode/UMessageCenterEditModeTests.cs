using NUnit.Framework;

namespace UTools.Tests
{
    public class UMessageCenterEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            UMessageCenter.Instance.Clear();
        }

        [Test]
        public void PublishBeforeSubscribe_ReplaysPendingMessageToFirstSubscriber()
        {
            int received = 0;

            UMessageCenter.Instance.Publish(new TestMessage { Value = 7 });
            UMessageCenter.Instance.Subscribe<TestMessage>(message => received = message.Value);

            Assert.That(received, Is.EqualTo(7));
        }

        [Test]
        public void DisposeSubscription_StopsReceivingMessages()
        {
            int received = 0;

            IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<TestMessage>(_ => received++);
            subscription.Dispose();
            UMessageCenter.Instance.Publish(new TestMessage());

            Assert.That(received, Is.EqualTo(0));
        }

        private sealed class TestMessage
        {
            public int Value { get; set; }
        }
    }
}
