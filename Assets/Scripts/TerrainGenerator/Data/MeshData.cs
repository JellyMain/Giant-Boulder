using UnityEngine;


namespace TerrainGenerator.Data
{
    public class MeshData
    {
        public Vector3[] Vertices { get; set; }
        public int[] Triangles { get; set; }
        public Color[] Colors { get; set; }
        public Vector2[] Uvs { get; set; }
        public Vector3[] Normals { get; set; }
        public int verticesPerLine;
        private int vertexCount;


        public MeshData(int verticesPerLine)
        {
            this.verticesPerLine = verticesPerLine;
            Vertices = new Vector3[verticesPerLine * verticesPerLine * 6];
            Triangles = new int[verticesPerLine * verticesPerLine * 6];
            Colors = new Color[verticesPerLine * verticesPerLine * 6];
            Uvs = new Vector2[verticesPerLine * verticesPerLine * 6];
        }


        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            Vertices[vertexCount] = v1;
            Vertices[vertexCount + 1] = v2;
            Vertices[vertexCount + 2] = v3;

            Triangles[vertexCount] = vertexCount;
            Triangles[vertexCount + 1] = vertexCount + 1;
            Triangles[vertexCount + 2] = vertexCount + 2;

            Colors[vertexCount] = color;
            Colors[vertexCount + 1] = color;
            Colors[vertexCount + 2] = color;

            Uvs[vertexCount] = uv1;
            Uvs[vertexCount + 1] = uv2;
            Uvs[vertexCount + 2] = uv3;

            vertexCount += 3;
        }


        private Vector3[] CalculateNormals()
        {
            Normals = new Vector3[Vertices.Length];
            int triangleCount = Triangles.Length;

            for (int i = 0; i < triangleCount; i += 3)
            {
                int vertexIndexA = Triangles[i];
                int vertexIndexB = Triangles[i + 1];
                int vertexIndexC = Triangles[i + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

                Normals[vertexIndexA] += triangleNormal;
                Normals[vertexIndexB] += triangleNormal;
                Normals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < Normals.Length; i++)
            {
                Normals[i] = Normals[i].normalized;
            }

            return Normals;
        }



        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = Vertices[indexA];
            Vector3 pointB = Vertices[indexB];
            Vector3 pointC = Vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return Vector3.Cross(sideAB, sideAC).normalized;
        }


        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = Vertices;
            mesh.triangles = Triangles;
            mesh.colors = Colors;
            mesh.uv = Uvs;

            mesh.normals = CalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
