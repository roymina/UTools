using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTools.Tests
{
    public class UBehaviourPlayModeTests
    {
        [UnityTest]
        public IEnumerator UBehaviour_AutoBindsChildrenAndComponents()
        {
            GameObject root = new("Root");
            root.AddComponent<BoxCollider>();

            GameObject child = new("Target");
            child.transform.SetParent(root.transform);
            child.AddComponent<BoxCollider>();

            SampleBehaviour behaviour = root.AddComponent<SampleBehaviour>();

            yield return null;

            Assert.That(behaviour.LocalCollider, Is.Not.Null);
            Assert.That(behaviour.Target, Is.EqualTo(child));

            Object.Destroy(root);
        }

        private sealed class SampleBehaviour : UBehaviour
        {
            [Comp]
            public BoxCollider LocalCollider;

            [Child("Target")]
            public GameObject Target;
        }
    }
}
