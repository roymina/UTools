using System;
using UnityEngine;

namespace UTools
{
    public interface IBindingBuilder
    {
        IBindingBuilder To<T>() where T : class;
        IBindingBuilder ToSelf();
        IBindingBuilder WithId(object identifier);
        IBindingBuilder AsSingle();
        IBindingBuilder AsTransient();
        IBindingBuilder InScope(BindingScope scope);
        IBindingBuilder OnInstantiated(Action<object> action);
        IBindingBuilder FromInstance(object instance);
        IBindingBuilder FromGameObject(GameObject gameObject);
        IBindingBuilder NonLazy();
    }

    public interface IBindingBuilder<TContract> : IBindingBuilder
    {
        new IBindingBuilder<TContract> To<T>() where T : class, TContract;
        new IBindingBuilder<TContract> ToSelf();
        new IBindingBuilder<TContract> WithId(object identifier);
        new IBindingBuilder<TContract> AsSingle();
        new IBindingBuilder<TContract> AsTransient();
        new IBindingBuilder<TContract> InScope(BindingScope scope);
        IBindingBuilder<TContract> OnInstantiated(Action<TContract> action);
        IBindingBuilder<TContract> FromInstance(TContract instance);
        new IBindingBuilder<TContract> NonLazy();
    }
}