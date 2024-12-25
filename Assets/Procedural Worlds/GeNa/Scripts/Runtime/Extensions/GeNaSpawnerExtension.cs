using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for running GeNa Spawners along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Spawner", menuName = "Procedural Worlds/GeNa/Extensions/Spawner", order = 0)]
    public class GeNaSpawnerExtension : GeNaSplineExtension
    {
        #region Variables

        [SerializeField] protected SpawnerEntry m_spawnerEntry = null;
        [SerializeField] protected bool m_autoIterate = false;
        [SerializeField] protected bool m_alignToSpline = false;
        [SerializeField] protected bool m_alignChildrenToSpline = false;
        [SerializeField] protected bool m_conformToSlope = false;
        [SerializeField] protected bool m_conformChildrenToSlope = false;
        [SerializeField] protected bool m_snapToGround = false;
        [SerializeField] protected bool m_snapChildrenToGround = false;
        [SerializeField] protected UndoRecord m_undoRecord = null;
        [SerializeField] protected Vector3 m_intersectionBoundsSize = new Vector3(10f, 10f, 10f);
        [SerializeField] protected bool m_checkIntersectionBounds = false;
        [NonSerialized] private bool m_isProcessing = false;

        #endregion

        #region Properties

        public SpawnerEntry SpawnerEntry => m_spawnerEntry;

        public GeNaSpawner Spawner
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.Spawner : null;
            set
            {
                if (m_spawnerEntry != null)
                {
                    if (m_spawnerEntry.Spawner != value)
                        m_spawnerEntry = new SpawnerEntry(value);
                }
                else
                {
                    m_spawnerEntry = new SpawnerEntry(value);
                }
            }
        }

        public GeNaSpawnerData SpawnerData => m_spawnerEntry?.SpawnerData;

        public Transform Target
        {
            get => m_spawnerEntry?.Target;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.Target = value;
            }
        }

        public Vector3 OffsetPosition
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.OffsetPosition : default;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.OffsetPosition = value;
            }
        }

        public Vector3 OffsetRotation
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.OffsetRotation : default;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.OffsetRotation = value;
            }
        }

        public float FlowRate
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.FlowRate : default;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.FlowRate = value;
            }
        }

        public float SpawnRange
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.SpawnRange : default;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.SpawnRange = value;
            }
        }

        public float ThrowDistance
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.ThrowDistance : default;
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.ThrowDistance = value;
            }
        }

        public List<SpawnCall> SpawnCalls
        {
            get => m_spawnerEntry != null ? m_spawnerEntry.SpawnCalls : new List<SpawnCall>();
            set
            {
                if (m_spawnerEntry != null)
                    m_spawnerEntry.SpawnCalls = value;
            }
        }

        public bool AutoIterate
        {
            get => m_autoIterate;
            set => m_autoIterate = value;
        }

        public bool AlignToSpline
        {
            get => m_alignToSpline;
            set => m_alignToSpline = value;
        }

        public bool AlignChildrenToSpline
        {
            get => m_alignChildrenToSpline;
            set => m_alignChildrenToSpline = value;
        }

        public bool ConformToSlope
        {
            get => m_conformToSlope;
            set => m_conformToSlope = value;
        }

        public bool ConformChildrenToSlope
        {
            get => m_conformChildrenToSlope;
            set => m_conformChildrenToSlope = value;
        }

        public bool SnapToGround
        {
            get => m_snapToGround;
            set => m_snapToGround = value;
        }

        public bool SnapChildrenToGround
        {
            get => m_snapChildrenToGround;
            set => m_snapChildrenToGround = value;
        }

        public bool IsProcessing
        {
            get => m_isProcessing;
            set => m_isProcessing = value;
        }

        public Vector3 IntersectionBoundsSize
        {
            get => m_intersectionBoundsSize;
            set => m_intersectionBoundsSize = value;
        }

        public bool CheckIntersectionBounds
        {
            get => m_checkIntersectionBounds;
            set => m_checkIntersectionBounds = value;
        }

        #endregion

        #region Methods

        protected override GameObject OnBake(GeNaSpline spline)
        {
            SpawnCalls.Clear();
            Execute();
            return null;
        }

        public void Load()
        {
            if (m_spawnerEntry == null)
                return;
            var spawner = m_spawnerEntry.Spawner;
            if (spawner == null)
                return;
            spawner.Load();
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            if (Spawner == null)
                return;
            name = Spawner.name;
            Spawner.Load();
        }

        protected override void OnAttach(GeNaSpline spline)
        {
            if (SpawnerData == null)
                return;
            m_spawnerEntry.SpawnCalls = GenerateSpawnCalls();
            m_spawnerEntry.FlowRate = SpawnerData.SpawnRange;
            UpdateSpawnCallsGround(m_spawnerEntry.SpawnCalls);
        }

        public override void PreExecute()
        {
            if (SpawnerData == null)
                return;
            UpdateSpawnCallsGround(m_spawnerEntry.SpawnCalls);
            // Generate the spawn calls
            m_spawnerEntry.SpawnCalls = GenerateSpawnCalls();
            name = Spawner.name;
            if (Spline != null)
                GeNaSpawnerInternal.SetupTempObject(Spline.transform);
        }

        public override void Execute()
        {
            if (SpawnerData == null)
                return;
            // Loop over each Spawn Call
            foreach (SpawnCall spawnCall in SpawnCalls)
            {
                // Loop over each Entity
                foreach (ResourceEntity entity in spawnCall.SpawnedEntities)
                {
                    GameObject gameObject = entity.GameObject;
                    if (gameObject != null)
                    {
                        if (gameObject.hideFlags == HideFlags.HideAndDontSave)
                            continue;
                        // Deactivate that entity
                        gameObject.SetActive(false);
                    }
                }
            }

            UpdateEntities();
        }

        protected override void OnDelete()
        {
            if (m_undoRecord != null)
                m_undoRecord.Undo();
            GeNaEvents.onSpawnFinished -= OnPostSpawn;
        }

        public void UpdateSpawnCallsGround(List<SpawnCall> spawnCalls)
        {
            if (m_spawnerEntry == null)
                return;
            GeNaSpawner spawner = m_spawnerEntry.Spawner;
            if (spawner == null)
                return;
            if (spawnCalls.Count == 0)
                return;
            ActivateAllEntities(false);
            Transform ground = GetTarget(out _);
            spawner.Load();
            ActivateAllEntities(true);
            foreach (SpawnCall spawnCall in spawnCalls)
            {
                spawnCall.Ground = ground;
                spawnCall.Target = ground;
                spawnCall.SpawnRange = SpawnRange;
                spawnCall.ThrowDistance = ThrowDistance;
                GeNaSpawnerInternal.GenerateAabbTest(SpawnerData, out AabbTest aabbTest, spawnCall.Location);
                bool locationIsValid = GeNaSpawnerInternal.CheckLocationForSpawn(SpawnerData, aabbTest, null, false);
                spawnCall.CanSpawn = locationIsValid && spawnCall.Target == ground;
            }
        }

        public void Spawn()
        {
            if (m_isProcessing)
                return;
            if (!Spline.HasNodes)
            {
                GeNaDebug.LogWarning($"The Spline '{Spline.name}' does not contain any nodes!");
                return;
            }

            if (SpawnerData == null)
                return;
            List<SpawnCall> spawnCalls = GenerateSpawnCalls();
            if (spawnCalls.Count == 0)
                return;
            m_spawnerEntry.RecordUndo = true;
            m_spawnerEntry.SpawnCalls = spawnCalls;
            m_spawnerEntry.RootSpawnCall = spawnCalls.First();
            m_spawnerEntry.Spawner.Load();
            GeNaUtility.ScheduleSpawn(m_spawnerEntry);
            m_isProcessing = true;
            GeNaEvents.onSpawnFinished -= OnPostSpawn;
            GeNaEvents.onSpawnFinished += OnPostSpawn;
        }

        public void OnPostSpawn()
        {
            m_undoRecord = SpawnerData.LastUndoRecord;
            m_isProcessing = false;
            Spline.UpdateSpline();
            GeNaEvents.onSpawnFinished -= OnPostSpawn;
        }

        public void Iterate()
        {
            if (SpawnerData == null)
                return;
            if (m_isProcessing)
                return;
            if (m_undoRecord != null)
                m_undoRecord.Undo();
            // Clears the AabbManager
            var geNaManager = GeNaManager.GetInstance();
            geNaManager.AabbManager.Clear();
            Spawn();
            GeNaEvents.onSpawnFinished -= OnPostSpawn;
            GeNaEvents.onSpawnFinished += OnPostSpawn;
        }

        public void UpdateEntities()
        {
            if (SpawnerData == null)
                return;
            // Loop over each spawned entity Dictionary
            foreach (SpawnCall spawnCall in SpawnCalls)
            {
                if (!spawnCall.IsActive)
                    continue;
                spawnCall.AlignChildrenToRotation = m_alignChildrenToSpline;
                spawnCall.ConformChildrenToSlope = m_conformChildrenToSlope;
                spawnCall.SnapChildrenToGround = m_snapChildrenToGround;
                spawnCall.UpdateEntities();
            }
        }

        public Transform GetTarget(Vector3 position, out RaycastHit hitInfo)
        {
            Transform target = default;
            // Sample ground at location
            Vector3 origin = position + Vector3.up * 2f;
            Vector3 direction = Vector3.down;
            Ray ray = new Ray(origin, direction);
            if (GeNaSpawnerInternal.Sample(SpawnerData, ray, out hitInfo))
                target = hitInfo.transform;
            return target;
        }

        public Transform GetTarget(out RaycastHit hitInfo)
        {
            hitInfo = default;
            if (SpawnerData == null)
                return null;
            if (Spline == null)
                return null;
            List<GeNaNode> nodes = Spline.Nodes;
            if (nodes.Count == 0)
                return null;
            GeNaNode firstNode = nodes.First();
            Vector3 nodePosition = firstNode.Position;
            return GetTarget(nodePosition, out hitInfo);
        }

        public void ActivateAllEntities(bool isActive)
        {
            foreach (var spawnCall in SpawnCalls)
            {
                if (isActive)
                    spawnCall.EnableEntities();
                else
                    spawnCall.DisableEntities();
            }
        }

        public List<SpawnCall> GenerateSpawnCalls()
        {
            List<SpawnCall> result = new List<SpawnCall>();
            if (SpawnerData == null)
                return result;
            if (FlowRate <= 0.0f)
                return result;
            List<GeNaNode> nodes = Spline.Nodes;
            float length = Spline.Length;
            int spawnCallCount = Mathf.CeilToInt(length / FlowRate);
            result.Capacity = spawnCallCount;
            if (nodes.Count == 0)
                return result;
            float distance = 0f;
            int index = 0;
            List<Bounds> intersectionBounds = GeNaSpline.BuildIntersectionBounds(Spline, IntersectionBoundsSize);
            while (distance < length)
            {
                // Collect Sample at Distance
                GeNaSample geNaSample = Spline.GetSampleAtDistance(distance);
                if (geNaSample != null)
                {
                    SpawnCall spawnCall = default;
                    if (index < SpawnCalls.Count)
                    {
                        spawnCall = SpawnCalls[index++];
                        // if (spawnCall.IsEmpty || !spawnCall.Generated)
                        //     spawnCall = null;
                    }

                    if (spawnCall == null)
                    {
                        // Generate Spawn Call for entry
                        spawnCall = new SpawnCall
                        {
                            Normal = Vector3.up
                        };
                    }

                    // Offset Location & Rotation
                    Vector3 location = geNaSample.Location +
                                       (geNaSample.Scale.x * OffsetPosition.x * geNaSample.Right) +
                                       (geNaSample.Scale.y * OffsetPosition.y * geNaSample.Up);
                    //Check if it's in intersection bounds
                    if (CheckIntersectionBounds)
                    {
                        if (GeNaSpline.IsInIntersectionBounds(intersectionBounds, location))
                        {
                            spawnCall.IsActive = false;
                        }
                        else
                        {
                            spawnCall.IsActive = true;
                        }
                    }
                    else
                    {
                        spawnCall.IsActive = true;
                    }

                    if (m_isProcessing)
                    {
                        spawnCall.Rotation = Vector3.zero;
                        spawnCall.SpawnedLocation = location;
                        spawnCall.Location = location;
                    }
                    else
                    {
                        Vector3 rotation = new Vector3(0f, OffsetRotation.y, 0f);
                        Quaternion forwardRotation = Quaternion.LookRotation(geNaSample.Forward, Vector3.up);
                        Vector3 euler = forwardRotation.eulerAngles;
                        // Align to Spline Mode?
                        if (m_alignToSpline)
                        {
                            euler.x = euler.z = 0f;
                            euler.y += OffsetRotation.y;
                            rotation = euler;
                        }

                        spawnCall.SpawnedLocation = location;
                        spawnCall.Location = location;
                        spawnCall.Rotation = rotation;
                        if (m_conformToSlope)
                        {
                            spawnCall.ConformToSlope();
                        }

                        if (m_snapToGround)
                        {
                            spawnCall.SnapToGround();
                            spawnCall.Location += Vector3.up * OffsetPosition.y;
                        }
                    }

                    // GeNaSpawnerInternal.SetSpawnOrigin(SpawnerData, spawnCall);
                    result.Add(spawnCall);
                }

                // Offset Distance for next iteration
                distance += FlowRate;
            }

            for (int i = index; i < SpawnCalls.Count; i++)
            {
                SpawnCall spawnCall = SpawnCalls[i];
                spawnCall.Dispose();
            }

            if (SpawnerData != null)
                GeNaSpawnerInternal.GenerateRandomData(SpawnerData, result);
            return result;
        }

        protected override void OnDeselect()
        {
        }

        #endregion
    }
}