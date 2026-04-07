using UnityEngine;

namespace UTools
{
    public class BindInfo
    {
        public System.Type ContractType { get; set; }
        public System.Type ConcreteType { get; set; }
        public object Instance { get; set; }
        public BindingScope Scope { get; set; }
        public bool NonLazy { get; set; }
        public bool IsMonoBehaviour { get; set; }
        public GameObject GameObjectContext { get; set; }
        public bool IsGlobal { get; set; }
        public bool RequiredForContextStart { get; set; }
    }

    public enum BindingScope
    {
        Transient,
        Singleton,
        Scoped
    }
}
