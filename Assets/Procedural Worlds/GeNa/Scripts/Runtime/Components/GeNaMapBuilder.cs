//Copyright(c)2021 Procedural Worlds Pty Limited 
using System;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    [ExecuteAlways]
    public class GeNaMapBuilder : MonoBehaviour
    {
        [Header("Area Parameters")]
        [Tooltip("Minimum radius of an area.")]
        public float m_minRadius = 80.0f;
        [Tooltip("The lowest altitude that will be included in an area.")]
        public float m_seaLevel = 25.0f;
        [Tooltip("The maximum altitude that will be included in an area.")]
        public float m_maxElevation = 600.0f;
        [Tooltip("The maximum difference in height that will be considered to include in an area.")]
        public float m_maxDeviation = 10.0f;
        [Tooltip("The minimum distance between towns.")]
        public float m_minDistance = 20.0f;
        [Space(5)]
        [Header("Visualization Parameters")]
        [Tooltip("Should the elevation at the cursor position be shown?")]
        public bool m_showElevationAtCursor = true;
        [Tooltip("Should the start position of where an area began it's search for be shown?")]
        public bool m_showMoveCircle = false;
        [Tooltip("The color that is to be used for showing elevation and other text information in the scene.")]
        public Color m_elevationTextColor = Color.blue;
        [Space(5)]
        [Header("Town Building Parameters")]
        [Tooltip("The search for where to build in areas is done in a grid pattern, and this value is the divisor or smallest grid block that a building can occupy. You can also think of it as the total width of a roadway or as the minimum lot size.")]
        public float m_lotSize = 18.0f;
        [Tooltip("The strength of the flattening stamp for each town.")]
        [Range(0.0f, 1.0f)]
        public float m_flattenStrength = 0.7f;
        [HideInInspector]
        public GameObject m_buildingsParent = null;
        [Tooltip("The list of GeNa spawners that can be randomly selected from to fit into grid blocks in a town.")]
        public List<GeNaSpawner> m_buildingSpawners;
        //[Tooltip("The list of Prefabs that can be randomly selected from to fit into grid blocks in a town.")]
        //public List<GameObject> m_prefabs;
        [Space(5)]
        [Header("Road Parameters")]
        [Range(2.0f, 20.0f)]
        [Tooltip("The maximum grade (height/distanceTraveled) that a road can 'breach'.")]
        public float m_maxGrade = 10.0f;
        [Tooltip("Step size for A* search for paths.")]
        [Range(2, 16)]
        public int m_cellSize = 8;
        [Tooltip("The amount that an increase or decrease in slope affects the cost of the path.")]
        [Range(0.1f, 2.0f)]
        public float m_slopeStrengthFactor = 0.9f;
        [Tooltip("Cost per meter to travel on a road. Note that it should be less than rough ground.")]
        [Range(0.5f, 1.0f)]
        public float m_splineTravelCostFactor = 0.8f;
        [Tooltip("The default heuristic used is more complex, but a simple heuristic (B) can be used.")]
        public bool m_useHeuristicB = false;
        [HideInInspector]
        public int m_roadIgnoreMask = 0;
        [Space(5)]
        [Header("Final Area Parameters")]
        [Tooltip("Should areas that overlap be included in the result?")]
        public bool m_keepOverlapping = false;
        [HideInInspector]
        [Tooltip("What percent of overlap, if overlap is to be included, be considered valid?")]
        public float m_allowedOverlap = 10.0f;
        [HideInInspector] public bool m_selectTerrainOnly = true;
        [HideInInspector] public AreaBuilder m_areaBuilder = null;
        [HideInInspector] [SerializeField] private MapBuilder m_mapBuilder = null;
        private Vector3 gizmosPathStart = Vector3.zero;
        private Vector3 gizmosPathEnd = Vector3.zero;
        private Vector3 gizmosAreaCenter = Vector3.zero;
        [NonSerialized] private GeNaSpline m_spline = null;
        [SerializeField] private List<MapBuilderEntry> m_entries = new List<MapBuilderEntry>();

        public List<MapBuilderEntry> Entries
        {
            get => m_entries;
            set => m_entries = value;
        }
        public void RemoveEntry(int index)
        {
            if (index < 0 || index >= m_entries.Count)
            {
                GeNaDebug.Log($"There is no entry at index '{index}'");
                return;
            }
            m_entries.RemoveAt(index); 
        }
        public MapBuilderEntry AddEntry(GeNaSpawner spawner)
        {
            MapBuilderEntry entry = new MapBuilderEntry();
            entry.Spawner = spawner;
            //if (spawner != null)
                //extension._Attach(this);
            m_entries.Add(entry);
            return entry;
        }

        private void OnValidate()
        {
            transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        }

        void Reset()
        {
            m_areaBuilder = Resources.Load<AreaSpawnBuilder>("Builders/Area Spawner Builder");
            m_seaLevel = GeNaEvents.GetSeaLevel(m_seaLevel);
        }
        void OnBeforeFindPath(Vector3 start, Vector3 end)
        {
            gizmosPathStart = start;
            gizmosPathEnd = end;
            GeNaEvents.RepaintAll();
        }
        void OnBeforeAreaBuild(Vector3 center, float radius)
        {
            gizmosAreaCenter = center;
            GeNaEvents.RepaintAll();
        }
        void OnAfterWorkDone()
        {
            gizmosPathStart = gizmosPathEnd = Vector3.zero;
            gizmosAreaCenter = Vector3.zero;
            GeNaEvents.RepaintAll();
        }
        public List<AreaFinder.AreaPoly> Areas => m_mapBuilder == null ? null : m_mapBuilder.Areas;
        public bool IsBusy => m_mapBuilder != null && m_mapBuilder.IsBusy;
        public int BuildCount => m_mapBuilder == null ? 0 : m_mapBuilder.AreasBuilt;
        public bool HasAreas => m_mapBuilder == null ? false : m_mapBuilder.HasAreas;
        public bool HasSpawners => m_entries.Count > 0;
        public bool CanBuildRoads => m_mapBuilder == null ? false : m_mapBuilder.CanBuildRoads;
        public void CancelBuild() => m_mapBuilder?.CancelBuild();
        public void FindAllAreas()
        {
            m_mapBuilder = new MapBuilder();
            m_mapBuilder.FindAreas(m_maxDeviation, m_minRadius, m_seaLevel, m_maxElevation, m_keepOverlapping, m_allowedOverlap);
        }
        public void ClearAreas() => m_mapBuilder = null;
        public void ApplyAreaRules() => m_mapBuilder?.ApplyAreaRules(m_keepOverlapping, m_allowedOverlap, 0.0f);
        public void BuildOnAreas()
        {
            if (m_mapBuilder == null)
                return;
            if (IsBusy || !HasSpawners)
                return;
            m_spline = Spline.CreateSpline("MapBuilder Roads");
            GeNaRoadExtension roadExtension = m_spline.AddExtension<GeNaRoadExtension>();
            roadExtension.Width = 7.8f;
            GeNaCarveExtension carve = m_spline.AddExtension<GeNaCarveExtension>();
            carve.Width = 8.0f;
            carve.Shoulder = 3.0f;
            GeNaTerrainExtension terrainTexture = m_spline.AddExtension<GeNaTerrainExtension>();
            if (terrainTexture != null)
            {
                terrainTexture.name = "Texture";
                terrainTexture.EffectType = EffectType.Texture;
                terrainTexture.Width = roadExtension.Width;
            }

            m_mapBuilder.OnBeforeBuildArea -= OnBeforeAreaBuild;
            m_mapBuilder.OnAfterWorkDone -= OnAfterWorkDone;
            m_mapBuilder.OnBeforeBuildArea += OnBeforeAreaBuild;
            m_mapBuilder.OnAfterWorkDone += OnAfterWorkDone;
            m_mapBuilder.ApplyAreaRules(m_keepOverlapping, m_allowedOverlap, m_minDistance);
            GeNaEvents.StartCoroutine(m_mapBuilder.BuildAllAreas(m_areaBuilder, m_spline, m_buildingsParent, m_flattenStrength, m_lotSize, m_entries, null), this);
        }
        public void ConnectAreasWithRoads()
        {
            if (m_mapBuilder == null || m_mapBuilder.IsBusy || !m_mapBuilder.CanBuildRoads)
                return;
            if (m_spline == null)
            {
                m_spline = Spline.CreateSpline("MapBuilder Roads");
                GeNaRoadExtension roadExtension = m_spline.AddExtension<GeNaRoadExtension>();
                GeNaTerrainExtension terrainTexture = m_spline.AddExtension<GeNaTerrainExtension>();
                if (terrainTexture != null)
                {
                    terrainTexture.name = "Texture";
                    terrainTexture.EffectType = EffectType.Texture;
                    terrainTexture.Width = roadExtension.Width;
                }
            }
            m_mapBuilder.SplineTravelFactor = m_splineTravelCostFactor;
            m_mapBuilder.UseHeuristicB = m_useHeuristicB;
            m_mapBuilder.OnBeforeFindPath -= OnBeforeFindPath;
            m_mapBuilder.OnAfterWorkDone -= OnAfterWorkDone;
            m_mapBuilder.OnBeforeFindPath += OnBeforeFindPath;
            m_mapBuilder.OnAfterWorkDone += OnAfterWorkDone;
            GeNaEvents.StartCoroutine(m_mapBuilder.ConnectAreasWithRoads(m_spline, m_maxGrade, m_cellSize, m_seaLevel, m_maxElevation + 150.0f, m_slopeStrengthFactor, m_roadIgnoreMask), this);
            GeNaEvents.RepaintAll();
        }
        private void OnDrawGizmosSelected()
        {
            if (m_mapBuilder == null || !m_mapBuilder.HasAreas)
                return;
            if (m_mapBuilder.TownConnections != null)
            {
                Gizmos.color = Color.blue;
                foreach (List<GeNaNode> nodeList in m_mapBuilder.TownConnections.Values)
                foreach (var node in nodeList)
                    Gizmos.DrawWireCube(node.Position, Vector3.one);
            }
            for (int n = 0; n < m_mapBuilder.Areas.Count; n++)
            {
                if (m_mapBuilder.Areas[n].Enabled == false)
                    continue;
                AreaFinder.AreaPoly areaPoly = m_mapBuilder.Areas[n];
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(areaPoly.Center, 0.2f);
                DrawDirections(areaPoly.Center, areaPoly.Axis);
                string str = $"{n}:({areaPoly.Id},g{areaPoly.IslandNumber})\nE={areaPoly.Center.y}\nR={areaPoly.Radius}";
                DrawString(str, areaPoly.Center, Color.yellow);
                if (m_showMoveCircle)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(areaPoly.OriginalCenter, areaPoly.Center);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(areaPoly.OriginalCenter, 2.0f);
                }

                //DrawDisc(areaPoly.m_avgCenter, areaPoly.m_avgRadius, Color.red);
                //DrawDisc(areaPoly.m_avgCenter, areaPoly.m_maxRadius, Color.blue);
                Gizmos.color = Color.grey;
                if (gizmosAreaCenter == areaPoly.Center)
                    Gizmos.color = Color.yellow;
                for (int i = 0; i < areaPoly.Points.Count; i++)
                {
                    int i2 = (i + 1) % areaPoly.Points.Count;
                    Gizmos.DrawLine(areaPoly.Points[i], areaPoly.Points[i2]);
                }

                // Draw a line to connected areas.
                if (areaPoly.Connected != null && areaPoly.Connected.Count > 0)
                {
                    Gizmos.color = Color.black;
// #if UNITY_EDITOR
//                     Handles.color = Color.black;
// #endif
                    for (int i = 0; i < areaPoly.Connected.Count; i++)
                    {
                        AreaFinder.AreaPoly connected = areaPoly.Connected[i];
                        // Raise each Connected line up a bit so that we can see if there are
                        // multiple connections between the same areas.
                        Vector3 raiseVec = Vector3.up * i * 2.0f;
                        if (areaPoly.Center == gizmosPathStart && connected.Center == gizmosPathEnd)
                            Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(areaPoly.Center + raiseVec, connected.Center + raiseVec);
// #if UNITY_EDITOR
//                         Vector3 pos = connected.m_center + raiseVec;
//                         Vector3 dir = (connected.m_center - areaPoly.m_center).normalized;
//                         float size = HandleUtility.GetHandleSize(pos) * 0.1f;
//                         if (dir.sqrMagnitude > 0.1f)
//                             Handles.ConeHandleCap(0, pos - dir * size, Quaternion.LookRotation(dir), size, EventType.Repaint);
//                         else
//                             Debug.LogWarning($"The distance between connected {i} area {areaPoly.m_radius} and {connected.m_radius} is zero");
//                         Handles.color = Color.magenta;
// #endif
                        Gizmos.color = Color.magenta;
                    }
                }
            }
        }
        void DrawDirections(Vector3 pos, Vector2 axis)
        {
// #if UNITY_EDITOR
//             float size = HandleUtility.GetHandleSize(pos);
//             size *= 0.075f;
//             Gizmos.color = Color.blue;
//             Gizmos.DrawRay(pos, Vector3.forward * 6.0f * size);
//             Handles.color = Color.blue;
//             Handles.ConeHandleCap(0, pos + Vector3.forward * 6.0f * size, Quaternion.LookRotation(Vector3.forward), size, EventType.Repaint);
//             Gizmos.color = Color.red;
//             Gizmos.DrawRay(pos, Vector3.right * 6.0f * size);
//             Handles.color = Color.red;
//             Handles.ConeHandleCap(0, pos + Vector3.right * 6.0f * size, Quaternion.LookRotation(Vector3.right), size, EventType.Repaint);
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawRay(pos, Vector3.up * 6.0f * size);
//             Handles.color = Color.yellow;
//             Handles.ConeHandleCap(0, pos + Vector3.up * 6.0f * size, Quaternion.LookRotation(Vector3.up), size, EventType.Repaint);
//
//             if (axis != Vector2.zero)
//             {
//                 Gizmos.color = Color.white;
//                 Vector3 axis3 = new Vector3(axis.x, 0.0f, axis.y);
//                 Gizmos.DrawRay(pos, axis3 * 10.0f * size);
//                 Handles.color = Color.white;
//                 Handles.ConeHandleCap(0, pos + axis3 * 10.0f * size, Quaternion.LookRotation(axis3), size, EventType.Repaint);
//             }
// #endif
        }
        void DrawDisc(Vector3 pos, float radius, Color color)
        {
// #if UNITY_EDITOR
//             Handles.color = color;
//             Handles.DrawWireDisc(pos, Vector3.up, radius);
// #endif
        }
        void DrawString(string str, Vector3 position, Color color)
        {
// #if UNITY_EDITOR
//             GUIStyle style = new GUIStyle();
//             style.normal.textColor = m_elevationTextColor;
//             Handles.Label(position, str, style);
// #endif
        }
    }
}