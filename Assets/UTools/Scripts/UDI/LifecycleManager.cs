using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UTools
{
    public class LifecycleManager : MonoBehaviour
    {
        private readonly List<IInitializable> _initializables = new List<IInitializable>();
        private readonly List<ITickable> _tickables = new List<ITickable>();
        private readonly List<IFixedTickable> _fixedTickables = new List<IFixedTickable>();
        private readonly List<ILateTickable> _lateTickables = new List<ILateTickable>();
        private readonly List<IUDisposable> _disposables = new List<IUDisposable>();
        private readonly List<IPausable> _pausables = new List<IPausable>();

        private bool _isPaused;

        public void Add(object instance)
        {
            if (instance == null) return;

            if (instance is IInitializable initializable)
                _initializables.Add(initializable);

            if (instance is ITickable tickable)
                _tickables.Add(tickable);

            if (instance is IFixedTickable fixedTickable)
                _fixedTickables.Add(fixedTickable);

            if (instance is ILateTickable lateTickable)
                _lateTickables.Add(lateTickable);

            if (instance is IUDisposable disposable)
                _disposables.Add(disposable);

            if (instance is IPausable pausable)
                _pausables.Add(pausable);
        }

        public void Remove(object instance)
        {
            if (instance == null) return;

            if (instance is IInitializable initializable)
                _initializables.Remove(initializable);

            if (instance is ITickable tickable)
                _tickables.Remove(tickable);

            if (instance is IFixedTickable fixedTickable)
                _fixedTickables.Remove(fixedTickable);

            if (instance is ILateTickable lateTickable)
                _lateTickables.Remove(lateTickable);

            if (instance is IUDisposable disposable)
                _disposables.Remove(disposable);

            if (instance is IPausable pausable)
                _pausables.Remove(pausable);
        }

        public void Initialize()
        {
            foreach (var initializable in _initializables.ToList())
            {
                initializable.Initialize();
            }
        }

        private void Update()
        {
            if (_isPaused) return;

            foreach (var tickable in _tickables.ToList())
            {
                tickable.Tick();
            }
        }

        private void FixedUpdate()
        {
            if (_isPaused) return;

            foreach (var fixedTickable in _fixedTickables.ToList())
            {
                fixedTickable.FixedTick();
            }
        }

        private void LateUpdate()
        {
            if (_isPaused) return;

            foreach (var lateTickable in _lateTickables.ToList())
            {
                lateTickable.LateTick();
            }
        }

        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;

            foreach (var pausable in _pausables)
            {
                pausable.Pause();
            }
        }

        public void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;

            foreach (var pausable in _pausables)
            {
                pausable.Resume();
            }
        }

        private void OnDestroy()
        {
            foreach (var disposable in _disposables.ToList())
            {
                disposable.Dispose();
            }

            _initializables.Clear();
            _tickables.Clear();
            _fixedTickables.Clear();
            _lateTickables.Clear();
            _disposables.Clear();
            _pausables.Clear();
        }
    }
}