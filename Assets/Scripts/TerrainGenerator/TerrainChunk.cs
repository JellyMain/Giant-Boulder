using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using StaticData.Data;
using Structures;
using TerrainGenerator.Data;
using TerrainGenerator.Enums;
using UnityEditor;
using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private readonly MeshRenderer meshRenderer;
        private readonly MeshFilter meshFilter;
        private readonly MeshCollider meshCollider;
        public GameObject chunkGameObject;
        public ChunkLandscapeType ChunkLandscapeType { get; private set; }
        private Bounds bounds;
        public MeshData meshData;
        public Vector3 position;
        public bool structuresInstantiated;
        public SpawnerConfig spawnerConfig;
        public readonly Dictionary<RaycastHit, StructureRoot> structures = new Dictionary<RaycastHit, StructureRoot>();


        public TerrainChunk(Material material, Vector2 chunkCoord, MeshData meshData,
            ChunkLandscapeType chunkLandscapeType, Transform parent, int chunkSize)
        {
            this.meshData = meshData;
            position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
            bounds = new Bounds(position, new Vector3(1 * chunkSize, 0, 1 * chunkSize));
            ChunkLandscapeType = chunkLandscapeType;

            chunkGameObject = new GameObject($"Terrain Chunk {ChunkLandscapeType.ToString()}");
            chunkGameObject.layer = LayerMask.NameToLayer("Ground");

            chunkGameObject.transform.SetParent(parent);

            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshFilter.mesh = meshData.CreateMesh();

            meshCollider = chunkGameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshRenderer.material = material;
        }


        public void SpawnStructures()
        {
            foreach (KeyValuePair<RaycastHit, StructureRoot> keyValuePair in structures)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, keyValuePair.Key.normal);
                StructureRoot spawnedObject = Object.Instantiate(keyValuePair.Value, keyValuePair.Key.point, rotation,
                    chunkGameObject.transform);
                ApplyStructureSettings(spawnedObject.structureChildSettings);
            }

            structuresInstantiated = true;
        }


        private void ApplyStructureSettings(List<StructureSpawnSettings> structureSpawnSettings)
        {
            foreach (StructureSpawnSettings childObject in structureSpawnSettings)
            {
                if (childObject.snapToGround)
                {
                    SnapToGround(childObject.transform);
                }

                int randomNumber = Random.Range(0, 100);

                if (randomNumber > childObject.SpawnChance)
                {
                    Object.Destroy(childObject.gameObject);
                    continue;
                }

                if (childObject.RotationModifier != Vector2.zero)
                {
                    float objectYRotation =
                        Random.Range(childObject.RotationModifier.x, childObject.RotationModifier.y);

                    childObject.transform.localRotation = Quaternion.Euler(childObject.transform.rotation.x,
                        objectYRotation,
                        childObject.transform.rotation.z);
                }
            }
        }


        private void SnapToGround(Transform objectToSnap)
        {
            Vector3 rayStart = objectToSnap.position + Vector3.up * spawnerConfig.raycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, spawnerConfig.groundLayer))
            {
                float currentYRotation = objectToSnap.eulerAngles.y;

                Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                objectToSnap.rotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                objectToSnap.position = hit.point;
            }
        }


        public void UpdateChunk(Transform player, float viewDistance)
        {
            float distanceToPlayer = Mathf.Sqrt(bounds.SqrDistance(new Vector2(player.position.x, player.position.z)));
            bool isVisible = distanceToPlayer <= viewDistance;
            SetVisible(isVisible);

            if (!structuresInstantiated)
            {
                SpawnStructures();
            }
        }


        public bool IsVisible()
        {
            return chunkGameObject.activeSelf;
        }


        public void SetVisible(bool isVisible)
        {
            chunkGameObject.SetActive(isVisible);
        }
    }
}
