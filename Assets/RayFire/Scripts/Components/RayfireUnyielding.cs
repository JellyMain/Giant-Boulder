using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Unyielding")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-unyielding-component/")]
    public class RayfireUnyielding : MonoBehaviour
    {
        public enum UnySimType
        {
            Original  = 1,
            Inactive  = 2, 
            Kinematic = 3
        }
        
        // UI
        public bool       unyielding     = true;
        public bool       activatable    = false;
        public UnySimType simulationType = UnySimType.Original;
        public Vector3    centerPosition;
        public Vector3    size = new Vector3 (1f, 1f, 1f);
        
        // Hidden
        public RayfireRigid       rigidHost;
        public List<RayfireRigid> rigidList;
        public List<RFShard>      shardList;
        public bool               showGizmo = true;
        public bool               showCenter;
        public float              alSz = 1f;
        
        /// /////////////////////////////////////////////////////////
        /// Connected Cluster setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void ClusterSetup (RayfireRigid rigid)
        {
            if (rigid.simTp == SimType.Inactive || rigid.simTp == SimType.Kinematic)
            {
                RayfireUnyielding[] unyArray =  rigid.GetComponents<RayfireUnyielding>();
                for (int i = 0; i < unyArray.Length; i++)
                    if (unyArray[i].enabled == true)
                    {
                        unyArray[i].rigidHost = rigid;
                        ClusterOverlap (unyArray[i], rigid);
                    }
            }
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        public static void ClusterOverlap (RayfireUnyielding uny, RayfireRigid rigid)
        {
            // Get target mask and overlap colliders
            int               finalMask     = ClusterLayerMask(rigid);
            Collider[]        colliders     = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);
            
            // Check with connected cluster
            uny.shardList = new List<RFShard>();
            if (rigid.objTp == ObjectType.ConnectedCluster)
            {
                // Get simulation type for overlapped shards
                SimType shardSimType = rigid.simTp;
                if (uny.simulationType != UnySimType.Original)
                    shardSimType = (SimType)uny.simulationType;

                // Get all overlapped shards and set uny, act and sim states
                for (int i = 0; i < rigid.physics.cc.Count; i++)
                {
                    if (rigid.physics.cc[i] != null)
                        if (collidersHash.Contains (rigid.physics.cc[i]) == true)
                        {
                            SetShardUnyState (rigid.clsDemol.cluster.shards[i], uny.unyielding, uny.activatable);
                            rigid.clsDemol.cluster.shards[i].sm = shardSimType;
                            uny.shardList.Add (rigid.clsDemol.cluster.shards[i]);
                        }
                }
            }
        }
        
        // Get combined layer mask
        static int ClusterLayerMask(RayfireRigid rigid)
        {
            int mask = 0;
            if (rigid.objTp == ObjectType.ConnectedCluster)
                for (int i = 0; i < rigid.physics.cc.Count; i++)
                    if (rigid.physics.cc[i] != null)
                        mask = mask | 1 << rigid.clsDemol.cluster.shards[i].tm.gameObject.layer;
            return mask;
        }
        
        // Set unyielding state
        static void SetShardUnyState (RFShard shard, bool unyielding, bool activatable)
        {
            shard.uny = unyielding;
            shard.act = activatable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Mesh Root setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void MeshRootSetup (RayfireRigid mRoot)
        {
            // Get uny list
            List<RayfireUnyielding> unyList = GetUnyList (mRoot.transform);
            
            // Iterate every unyielding component
            for (int i = 0; i < unyList.Count; i++)
                SetMeshRootUnyRigidList (mRoot, unyList[i]);
            
            // Set rigid list uny and sim states 
            SetMeshRootUnyState (mRoot.transform, unyList);
        }
        
        // Get uny list
        static List<RayfireUnyielding> GetUnyList (Transform tm)
        {
            List<RayfireUnyielding> unyList = tm.GetComponents<RayfireUnyielding>().ToList();
            for (int i = unyList.Count - 1; i >= 0; i--)
                if (unyList[i].enabled == false)
                    unyList.RemoveAt (i);
            return unyList;
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        static void SetMeshRootUnyRigidList (RayfireRigid mRoot, RayfireUnyielding uny)
        {
            // Get target mask
            int               finalMask     = MeshRootLayerMask(mRoot);
            Collider[]        colliders     = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);
            
            // Check with connectivity rigids
            uny.rigidList = new List<RayfireRigid>();
            for (int i = 0; i < mRoot.fragments.Count; i++)
                if (mRoot.fragments[i].physics.mc != null)
                    if (collidersHash.Contains (mRoot.fragments[i].physics.mc) == true)
                        uny.rigidList.Add (mRoot.fragments[i]);
        }
        
        // Get combined layer mask
        static int MeshRootLayerMask(RayfireRigid mRoot)
        {
            int mask = 0;
            for (int i = 0; i < mRoot.fragments.Count; i++)
                if (mRoot.fragments[i].physics.mc != null)
                    mask = mask | 1 << mRoot.fragments[i].gameObject.layer;
            return mask;
        }
        
        // Set rigid list uny and sim states 
        public static void SetMeshRootUnyState (Transform tm, List<RayfireUnyielding> unyList)
        {
            // Get uny list
            if (unyList == null)
                unyList = GetUnyList (tm);
            
            // Iterate uny components list
            for (int c = 0; c < unyList.Count; c++)
            {
                // No rigids
                if (unyList[c].rigidList.Count == 0)
                    continue;

                // Set uny and act states for Rigids
                SetRigidUnyState (unyList[c]);
                
                // Set simulation type by
                SetRigidUnySim (unyList[c]);
            }
        }
        
        // Set unyielding state
        static void SetRigidUnyState (RayfireUnyielding uny)
        {
            // Common ops> Editor and Runtime
            for (int i = 0; i < uny.rigidList.Count; i++)
            {
                uny.rigidList[i].activation.uny = uny.unyielding;
                uny.rigidList[i].activation.atb = uny.activatable;
            }

            // Runtime ops.
            if (Application.isPlaying == true)
                for (int i = 0; i < uny.rigidList.Count; i++)
                {
                    // Stop velocity and offset activation coroutines for not activatable uny objects 
                    if (uny.unyielding == true && uny.activatable == false)
                    {
                        if (uny.rigidList[i].activation.velocityCorState == true)
                            RayfireMan.inst.RemoveVelocityActivationCor (uny.rigidList[i]);
                        if (uny.rigidList[i].activation.offsetCorState == true)
                            RayfireMan.inst.RemoveOffsetActivationCor (uny.rigidList[i]);
                    } 
                }
        }
        
        // Set unyielding rigids sim type by
        static void SetRigidUnySim (RayfireUnyielding uny)
        {
            if (Application.isPlaying == true && uny.simulationType != UnySimType.Original)
                for (int i = 0; i < uny.rigidList.Count; i++)
                {
                    uny.rigidList[i].simTp = (SimType)uny.simulationType;
                    RFPhysic.SetSimulationType (uny.rigidList[i].physics.rb, uny.rigidList[i].simTp,
                        ObjectType.Mesh, uny.rigidList[i].physics.gr, uny.rigidList[i].physics.si, uny.rigidList[i].physics.st);
                }
        }
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void ResetMeshRootSetup (RayfireRigid mRoot)
        {
            RayfireUnyielding[] unyList = mRoot.GetComponents<RayfireUnyielding>();
            for (int i = unyList.Length - 1; i >= 0; i--)
                unyList[i].rigidList = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Runtime Fragments
        /// /////////////////////////////////////////////////////////
        
        // Check for overlap with mesh Rigid. Copies Uny component in case of slice, so new slices will have uny gizmo.
        public static void SetUnyieldingFragments (RayfireRigid scr, bool slice)
        {
            // Not mesh demolition
            if (scr.objTp != ObjectType.Mesh)
                return;
            
            // Skip if convert to Connected Cluster and fragments wont need uny, rigid, rb components
            if (scr.mshDemol.cnv == RFDemolitionMesh.ConvertType.ConnectedCluster)
                return;

            // No fragments
            if (scr.HasFragments == false)
                return;
            
            // TODO collect layer mask by all layers -> int finalMask = RayfireUnyielding.ClusterLayerMask(scr);
            int layerMask = 1 << scr.gameObject.layer;
            
            // Overlapped objects: Copy uny, stay kinematic
            RayfireUnyielding[] unyArr = scr.GetComponents<RayfireUnyielding>();
            
            // Get all overlapped fragments
            Collider[]        colliders;
            HashSet<Collider> collidersHash;
            for (int u = 0; u < unyArr.Length; u++)
            {
                // Get box overlap colliders
                // Physics.OverlapBoxNonAlloc (unyArr[u].transform.TransformPoint (unyArr[u].centerPosition), unyArr[u].Extents, colliders, unyArr[u].transform.rotation, layerMask);
                colliders     = Physics.OverlapBox (unyArr[u].transform.TransformPoint (unyArr[u].centerPosition), unyArr[u].Extents, unyArr[u].transform.rotation, layerMask);
                collidersHash = new HashSet<Collider> (colliders);
                
                // Activate if do not overlap
                for (int i = 0; i < scr.fragments.Count; i++)
                {
                    // Activate not overlapped and copy to overlapped
                    if (collidersHash.Contains (scr.fragments[i].physics.mc) == true)
                    {
                        // Copy overlap uny to overlapped object
                        SetRigidUnyState (scr.fragments[i], unyArr[u].unyielding, unyArr[u].activatable);
                        
                        // Set simulation type
                        if (unyArr[u].simulationType != UnySimType.Original)
                            scr.fragments[i].simTp = (SimType)unyArr[u].simulationType;
                        
                        // Set simulation type
                        RFPhysic.SetSimulationType (scr.fragments[i].physics.rb, scr.fragments[i].simTp, 
                            scr.fragments[i].objTp, scr.fragments[i].physics.gr, scr.fragments[i].physics.si, scr.fragments[i].physics.st);
                        
                        // Copy rigid to overlapped fragments for further slices.
                        if (slice == true)
                            CopyUny (unyArr[u], scr.fragments[i].gameObject);
                        
                        // Start inactive coroutines
                        scr.fragments[i].InactiveCors();
                    } 
                }
            }
        }
        
        // Set unyielding state
        static void SetRigidUnyState (RayfireRigid rigid, bool uny, bool act)
        {
            rigid.activation.uny = uny;
            rigid.activation.atb = act;
            
            // Stop velocity and offset activation coroutines for not activatable uny objects 
            if (uny == true && act == false)
            {
                RayfireMan.inst.RemoveVelocityActivationCor (rigid);
                RayfireMan.inst.RemoveOffsetActivationCor (rigid);
            }
        }
        
        // Copy unyielding component
        static void CopyUny (RayfireUnyielding source, GameObject target)
        {
            RayfireUnyielding newUny = target.AddComponent<RayfireUnyielding>();

            // Copy position
            Vector3 globalCenter = source.transform.TransformPoint (source.centerPosition);
            newUny.centerPosition = newUny.transform.InverseTransformPoint (globalCenter);

            // Copy size
            Vector3 localScale = source.transform.localScale;
            newUny.size   =  source.size;
            newUny.size.x *= localScale.x;
            newUny.size.y *= localScale.y;
            newUny.size.z *= localScale.z;

            // Copy properties
            newUny.simulationType = source.simulationType;
            newUny.unyielding     = source.unyielding;
            newUny.activatable    = source.activatable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Root Setup
        /// /////////////////////////////////////////////////////////
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        void GetRigidRootUnyShardList(RayfireRigidRoot rigidRoot)
        {
            // Uny disabled
            if (enabled == false)
                return;

            // Get target mask TODO check fragments layer
            int mask = 0;
            
            // Check with rigid root shards colliders
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    mask = mask | 1 << rigidRoot.cluster.shards[i].tm.gameObject.layer;
                            
            // Get box overlap colliders
            Collider[]        colliders     = Physics.OverlapBox (transform.TransformPoint (centerPosition), Extents, transform.rotation, mask);
            HashSet<Collider> collidersHash = new HashSet<Collider> (colliders);

            // Check with rigid root shards colliders
            shardList = new List<RFShard>();
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    if (collidersHash.Contains (rigidRoot.cluster.shards[i].col) == true)
                        shardList.Add (rigidRoot.cluster.shards[i]);
        }
        
        // Set sim amd uny states for cached shards
        public void SetRigidRootUnyShardList()
        {
            // No shards
            if (shardList.Count == 0)
                return;
            
            // Iterate cached shards
            for (int i = 0; i < shardList.Count; i++)
            {
                // Set uny states
                shardList[i].uny = unyielding;
                shardList[i].act = activatable;
                
                // Set sim states
                if (simulationType != UnySimType.Original)
                    shardList[i].sm = (SimType)simulationType;
            }

            // TODO Stop velocity and offset activation coroutines for not activatable uny objects (copy from above)
        }

        // Set unyielding shards. Should be After collider set and Before SetPhysics because Changes simType.
        public static void SetUnyielding(RayfireRigidRoot rr)
        {
            // Set by rigid root
            for (int i = 0; i < rr.rigidRootShards.Count; i++)
            {
                rr.rigidRootShards[i].uny = rr.activation.uny;
                rr.rigidRootShards[i].act = rr.activation.atb;
            }
            
            // Set by mesh root
            for (int i = 0; i < rr.meshRootShards.Count; i++)
            {
                rr.meshRootShards[i].uny = rr.meshRootShards[i].rigid.activation.uny;
                rr.meshRootShards[i].act = rr.meshRootShards[i].rigid.activation.atb;
            }
            
            // Set by connected cluster
            for (int i = 0; i < rr.connClusterShards.Count; i++)
            {
                rr.connClusterShards[i].uny = rr.connClusterShards[i].rigid.activation.uny;
                rr.connClusterShards[i].act = rr.connClusterShards[i].rigid.activation.atb;
            }

            // Set by uny components
            if (rr.HasUny == true)
            {
                for (int i = 0; i < rr.unyList.Length; i++)
                {
                    rr.unyList[i].GetRigidRootUnyShardList (rr);
                    rr.unyList[i].SetRigidRootUnyShardList();
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activate
        /// /////////////////////////////////////////////////////////
        
        // Activate inactive\kinematic shards/fragments
        public void Activate()
        {
            // Activate all rigids, init connectivity check after last activation, nullify connectivity for every
            if (HasRigids == true)
            {
                for (int i = 0; i < rigidList.Count; i++)
                {
                    // Activate if activatable
                    if (rigidList[i].activation.atb == true)
                    {
                        rigidList[i].Activate (i == rigidList.Count - 1);
                        rigidList[i].activation.cnt = null;
                    }
                }
            }

            // Activate connected clusters shards
            if (HasShards == true)
            {
                // Collect shards colliders
                Collider[] colliders = new Collider[shardList.Count];
                for (int i = 0; i < shardList.Count; i++)
                    if (shardList[i].col != null)
                        colliders[i] = shardList[i].col;

                // No colliders
                if (colliders.Length == 0)
                    return;
                
                // Get Unyielding shards FIXME doesn't demolish cluster root if all shards activated
                List<RFShard> shards = RFDemolitionCluster.DemolishConnectedCluster (rigidHost, colliders);

                // Activate
                if (shards != null && shards.Count > 0)
                    for (int i = 0; i < shards.Count; i++)
                        RFActivation.ActivateShard (shards[i], null);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Had child cluster
        bool HasRigids { get { return rigidList != null && rigidList.Count > 0; } }
        bool HasShards { get { return shardList != null && shardList.Count > 0; } }
        
        // Get final extents
        Vector3 Extents
        {
            get
            {
                Vector3 ext        = size / 2f;
                Vector3 lossyScale = transform.lossyScale;
                ext.x *= lossyScale.x;
                ext.y *= lossyScale.y;
                ext.z *= lossyScale.z;
                return ext;
            }
        }
    }
}