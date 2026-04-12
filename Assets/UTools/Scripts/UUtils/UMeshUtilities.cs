using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UTools
{
    public static class UMeshUtilities
    {
        public static void ToggleMesh(this GameObject self, bool show = false, bool selfOnly = false)
        {
            if (self == null)
            {
                return;
            }

            if (selfOnly)
            {
                SetRendererEnabled(self.GetComponent<MeshRenderer>(), show);
                SetRendererEnabled(self.GetComponent<SkinnedMeshRenderer>(), show);
                return;
            }

            foreach (MeshRenderer renderer in self.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = show;
            }

            foreach (SkinnedMeshRenderer renderer in self.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                renderer.enabled = show;
            }
        }

        public static GameObject CloneMesh(this GameObject self, Material material = null, string name = null)
        {
            if (self == null)
            {
                return null;
            }

            GameObject clone = new(name ?? $"{self.name}_MeshClone");
            clone.transform.SetParent(self.transform.parent, false);
            clone.transform.localPosition = self.transform.localPosition;
            clone.transform.localRotation = self.transform.localRotation;
            clone.transform.localScale = self.transform.localScale;

            if (self.TryGetComponent(out SkinnedMeshRenderer skinnedSource))
            {
                SkinnedMeshRenderer skinnedClone = clone.AddComponent<SkinnedMeshRenderer>();
                skinnedClone.sharedMesh = skinnedSource.sharedMesh;
                skinnedClone.rootBone = skinnedSource.rootBone;
                skinnedClone.bones = skinnedSource.bones;
                skinnedClone.sharedMaterials = material == null
                    ? skinnedSource.sharedMaterials
                    : new[] { material };
                return clone;
            }

            if (self.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                MeshFilter cloneFilter = clone.AddComponent<MeshFilter>();
                MeshRenderer cloneRenderer = clone.AddComponent<MeshRenderer>();
                Renderer sourceRenderer = self.GetComponent<Renderer>();

                cloneFilter.sharedMesh = meshFilter.sharedMesh;
                cloneRenderer.sharedMaterials = material == null && sourceRenderer != null
                    ? sourceRenderer.sharedMaterials
                    : new[] { material };
                return clone;
            }

            DestroyUnityObject(clone);
            return null;
        }

        public static GameObject CombineMesh(this GameObject self, string name = null, Transform parent = null)
        {
            if (self == null)
            {
                return null;
            }

            MeshFilter[] meshFilters = self
                .GetComponentsInChildren<MeshFilter>(true)
                .Where(meshFilter => meshFilter.sharedMesh != null)
                .ToArray();

            if (meshFilters.Length == 0)
            {
                Debug.LogWarning($"No MeshFilter found under {self.name}.");
                return null;
            }

            Transform targetParent = parent ?? self.transform;
            Matrix4x4 worldToLocal = targetParent.worldToLocalMatrix;
            List<CombineInstance> combineInstances = new(meshFilters.Length);
            int vertexCount = 0;
            Material material = null;

            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.sharedMesh;
                vertexCount += mesh.vertexCount;

                combineInstances.Add(new CombineInstance
                {
                    mesh = mesh,
                    transform = worldToLocal * meshFilter.transform.localToWorldMatrix,
                });

                if (material == null && meshFilter.TryGetComponent(out Renderer renderer))
                {
                    material = renderer.sharedMaterial;
                }
            }

            Mesh combinedMesh = new()
            {
                name = name == null ? $"{self.name}_CombinedMesh" : $"{name}_Mesh",
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
            };
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateBounds();

            GameObject combinedObject = CreateMeshObject(name ?? $"{self.name}_Combined", combinedMesh, material);
            combinedObject.transform.SetParent(targetParent, false);
            combinedObject.transform.localPosition = Vector3.zero;
            combinedObject.transform.localRotation = Quaternion.identity;
            combinedObject.transform.localScale = Vector3.one;
            return combinedObject;
        }

        public static Bounds GetMeshFilterBounds(this Transform objectTransform, bool includeChildren = true)
        {
            if (objectTransform == null)
            {
                return new Bounds();
            }

            Renderer[] renderers = includeChildren
                ? objectTransform.GetComponentsInChildren<Renderer>(true)
                : objectTransform.GetComponents<Renderer>();

            if (renderers.Length == 0)
            {
                return new Bounds(objectTransform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        public static GameObject GenerateSkirtingLine(
            Vector3 center,
            Vector2 extent,
            Material material,
            float standardWidth = 10f,
            float standardHeight = 10f,
            float fixedWidth = 0.01f,
            float fixedHeight = 0.08f)
        {
            float halfExtentY = extent.y * 0.5f;
            Vector3 realCenter = center
                - new Vector3(0, halfExtentY, 0)
                + new Vector3(0, fixedHeight * 0.5f, fixedWidth * 0.5f);
            float realHalfExtentX = extent.x * 0.5f;
            float realHalfExtentY = fixedHeight * 0.5f;
            float realHalfExtentZ = fixedWidth * 0.5f;

            float bottomY = realCenter.y - realHalfExtentY;
            float topY = realCenter.y + realHalfExtentY;
            float leftX = realCenter.x - realHalfExtentX;
            float rightX = realCenter.x + realHalfExtentX;
            float farZ = realCenter.z - realHalfExtentZ;
            float nearZ = realCenter.z + realHalfExtentZ;

            Vector3[] vertices =
            {
                new(rightX, topY, nearZ),
                new(rightX, topY, farZ),
                new(leftX, topY, farZ),
                new(leftX, topY, nearZ),
                new(leftX, topY, nearZ),
                new(leftX, bottomY, nearZ),
                new(rightX, bottomY, nearZ),
                new(rightX, topY, nearZ),
            };

            float u = SafeDivide(extent.x, standardWidth);
            float v = SafeDivide(fixedHeight, standardHeight);
            Vector2[] uvs =
            {
                new(u, 0),
                new(u, v),
                new(0, v),
                new(0, 0),
                new(0, v),
                new(0, 0),
                new(u, 0),
                new(u, v),
            };

            int[] triangles = { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4 };
            return CreateMeshObject("SkirtingLine", CreateRuntimeMesh("SkirtingLineMesh", vertices, uvs, triangles), material);
        }

        public static GameObject GenerateQuadMesh(
            Vector3 center,
            Vector2 extent,
            Material material,
            float standardWidth = 10f,
            float standardHeight = 10f)
        {
            const float expand = 0.01f;
            float halfExtentX = extent.x * 0.5f;
            float halfExtentY = extent.y * 0.5f;

            Vector3[] vertices =
            {
                new(center.x - halfExtentX - expand, center.y - halfExtentY - expand, center.z),
                new(center.x + halfExtentX + expand, center.y - halfExtentY - expand, center.z),
                new(center.x + halfExtentX + expand, center.y + halfExtentY + expand, center.z),
                new(center.x - halfExtentX - expand, center.y + halfExtentY + expand, center.z),
            };

            Vector2[] uvs = CalculateQuadUvs(extent, standardWidth, standardHeight);
            int[] triangles = { 0, 1, 2, 2, 3, 0 };
            return CreateMeshObject("QuadMesh", CreateRuntimeMesh("QuadMesh", vertices, uvs, triangles), material);
        }

        public static GameObject GeneratePolygonMesh(
            IList<Vector3> points,
            Material material,
            float standardWidth = 10f,
            float standardHeight = 10f)
        {
            if (points == null || points.Count < 3)
            {
                Debug.LogWarning("At least three points are required to generate a polygon mesh.");
                return null;
            }

            Vector3[] vertices = points.ToArray();
            Bounds bounds = CalculatePlanarBounds(vertices);
            float width = Mathf.Max(bounds.size.x, Mathf.Epsilon);
            float height = Mathf.Max(bounds.size.y, Mathf.Epsilon);
            float uScale = SafeDivide(bounds.size.x, standardWidth);
            float vScale = SafeDivide(bounds.size.y, standardHeight);
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                float u = (vertices[i].x - bounds.min.x) / width * uScale;
                float v = (vertices[i].y - bounds.min.y) / height * vScale;
                uvs[i] = new Vector2(u, v);
            }

            int[] triangles = new Triangulator(vertices).Triangulate();
            return CreateMeshObject("PolygonMesh", CreateRuntimeMesh("PolygonMesh", vertices, uvs, triangles), material);
        }

        public static GameObject GeneratePlane(
            Vector3 center,
            Vector2 extent,
            Material material,
            float standardWidth = 10f,
            float standardHeight = 10f)
        {
            float halfExtentX = extent.x * 0.5f;
            float halfExtentY = extent.y * 0.5f;

            Vector3[] vertices =
            {
                new(center.x - halfExtentX, center.y, center.z - halfExtentY),
                new(center.x + halfExtentX, center.y, center.z - halfExtentY),
                new(center.x + halfExtentX, center.y, center.z + halfExtentY),
                new(center.x - halfExtentX, center.y, center.z + halfExtentY),
            };

            Vector2[] uvs = CalculateQuadUvs(extent, standardWidth, standardHeight);
            int[] triangles = { 0, 3, 2, 2, 1, 0 };
            return CreateMeshObject("PlaneMesh", CreateRuntimeMesh("PlaneMesh", vertices, uvs, triangles), material);
        }

        private static void SetRendererEnabled(Renderer renderer, bool enabled)
        {
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        private static GameObject CreateMeshObject(string objectName, Mesh mesh, Material material)
        {
            GameObject meshObject = new(objectName);
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
            RuntimeMeshLifetime meshLifetime = meshObject.AddComponent<RuntimeMeshLifetime>();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
            meshLifetime.Initialize(mesh);

            return meshObject;
        }

        private static Mesh CreateRuntimeMesh(string meshName, Vector3[] vertices, Vector2[] uvs, int[] triangles)
        {
            Mesh mesh = new()
            {
                name = meshName,
                indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                vertices = vertices,
                uv = uvs,
                triangles = triangles,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2[] CalculateQuadUvs(Vector2 extent, float standardWidth, float standardHeight)
        {
            float u = SafeDivide(extent.x, standardWidth);
            float v = SafeDivide(extent.y, standardHeight);
            return new[]
            {
                new Vector2(0, 0),
                new Vector2(u, 0),
                new Vector2(u, v),
                new Vector2(0, v),
            };
        }

        private static Bounds CalculatePlanarBounds(IReadOnlyList<Vector3> points)
        {
            Bounds bounds = new(points[0], Vector3.zero);
            for (int i = 1; i < points.Count; i++)
            {
                bounds.Encapsulate(points[i]);
            }

            return bounds;
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? 0f : value / divisor;
        }

        private static void DestroyUnityObject(Object unityObject)
        {
            if (unityObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(unityObject);
            }
            else
            {
                Object.DestroyImmediate(unityObject);
            }
        }
    }
}
