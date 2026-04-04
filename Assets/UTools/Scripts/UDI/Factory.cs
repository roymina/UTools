using System;
using UnityEngine;

namespace UTools
{
    public interface IFactory<T>
    {
        T Create();
    }

    public interface IFactory<TParam1, T>
    {
        T Create(TParam1 param);
    }

    public interface IFactory<TParam1, TParam2, T>
    {
        T Create(TParam1 param1, TParam2 param2);
    }

    public class PrefabFactory<T> : IFactory<T> where T : Component
    {
        readonly T _prefab;
        readonly UDIContainer _container;
        readonly Transform _parent;

        public PrefabFactory(T prefab, UDIContainer container, Transform parent = null)
        {
            _prefab = prefab;
            _container = container;
            _parent = parent;
        }

        public T Create()
        {
            var instance = GameObject.Instantiate(_prefab, _parent);
            _container.InjectGameObject(instance.gameObject);
            return instance;
        }
    }

    public class GameObjectFactory : IFactory<string, GameObject>
    {
        private readonly UDIContainer _container;
        private readonly Transform _parent;

        public GameObjectFactory(UDIContainer container, Transform parent = null)
        {
            _container = container;
            _parent = parent;
        }

        public GameObject Create(string name)
        {
            var instance = new GameObject(name);
            if (_parent != null)
            {
                instance.transform.SetParent(_parent);
            }
            _container.InjectGameObject(instance);
            return instance;
        }
    }
}