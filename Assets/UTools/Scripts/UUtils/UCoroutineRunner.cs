using System.Collections;
using UnityEngine;

namespace UTools
{
    internal static class UCoroutineRunner
    {
        private static RunnerBehaviour defaultRunner;

        public static Coroutine Start(IEnumerator routine, MonoBehaviour runner = null)
        {
            if (routine == null)
            {
                return null;
            }

            MonoBehaviour resolvedRunner = runner != null ? runner : GetDefaultRunner();
            return resolvedRunner.StartCoroutine(routine);
        }

        private static RunnerBehaviour GetDefaultRunner()
        {
            if (defaultRunner != null)
            {
                return defaultRunner;
            }

            GameObject runnerObject = new("UTools Coroutine Runner");
            Object.DontDestroyOnLoad(runnerObject);
            defaultRunner = runnerObject.AddComponent<RunnerBehaviour>();
            return defaultRunner;
        }

        private sealed class RunnerBehaviour : MonoBehaviour
        {
        }
    }
}
