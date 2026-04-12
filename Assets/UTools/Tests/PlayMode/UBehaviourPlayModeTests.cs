using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
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

        [UnityTest]
        public IEnumerator UBehaviour_AutoBindsChildrenCollections()
        {
            GameObject root = CreateChildrenRoot();
            ChildrenCollectionBehaviour behaviour = root.AddComponent<ChildrenCollectionBehaviour>();

            yield return null;

            Assert.That(behaviour.NamedParent, Is.Not.Null);
            Assert.That(behaviour.NamedChildren, Has.Count.EqualTo(3));
            Assert.That(behaviour.NamedChildren, Has.All.Not.Null);
            Assert.That(behaviour.NamedChildren[0].name, Is.EqualTo("ActiveChild"));
            Assert.That(behaviour.NamedChildren[1].name, Is.EqualTo("InactiveChild"));
            Assert.That(behaviour.NamedChildren[2].name, Is.EqualTo("NestedParent"));

            Assert.That(behaviour.NamedDescendants, Has.Count.EqualTo(4));
            Assert.That(behaviour.NamedDescendants.Exists(child => child.name == "Grandchild"), Is.True);

            Assert.That(behaviour.ActiveOnlyChildren, Has.Count.EqualTo(2));
            Assert.That(behaviour.ActiveOnlyChildren.Exists(child => child.name == "InactiveChild"), Is.False);

            Assert.That(behaviour.NamedChildColliders, Has.Count.EqualTo(2));
            Assert.That(behaviour.NamedChildColliders[0].gameObject.name, Is.EqualTo("ActiveChild"));
            Assert.That(behaviour.NamedChildColliders[1].gameObject.name, Is.EqualTo("InactiveChild"));

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator UBehaviour_LogsErrorsForMissingParentAndInvalidChildrenList()
        {
            LogAssert.Expect(LogType.Error, new Regex("GameObject 'MissingParent' not found under Root", RegexOptions.IgnoreCase));
            LogAssert.Expect(LogType.Error, new Regex("ChildrenAttribute on InvalidValues must target List<GameObject> or List<Component>", RegexOptions.IgnoreCase));

            GameObject root = CreateChildrenRoot();
            InvalidChildrenCollectionBehaviour behaviour = root.AddComponent<InvalidChildrenCollectionBehaviour>();

            yield return null;

            Assert.That(behaviour.MissingParent, Is.Null);
            Assert.That(behaviour.InvalidValues, Is.Null);

            Object.Destroy(root);
        }

        private static GameObject CreateChildrenRoot()
        {
            GameObject root = new("Root");

            GameObject namedParent = new("NamedParent");
            namedParent.transform.SetParent(root.transform);

            GameObject activeChild = new("ActiveChild");
            activeChild.transform.SetParent(namedParent.transform);
            activeChild.AddComponent<BoxCollider>();

            GameObject inactiveChild = new("InactiveChild");
            inactiveChild.transform.SetParent(namedParent.transform);
            inactiveChild.AddComponent<BoxCollider>();
            inactiveChild.SetActive(false);

            GameObject nestedParent = new("NestedParent");
            nestedParent.transform.SetParent(namedParent.transform);

            GameObject grandchild = new("Grandchild");
            grandchild.transform.SetParent(nestedParent.transform);
            grandchild.AddComponent<BoxCollider>();

            return root;
        }

        private sealed class SampleBehaviour : UBehaviour
        {
            [Comp]
            public BoxCollider LocalCollider;

            [Child("Target")]
            public GameObject Target;
        }

        private sealed class ChildrenCollectionBehaviour : UBehaviour
        {
            [Children]
            public List<GameObject> NamedParent;

            [Children("NamedParent")]
            public List<GameObject> NamedChildren;

            [Children("NamedParent", includeDescendants = true)]
            public List<GameObject> NamedDescendants;

            [Children("NamedParent", includeInactive = false)]
            public List<GameObject> ActiveOnlyChildren;

            [Children("NamedParent")]
            public List<BoxCollider> NamedChildColliders;
        }

        private sealed class InvalidChildrenCollectionBehaviour : UBehaviour
        {
            [Children("MissingParent")]
            public List<GameObject> MissingParent;

            [Children("NamedParent")]
            public List<int> InvalidValues;
        }
    }
}
