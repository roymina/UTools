using UnityEngine;

namespace UTools
{
    internal class BindingBuilder : IBindingBuilder
    {
        protected readonly BindInfo BindInfo;
        protected readonly UDIContainer Container;

        public BindingBuilder(BindInfo bindInfo, UDIContainer container)
        {
            BindInfo = bindInfo;
            Container = container;
        }

        public IBindingBuilder To<T>() where T : class
        {
            BindInfo.ConcreteType = typeof(T);
            return this;
        }

        public IBindingBuilder ToSelf()
        {
            BindInfo.ConcreteType = BindInfo.ContractType;
            return this;
        }

        public IBindingBuilder AsSingle()
        {
            BindInfo.Scope = BindingScope.Singleton;
            return this;
        }

        public IBindingBuilder AsTransient()
        {
            BindInfo.Scope = BindingScope.Transient;
            return this;
        }

        public IBindingBuilder InScope(BindingScope scope)
        {
            BindInfo.Scope = scope;
            return this;
        }

        public IBindingBuilder FromInstance(object instance)
        {
            BindInfo.Instance = instance;
            return this;
        }

        public IBindingBuilder FromGameObject(GameObject gameObject)
        {
            BindInfo.GameObjectContext = gameObject;
            return this;
        }

        public IBindingBuilder AsGlobal()
        {
            Container.MarkBindingGlobal(BindInfo);
            return this;
        }

        public IBindingBuilder RequiredForContextStart()
        {
            Container.MarkBindingRequiredForContextStart(BindInfo);
            return this;
        }

        public IBindingBuilder NonLazy()
        {
            BindInfo.NonLazy = true;
            Container.FinalizeBinding(BindInfo);
            return this;
        }
    }

    internal class BindingBuilder<TContract> : BindingBuilder, IBindingBuilder<TContract>
    {
        public BindingBuilder(BindInfo bindInfo, UDIContainer container)
            : base(bindInfo, container)
        {
        }

        public new IBindingBuilder<TContract> To<T>() where T : class, TContract
        {
            base.To<T>();
            return this;
        }

        public new IBindingBuilder<TContract> ToSelf()
        {
            base.ToSelf();
            return this;
        }

        public new IBindingBuilder<TContract> AsSingle()
        {
            base.AsSingle();
            return this;
        }

        public new IBindingBuilder<TContract> AsTransient()
        {
            base.AsTransient();
            return this;
        }

        public new IBindingBuilder<TContract> InScope(BindingScope scope)
        {
            base.InScope(scope);
            return this;
        }

        public IBindingBuilder<TContract> FromInstance(TContract instance)
        {
            base.FromInstance(instance);
            return this;
        }

        public new IBindingBuilder<TContract> AsGlobal()
        {
            base.AsGlobal();
            return this;
        }

        public new IBindingBuilder<TContract> RequiredForContextStart()
        {
            base.RequiredForContextStart();
            return this;
        }

        public new IBindingBuilder<TContract> NonLazy()
        {
            base.NonLazy();
            return this;
        }
    }
}
