using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;


namespace TerrainGenerator
{ 

    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField, OnValueChanged("SetMesh")] private int size = 200;
        [SerializeField, OnValueChanged("SetMesh")] private float noiseScale = 10;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;

        private NoiseGenerator noiseGenerator = new NoiseGenerator();
        private TextureGenerator textureGenerator = new TextureGenerator();
        private MeshGenerator meshGenerator = new MeshGenerator();

        
        
        [Button]
        private void SetHeightMapTexture()
        {
            float[,] heightMap = noiseGenerator.GenerateHeightMap(size, noiseScale);
            Texture2D texture = textureGenerator.CreateTextureFromHeightMap(heightMap);

            meshRenderer.sharedMaterial.mainTexture = texture;

            meshRenderer.transform.localScale = new Vector3(size, 1, size);
        }



        [Button]
        private void SetMesh()
        {
            float[,] heightMap = noiseGenerator.GenerateHeightMap(size, noiseScale);

            MeshData meshData = meshGenerator.CreateMeshData(heightMap);

            meshFilter.sharedMesh = meshData.CreateMesh();
            Texture2D texture = textureGenerator.CreateTextureFromHeightMap(heightMap);
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
        
    }
}
