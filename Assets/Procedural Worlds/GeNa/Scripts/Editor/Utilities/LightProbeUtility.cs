using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class LightProbeUtility
    {
        static LightProbeUtility()
        {
            GeNaEvents.GetOrCreateNearestProbeGroup = GetOrCreateNearestProbeGroup;
            GeNaEvents.ProbeGameObjectLPG = ProbeGameObject;
        }
        protected static bool canProbe;
        #region Methods
        /// <summary>
        /// Return probe group with the nearest probe to the given m_position, or create a new one
        /// </summary>
        /// <param name="geNaSpawner"></param>
        /// <param name="position">Position to check for in world coordinates</param>
        /// <param name="canAddNewProbes">Returns whether or not can add new probes at this location</param>
        /// <returns>Nearest probe group or null</returns>
        public static LightProbeGroup GetOrCreateNearestProbeGroup(GeNaSpawnerData geNaSpawner, Vector3 position, bool canAddNewProbes)
        {
            SpawnerSettings settings = geNaSpawner.Settings;
            canAddNewProbes = false;
            ProbeManager probeManager = GeNaGlobalReferences.GeNaManagerInstance.ProbeManager;
            List<LightProbeGroup> probeGroups = probeManager.GetProbeGroups(position, settings.MinProbeDistance);
            if (probeGroups != null)
            {
                if (probeGroups.Count != 0)
                {
                    canAddNewProbes = false;
                    return probeGroups.First();
                }
                canAddNewProbes = true;
                probeGroups = probeManager.GetProbeGroups(position, settings.MinProbeGroupDistance);
                if (probeGroups.Count != 0)
                    return probeGroups.First();
            }
            // Create new probe group and return it
            GameObject probeGo = new GameObject(string.Format("Light Probe Group {0:0}x {1:0}z", position.x, position.z));
            probeGo.transform.position = position;
            if (geNaSpawner.ProbeParent != null)
                probeGo.transform.parent = geNaSpawner.ProbeParent.transform;
            LightProbeGroup lpg = probeGo.AddComponent<LightProbeGroup>();
            //TODO : Manny : This needs to be in editor code as it is an editor only call (lpg.probePositions)
            lpg.probePositions = Array.Empty<Vector3>();
            GameObjectEntity entity = GeNaFactory.CreateUndoEntity<GameObjectEntity>();
            entity.m_gameObject = probeGo;
            GeNaUndoRedo.RecordUndo(entity);
            // geNaSpawner.UndoRecord.Record(entity);
            canProbe = canAddNewProbes;
            return lpg;
        }
        public static bool ProbeGameObject(GeNaSpawnerData geNaSpawner, Resource resource, GameObject go)
        {
            LightProbeGroup lpg = GetOrCreateNearestProbeGroup(geNaSpawner, go.transform.position, canProbe);
            if (lpg == null)
                return false;
            if (canProbe)
            {
                Vector3 newSize = Vector3.Scale(resource.BaseSize, go.transform.localScale);
                List<Vector3> probePositions = new List<Vector3>(lpg.probePositions);
                Vector3 transformPosition = go.transform.position;
                Vector3 position = transformPosition - lpg.transform.position; //Translate to local space relative to lpg
                position += new Vector3(0f, 0.5f, 0f);
                probePositions.Add(position);
                position += new Vector3(0f, newSize.y, 0f);
                probePositions.Add(position);
                position += new Vector3(0f, 5f, 0f);
                probePositions.Add(position);
                //TODO : Manny : This needs to be in editor code as it is an editor only call (lpg.probePositions = probePositions.ToArray();)
                lpg.probePositions = probePositions.ToArray();
                GeNaManager geNaManager = GeNaGlobalReferences.GeNaManagerInstance;
                if (geNaManager == null)
                    return false;
                ProbeManager probeManager = geNaManager.GetProbeManager();
                probeManager.AddProbe(transformPosition, lpg);
            }
            return true;
        }
        #endregion
    }
}