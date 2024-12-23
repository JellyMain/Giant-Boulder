using UnityEngine;


namespace TerrainGenerator
{
    public class MeshData
    {
        private readonly Vector3[] vertices;
        private readonly int[] triangles;
        private readonly Color[] colors;

        private int vertexCount = 0;


        public MeshData(int verticesPerLine)
        {
            vertices = new Vector3[verticesPerLine * verticesPerLine * 6];
            triangles = new int[verticesPerLine * verticesPerLine * 6];
            colors = new Color[verticesPerLine * verticesPerLine * 6];
        }


        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
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

            vertexCount += 3;
        }


        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;

            return mesh;
        }
    }
}
