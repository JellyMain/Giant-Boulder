using UnityEngine;


namespace TerrainGenerator
{
    public class MeshData
    {
        public Vector3[] vertices;
        private int[] triangles;
        public Vector2[] uvs;
        private int triangleIndex;


        public MeshData(int meshSize)
        {
            vertices = new Vector3[meshSize * meshSize];
            uvs = new Vector2[meshSize * meshSize];
            triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
        }


        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();

            return mesh;
        }


        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            
            triangleIndex += 3;
        }
        
    }
}
