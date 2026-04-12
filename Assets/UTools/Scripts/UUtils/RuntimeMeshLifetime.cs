using UnityEngine;

namespace UTools
{
    [DisallowMultipleComponent]
    internal sealed class RuntimeMeshLifetime : MonoBehaviour
    {
        [SerializeField]
        private Mesh ownedMesh;

        public void Initialize(Mesh mesh)
        {
            ownedMesh = mesh;
        }

        private void OnDestroy()
        {
            if (ownedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(ownedMesh);
            }
            else
            {
                DestroyImmediate(ownedMesh);
            }

            ownedMesh = null;
        }
    }
}
