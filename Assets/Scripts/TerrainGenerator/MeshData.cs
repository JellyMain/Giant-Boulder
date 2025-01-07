using UnityEngine;


namespace TerrainGenerator
{
    public class MeshData
    {
        private readonly Vector3[] vertices;
        private readonly int[] triangles;
        private readonly Color[] colors;
        private readonly Vector2[] uvs;

        private int vertexCount = 0;


        public MeshData(int verticesPerLine)
        {
            vertices = new Vector3[verticesPerLine * verticesPerLine * 6];
            triangles = new int[verticesPerLine * verticesPerLine * 6];
            colors = new Color[verticesPerLine * verticesPerLine * 6];
            uvs = new Vector2[verticesPerLine * verticesPerLine * 6];
        }


        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color, Vector2 uv1, Vector2 uv2, Vector2 uv3)
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


        private Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length;

            for (int i = 0; i < triangleCount; i += 3) 
            {
                int vertexIndexA = triangles[i];
                int vertexIndexB = triangles[i + 1];
                int vertexIndexC = triangles[i + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i] = vertexNormals[i].normalized; 
            }

            return vertexNormals;
        }



        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return Vector3.Cross(sideAB, sideAC).normalized;
        }


        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.uv = uvs;
            
            mesh.normals = CalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
    }
}
