using UnityEngine;


namespace TerrainGenerator
{
    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public Color[] colors;
        public Vector3[] normals;
        private int triangleIndex;


        public MeshData(int meshSize)
        {
            vertices = new Vector3[meshSize * meshSize];
            uvs = new Vector2[meshSize * meshSize];
            triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
            colors = new Color[meshSize * meshSize];
            normals = new Vector3[meshSize * meshSize];
        }


        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.normals = normals;

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
