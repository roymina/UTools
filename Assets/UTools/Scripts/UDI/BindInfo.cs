using System;
using UnityEngine;

namespace UTools
{
    public class BindInfo
    {
        public Type ContractType { get; set; }
        public Type ConcreteType { get; set; }
        public object Instance { get; set; }
        public object Identifier { get; set; }
        public BindingScope Scope { get; set; }
        public bool NonLazy { get; set; }
        public bool IsMonoBehaviour { get; set; }
        public GameObject GameObjectContext { get; set; }
    }

    public enum BindingScope
    {
        Transient,    // 每次解析创建新实例
        Singleton,    // 单例
        Scoped       // 作用域单例
    }
}