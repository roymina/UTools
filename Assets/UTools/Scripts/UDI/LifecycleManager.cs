using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools
{
    public class LifecycleManager : MonoBehaviour
    {
        private readonly List<IInitializable> _initializables = new();
        private readonly List<ITickable> _tickables = new();
        private readonly List<IFixedTickable> _fixedTickables = new();
        private readonly List<ILateTickable> _lateTickables = new();
        private readonly List<IUDisposable> _disposables = new();
        private readonly List<IPausable> _pausables = new();
        private readonly HashSet<object> _trackedInstances = new();

        private bool _isPaused;
        private bool _hasInitialized;
        private bool _isDisposing;

        public void Add(object instance)
        {
            if (instance == null || _isDisposing || !_trackedInstances.Add(instance))
            {
                return;
            }

            if (instance is IInitializable initializable)
            {
                _initializables.Add(initializable);
                if (_hasInitialized)
                {
                    initializable.Initialize();
                }
            }

            if (instance is ITickable tickable)
            {
                _tickables.Add(tickable);
            }

            if (instance is IFixedTickable fixedTickable)
            {
                _fixedTickables.Add(fixedTickable);
            }

            if (instance is ILateTickable lateTickable)
            {
                _lateTickables.Add(lateTickable);
            }

            if (instance is IUDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            if (instance is IPausable pausable)
            {
                _pausables.Add(pausable);
            }
        }

        public void Remove(object instance)
        {
            if (instance == null || !_trackedInstances.Remove(instance))
            {
                return;
            }

            if (instance is IInitializable initializable)
            {
                _initializables.Remove(initializable);
            }

            if (instance is ITickable tickable)
            {
                _tickables.Remove(tickable);
            }

            if (instance is IFixedTickable fixedTickable)
            {
                _fixedTickables.Remove(fixedTickable);
            }

            if (instance is ILateTickable lateTickable)
            {
                _lateTickables.Remove(lateTickable);
            }

            if (instance is IUDisposable disposable)
            {
                _disposables.Remove(disposable);
            }

            if (instance is IPausable pausable)
            {
                _pausables.Remove(pausable);
            }
        }

        public void Initialize()
        {
            if (_hasInitialized)
            {
                return;
            }

            _hasInitialized = true;
            foreach (var initializable in _initializables.ToList())
            {
                initializable.Initialize();
            }
        }

        private void Update()
        {
            if (_isPaused)
            {
                return;
            }

            foreach (var tickable in _tickables.ToList())
            {
                tickable.Tick();
            }
        }

        private void FixedUpdate()
        {
            if (_isPaused)
            {
                return;
            }

            foreach (var fixedTickable in _fixedTickables.ToList())
            {
                fixedTickable.FixedTick();
            }
        }

        private void LateUpdate()
        {
            if (_isPaused)
            {
                return;
            }

            foreach (var lateTickable in _lateTickables.ToList())
            {
                lateTickable.LateTick();
            }
        }

        public void Pause()
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;
            foreach (var pausable in _pausables.ToList())
            {
                pausable.Pause();
            }
        }

        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;
            foreach (var pausable in _pausables.ToList())
            {
                pausable.Resume();
            }
        }

        private void OnDestroy()
        {
            _isDisposing = true;
            foreach (var disposable in _disposables.ToList())
            {
                disposable.Dispose();
            }

            _trackedInstances.Clear();
            _initializables.Clear();
            _tickables.Clear();
            _fixedTickables.Clear();
            _lateTickables.Clear();
            _disposables.Clear();
            _pausables.Clear();
        }
    }
}
