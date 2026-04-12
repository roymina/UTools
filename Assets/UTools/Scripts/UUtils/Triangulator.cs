using System.Collections.Generic;
using UnityEngine;

namespace UTools
{
    internal sealed class Triangulator
    {
        private readonly List<Vector3> points;

        public Triangulator(IEnumerable<Vector3> points)
        {
            this.points = new List<Vector3>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new();
            int pointCount = points.Count;
            if (pointCount < 3)
            {
                return indices.ToArray();
            }

            int[] vertexIndices = new int[pointCount];
            if (Area() > 0f)
            {
                for (int i = 0; i < pointCount; i++)
                {
                    vertexIndices[i] = i;
                }
            }
            else
            {
                for (int i = 0; i < pointCount; i++)
                {
                    vertexIndices[i] = pointCount - 1 - i;
                }
            }

            int remainingVertexCount = pointCount;
            int guard = 2 * remainingVertexCount;
            for (int v = remainingVertexCount - 1; remainingVertexCount > 2;)
            {
                if (guard-- <= 0)
                {
                    return indices.ToArray();
                }

                int u = v;
                if (remainingVertexCount <= u)
                {
                    u = 0;
                }

                v = u + 1;
                if (remainingVertexCount <= v)
                {
                    v = 0;
                }

                int w = v + 1;
                if (remainingVertexCount <= w)
                {
                    w = 0;
                }

                if (!Snip(u, v, w, remainingVertexCount, vertexIndices))
                {
                    continue;
                }

                indices.Add(vertexIndices[u]);
                indices.Add(vertexIndices[v]);
                indices.Add(vertexIndices[w]);

                for (int source = v + 1, target = v; source < remainingVertexCount; source++, target++)
                {
                    vertexIndices[target] = vertexIndices[source];
                }

                remainingVertexCount--;
                guard = 2 * remainingVertexCount;
            }

            return indices.ToArray();
        }

        private float Area()
        {
            int count = points.Count;
            float area = 0f;
            for (int previous = count - 1, current = 0; current < count; previous = current++)
            {
                Vector2 previousPoint = points[previous];
                Vector2 currentPoint = points[current];
                area += previousPoint.x * currentPoint.y - currentPoint.x * previousPoint.y;
            }

            return area * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] vertexIndices)
        {
            Vector2 a = points[vertexIndices[u]];
            Vector2 b = points[vertexIndices[v]];
            Vector2 c = points[vertexIndices[w]];
            if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
            {
                return false;
            }

            for (int p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                {
                    continue;
                }

                Vector2 point = points[vertexIndices[p]];
                if (InsideTriangle(a, b, c, point))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
        {
            float ax = c.x - b.x;
            float ay = c.y - b.y;
            float bx = a.x - c.x;
            float by = a.y - c.y;
            float cx = b.x - a.x;
            float cy = b.y - a.y;
            float apx = point.x - a.x;
            float apy = point.y - a.y;
            float bpx = point.x - b.x;
            float bpy = point.y - b.y;
            float cpx = point.x - c.x;
            float cpy = point.y - c.y;

            float aCrossBp = ax * bpy - ay * bpx;
            float bCrossCp = bx * cpy - by * cpx;
            float cCrossAp = cx * apy - cy * apx;

            return aCrossBp >= 0f && bCrossCp >= 0f && cCrossAp >= 0f;
        }
    }
}
