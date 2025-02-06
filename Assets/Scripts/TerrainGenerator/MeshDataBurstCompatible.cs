using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace TerrainGenerator
{
    public struct MeshDataBurstCompatible
    {
        public NativeArray<float3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Color> colors;
        public NativeArray<float2> uvs;
        public int verticesPerLine;
        private int vertexCount;


        public MeshDataBurstCompatible(int verticesPerLine)
        {
            this.verticesPerLine = verticesPerLine;
            vertexCount = 0;
            vertices = new NativeArray<float3>(verticesPerLine * verticesPerLine * 6, Allocator.TempJob);
            triangles = new NativeArray<int>(verticesPerLine * verticesPerLine * 6, Allocator.TempJob);
            colors = new NativeArray<Color>(verticesPerLine * verticesPerLine * 6, Allocator.TempJob);
            uvs = new NativeArray<float2>(verticesPerLine * verticesPerLine * 6, Allocator.TempJob);
        }


        public void AddTriangle(float3 v1, float3 v2, float3 v3, Color color, float2 uv1, float2 uv2, float2 uv3)
        {
            vertices[vertexCount] = v1;
            vertices[vertexCount + 1] = v2;
            vertices[vertexCount + 2] = v3;

            triangles[vertexCount] = vertexCount;
            triangles[vertexCount + 1] = vertexCount + 1;
            triangles[vertexCount + 2] = vertexCount + 2;

            colors[vertexCount] = color;
            colors[vertexCount + 1] = color;
            colors[vertexCount + 2] = color;

            uvs[vertexCount] = uv1;
            uvs[vertexCount + 1] = uv2;
            uvs[vertexCount + 2] = uv3;

            vertexCount += 3;
        }


        public void Dispose()
        {
            vertices.Dispose();
            triangles.Dispose();
            uvs.Dispose();
            colors.Dispose();
        }
    }
}
