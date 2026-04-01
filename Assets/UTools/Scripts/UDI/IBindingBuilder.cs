using UnityEngine;

namespace UTools
{
    public interface IBindingBuilder
    {
        IBindingBuilder To<T>() where T : class;
        IBindingBuilder ToSelf();
        IBindingBuilder AsSingle();
        IBindingBuilder AsTransient();
        IBindingBuilder InScope(BindingScope scope);
        IBindingBuilder FromInstance(object instance);
        IBindingBuilder FromGameObject(GameObject gameObject);
        IBindingBuilder NonLazy();
    }

    public interface IBindingBuilder<TContract> : IBindingBuilder
    {
        new IBindingBuilder<TContract> To<T>() where T : class, TContract;
        new IBindingBuilder<TContract> ToSelf();
        new IBindingBuilder<TContract> AsSingle();
        new IBindingBuilder<TContract> AsTransient();
        new IBindingBuilder<TContract> InScope(BindingScope scope);
        IBindingBuilder<TContract> FromInstance(TContract instance);
        new IBindingBuilder<TContract> NonLazy();
    }
}
