using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;


namespace TerrainGenerator
{
    public enum GenerationMode
    {
        RandomPoints = 0,
        PoissonDiskSampling = 1
    }

    public enum ColorMode
    {
        Random = 0,
        HeightGradient = 1
    }

    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private GenerationMode generationMode;
        [SerializeField] private ColorMode colorMode;

        [SerializeField, ShowIf("@colorMode == ColorMode.HeightGradient")]
        private Gradient heightGradient;

        [SerializeField, Range(10, 1000)] private int size = 1000;

        [SerializeField, Range(1, 10), ShowIf("@generationMode == GenerationMode.PoissonDiskSampling ")]
        private float minimumDistance = 5;

        [SerializeField, ShowIf("@generationMode == GenerationMode.PoissonDiskSampling ")]
        private int attemptsNumber = 10;

        [SerializeField, Range(10, 1000), ShowIf("@generationMode == GenerationMode.RandomPoints")]
        private int verticesCount = 500;

        [SerializeField, Range(1f, 3000f)] private float heightScale = 50f;
        [SerializeField, Range(50f, 300f)] private float scale = 34;
        [SerializeField, Range(0.001f, 1f)] private float heightSmoothing = 0.21f;
        [SerializeField, Range(1, 15)] private int layers = 1;
        [SerializeField, Range(0f, 1f)] private float detailStrength = 0.1f;
        [SerializeField, Range(1f, 10f)] private float detailSpacing = 1.5f;

        private Vector2 offset;
        private List<Vector3> allVertices;
        private List<float> heights;
        private List<(Vector3, Vector3, Vector3)> newTriangles;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private float maxNoiseHeight;
        private float minNoiseHeight;
        private readonly DelaunayTriangulation delaunayTriangulation = new DelaunayTriangulation();



        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
        }


        private void Start()
        {
            GenerateTerrain();
        }


        private void GenerateTerrain()
        {
            CreateTriangles();
            ShapeTerrain();
            CreateMesh();
        }


        private void CreateTriangles()
        {
            switch (generationMode)
            {
                case GenerationMode.RandomPoints:
                {
                    allVertices = new List<Vector3>(verticesCount);

                    for (int i = 0; i < verticesCount; i++)
                    {
                        float xPosition = Random.Range(-size / 2, size / 2);
                        float zPosition = Random.Range(-size / 2, size / 2);

                        Vector3 vertex = new Vector3(xPosition, 0, zPosition);

                        allVertices.Add(vertex);
                    }

                    break;
                }
                case GenerationMode.PoissonDiskSampling:
                    allVertices = PoissonDiskSampling.GetPoints(size, minimumDistance, attemptsNumber);
                    break;
            }

            newTriangles = delaunayTriangulation.Triangulate(size, allVertices);
        }
        
        

        private void ShapeTerrain()
        {
            heights = new List<float>();

            offset = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));

            foreach ((Vector3 v0, Vector3 v1, Vector3 v2) in newTriangles)
            {
                Vector3[] vertices = { v0, v1, v2 };

                foreach (var vertex in vertices)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int o = 0; o < layers; o++)
                    {
                        float xValue = vertex.x / scale * frequency;
                        float zValue = vertex.z / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(xValue + offset.x, zValue + offset.y) * 2 - 1;
                        perlinValue *= heightSmoothing;

                        noiseHeight += perlinValue * amplitude;

                        amplitude *= detailStrength;
                        frequency *= detailSpacing;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }


                    noiseHeight = noiseHeight < 0f ? noiseHeight * heightScale / 10f : noiseHeight * heightScale;

                    heights.Add(noiseHeight);
                }
            }
        }


        private void CreateMesh()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<Vector3> normals = new List<Vector3>();


            foreach ((Vector3 v0, Vector3 v1, Vector3 v2) triangle in newTriangles)
            {
                int v0Index = vertices.Count;
                int v1Index = v0Index + 1;
                int v2Index = v0Index + 2;

                Vector3 shapedV0 = new Vector3(triangle.v0.x, heights[v0Index], triangle.v0.z);
                Vector3 shapedV1 = new Vector3(triangle.v1.x, heights[v1Index], triangle.v1.z);
                Vector3 shapedV2 = new Vector3(triangle.v2.x, heights[v2Index], triangle.v2.z);

                triangles.Add(v0Index);
                triangles.Add(v1Index);
                triangles.Add(v2Index);

                vertices.Add(shapedV0);
                vertices.Add(shapedV1);
                vertices.Add(shapedV2);

                Vector3 normal = Vector3.Cross(triangle.v1 - triangle.v0, triangle.v2 - triangle.v0);

                Color triangleColor = ChooseColor((shapedV0, shapedV1, shapedV2));

                for (int x = 0; x < 3; x++)
                {
                    normals.Add(normal);
                    uvs.Add(Vector3.zero);
                    colors.Add(triangleColor);
                }
            }

            Mesh terrainMesh = new Mesh();

            terrainMesh.vertices = vertices.ToArray();
            terrainMesh.triangles = triangles.ToArray();
            terrainMesh.colors = colors.ToArray();
            terrainMesh.normals = normals.ToArray();
            terrainMesh.uv = uvs.ToArray();

            meshRenderer.material = terrainMaterial;

            meshFilter.mesh = terrainMesh;
        }



        private Color ChooseColor((Vector3 v0, Vector3 v1, Vector3 v2) triangle)
        {
            float currentHeight = (triangle.v0.y + triangle.v1.y + triangle.v2.y) / 3;

            switch (colorMode)
            {
                case ColorMode.HeightGradient:
                    currentHeight = currentHeight < 0 ? currentHeight / heightScale * 10 : currentHeight / heightScale;
                    float gradientValue = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentHeight);
                    return heightGradient.Evaluate(gradientValue);
            }

            return Color.magenta;
        }




        [Button]
        private void RegenerateMesh()
        {
            if (meshFilter.mesh != null)
            {
                meshFilter.sharedMesh.Clear();
                meshFilter.mesh.Clear();
                Destroy(meshFilter.mesh);
                Destroy(meshFilter.sharedMesh);
            }

            GenerateTerrain();
        }


        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                RegenerateMesh();
            }
        }
    }
}
