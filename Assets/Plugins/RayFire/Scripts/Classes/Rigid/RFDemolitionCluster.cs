using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFDemolitionCluster
    {
        public enum RFDetachType
        {
            RatioToSize = 0,
            WorldUnits  = 3
        }
        
        // UI
        [FormerlySerializedAs ("connectivity")] public    ConnectivityType cnt;
        public                                            FragSimType      sim;
        [FormerlySerializedAs ("minimumArea")] public     float            mAr;
        [FormerlySerializedAs ("minimumSize")] public     float            mSz;
        [FormerlySerializedAs ("percentage")]  public     int              pct;
        public                                            int              seed;
        public                                            RFDetachType     type;
        public                                            int              ratio;
        public                                            float            units;
        [FormerlySerializedAs ("shardArea")]       public int              sAr;
        [FormerlySerializedAs ("shardDemolition")] public bool             sDm;
        [FormerlySerializedAs ("minAmount")]       public int              mnAm;
        [FormerlySerializedAs ("maxAmount")]       public int              mxAm;
        [FormerlySerializedAs ("demolishable")]    public bool             cDm;
        public                                            RFCollapse       collapse;
        public                                            int              clsCount;
        public                                            RFCluster        cluster;
        public                                            List<RFCluster>  minorClusters;
        public                                            bool             cn;
        public                                            bool             nd;
        public                                            int              am; // initial amount, for amount integrity getter
        
        // Non serialized
        [NonSerialized] public RFBackupCluster backup;
        [NonSerialized] public float           damageRadius;

        // New cluster name appendix
        public static string nameApp = "_cls_";
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDemolitionCluster()
        {
            InitValues();
            LocalReset();
        }

        void InitValues()
        {
            cnt           = ConnectivityType.ByBoundingBox;
            sim           = FragSimType.Dynamic;
            mAr           = 0f;
            mSz           = 0f;
            pct           = 0;
            seed          = 0;
            type          = RFDetachType.RatioToSize;
            ratio         = 15;
            units         = 1f;
            sAr           = 70;
            sDm           = false;
            mnAm          = 3;
            mxAm          = 6;
            cDm           = true;
            collapse      = null;
            clsCount      = 1;
            cluster       = null;
            minorClusters = null;
            cn            = true;
            nd            = false;
            am            = 0;
        }
        
        // Reset
        public void LocalReset()
        {
            damageRadius = 0f;
        }

        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
            backup = null;
        }

        // Copy from
        public void CopyFrom (RFDemolitionCluster source)
        {
            cnt   = source.cnt;
            sim   = source.sim;
            mAr   = source.mAr;
            mSz   = source.mSz;
            pct   = source.pct;
            seed  = source.seed;
            type  = source.type;
            ratio = source.ratio;
            units = source.units;
            sAr   = source.sAr;
            sDm   = source.sDm;
            mxAm  = source.mxAm;
            mnAm  = source.mnAm;
            cDm   = source.cDm;
            cn    = source.cn;
            nd    = source.nd;
            
            LocalReset();
        }

        /// /////////////////////////////////////////////////////////
        /// Clusterize
        /// /////////////////////////////////////////////////////////

        // Reset Connected cluster editor setup
        public static void ResetClusterize (RayfireRigid scr)
        {
            RFPhysic.DestroyColliders (scr);
            scr.physics.ign                     = null;
            scr.physics.cc                      = null;
            scr.clsDemol.cluster       = new RFCluster();
            scr.clsDemol.clsCount      = 1;
            scr.clsDemol.minorClusters = null;
            scr.tsf                       = null;
        }
        
        // Editor Clusterize
        public static void ClusterizeEditor (RayfireRigid scr)
        {
            // Reset
            ResetClusterize (scr);
            
            // Basic components
            scr.SetComponentsBasic();
            
            // Clusterize and return state
            Clusterize (scr);
        }
        
        // Clusterize
        public static void Clusterize (RayfireRigid scr)
        {
            // TODO skip if minor nested cluster
            if (scr.objTp == ObjectType.NestedCluster)
                if (scr.clsDemol.cluster.id > 1)
                    return;
            
            // No children
            if (scr.tsf.childCount == 0)
            {
                // Fail and warning
                scr.physics.exclude = true;
                RayfireMan.Log ($"{RFLog.scr_rig}{scr.name}{RFLog.rig_children}", scr.gameObject);
                return;
            }
            
            // Missing shards check TODO test with nested cluster
            if (RFCluster.IntegrityCheck (scr.clsDemol.cluster) == false)
                scr.clsDemol.cluster = new RFCluster();

            // Clusterize
            if (scr.objTp == ObjectType.NestedCluster)
                ClusterizeNested (scr);
            else if (scr.objTp == ObjectType.ConnectedCluster)
                ClusterizeConnected (scr);

            // Reinit connected cluster shards non serialized fields if main cluster not initialized
            RFCluster.InitCluster (scr, scr.clsDemol.cluster);
            
            // Set colliders
            RFPhysic.SetClusterCollidersByShards (scr);

            // Ignore collision
            RFPhysic.SetIgnoreColliders(scr.physics, scr.clsDemol.cluster.shards);
            
            // Set particles
            if (Application.isPlaying == true)
                RFPoolingParticles.InitializeParticles(scr);
            
            // Set unyielding state
            RayfireUnyielding.ClusterSetup (scr);
            
            // Save backup if cluster will be restored
            RFBackupCluster.BackupConnectedCluster (scr);
        }
        
        // Clusterize connected cluster 
        static void ClusterizeConnected (RayfireRigid scr)
        {
            if (scr.clsDemol.cluster.shards.Count == 0)
            {
                // Create main cluster
                scr.clsDemol.cluster              = new RFCluster();
                scr.clsDemol.cluster.id           = RFCluster.GetUniqClusterId (scr.clsDemol.cluster);
                scr.clsDemol.cluster.tm           = scr.tsf;
                scr.clsDemol.cluster.pos          = scr.tsf.position;
                scr.clsDemol.cluster.rot          = scr.tsf.rotation;
                scr.clsDemol.cluster.depth        = 0;
                scr.clsDemol.cluster.initialized  = true;
                scr.clsDemol.cluster.demolishable = true;
                scr.clsDemol.cluster.rigid        = scr;
                
                // Set shards for main cluster
                RFShard.SetConnectedClusterShards(scr.clsDemol.cluster, scr.clsDemol.cnt, true);
                
                // Set shard neibs
                RFShard.SetShardNeibs (scr.clsDemol.cluster.shards, scr.clsDemol.cnt, 
                    scr.clsDemol.mAr, scr.clsDemol.mSz, 
                    scr.clsDemol.pct, scr.clsDemol.seed);
                
                // Set range for area and size
                RFCollapse.SetRangeData (scr.clsDemol.cluster, scr.clsDemol.pct);
                
                // Set initial shards amount
                scr.clsDemol.am = scr.clsDemol.cluster.shards.Count;
            }
        }

        // Check if minor clusters tm is child of parent cluster and set as child cluster if so
        static void InitNestedCluster(RFCluster parentCluster, List<RFCluster> minorClusters)
        {
            // Define child clusters
            for (int i = 0; i < minorClusters.Count; i++)
            {
                if (minorClusters[i].tm.parent == parentCluster.tm)
                {
                    if (parentCluster.childClusters == null)
                        parentCluster.childClusters = new List<RFCluster>();
                    parentCluster.childClusters.Add (minorClusters[i]);
                }
            }
            
            // Repeat for new child clusters
            if (parentCluster.childClusters != null && parentCluster.childClusters.Count > 0)
                for (int i = 0; i < parentCluster.childClusters.Count; i++)
                    InitNestedCluster(parentCluster.childClusters[i], minorClusters);
        }
        
        // Create one cluster which includes only children meshes, not children of children meshes.
        static void ClusterizeNested (RayfireRigid scr)
        {
            // Unpack Editor Setup cluster. RUNTIME ONLY
            if (Application.isPlaying == true && scr.clsDemol.HasMinorClusters == true)
            {
                // Reinit child clusters
                InitNestedCluster(scr.clsDemol.cluster, scr.clsDemol.minorClusters);

                // Reinit main cluster
                for (int i = 0; i < scr.clsDemol.minorClusters.Count; i++)
                    scr.clsDemol.minorClusters[i].mainCluster = scr.clsDemol.cluster;
            }
            
            // Has not minor cluster. Never was Clusterized. DO NOT REPEAT FOR MINOR CLUSTERS
            if (scr.clsDemol.HasMinorClusters == false && scr.clsDemol.cluster.id == -1)
            {
                // Create main cluster
                scr.clsDemol.cluster              = new RFCluster();
                scr.clsDemol.cluster.id           = RFCluster.GetUniqClusterId (scr.clsDemol.cluster);
                scr.clsDemol.cluster.tm           = scr.tsf;
                scr.clsDemol.cluster.pos          = scr.tsf.position;
                scr.clsDemol.cluster.rot          = scr.tsf.rotation;
                scr.clsDemol.cluster.depth        = 0;
                scr.clsDemol.cluster.initialized  = true;
                scr.clsDemol.cluster.demolishable = true;
                
                // List to store all clusters for prefabs
                scr.clsDemol.cluster.rigid = scr;
                scr.clsDemol.minorClusters = new List<RFCluster>();
                
                // Create child clusters and their child clusters
                ClusterizeNestedRecursive(scr, scr.tsf, scr.clsDemol.cluster, scr.clsDemol.cnt);
            }
        }

        // Setup shards and child clusters by children
        static void ClusterizeNestedRecursive(RayfireRigid scr, Transform transform, RFCluster cluster, ConnectivityType connectivity)
        {
            // Get shards and clusters transforms
            Transform       tm;
            List<Transform> tmShards   = new List<Transform>();
            List<Transform> tmClusters = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                tm = transform.GetChild (i);
                if (tm.childCount == 0) 
                    tmShards.Add (tm);
                else 
                    tmClusters.Add (tm);
            }

            // Setup shards
            if (tmShards.Count > 0)
                RFShard.SetShardsByTransformList (cluster, tmShards.ToArray(), connectivity, true);

            // Setup child Clusters
            if (tmClusters.Count > 0)
            {
                for (int i = tmClusters.Count - 1; i >= 0; i--)
                {
                    // TODO check if children have meshfilter
                    
                    // Create main cluster
                    RFCluster newCluster    = new RFCluster();
                    newCluster.mainCluster  = scr.clsDemol.cluster;
                    newCluster.id           = RFCluster.GetUniqClusterId (newCluster);
                    newCluster.tm           = tmClusters[i];
                    newCluster.pos          = tmClusters[i].position;
                    newCluster.rot          = tmClusters[i].rotation;
                    newCluster.depth        = 0;
                    newCluster.initialized  = true;
                    newCluster.demolishable = true;
                    
                    // Other properties
                    newCluster.rigid       = newCluster.tm.GetComponent<RayfireRigid>();
                    newCluster.bound       = RFCluster.GetChildrenBound (newCluster.tm);
                    
                    // Save in minor cluster
                    scr.clsDemol.minorClusters.Add (newCluster);
                    
                    // Create Child Clusters and shards for new cluster
                    ClusterizeNestedRecursive (scr, tmClusters[i], newCluster, connectivity);

                    // Add new child cluster
                    cluster.AddChildCluster (newCluster);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Demolish cluster to children nodes
        public static bool DemolishCluster (RayfireRigid scr, Collider[] detachColliders = null)
        {
            if (scr.objTp != ObjectType.NestedCluster && scr.objTp != ObjectType.ConnectedCluster)
                return false;
            
            // Skip if not runtime
            if (scr.dmlTp != DemolitionType.Runtime)
                return true;

            // TODO inherit original cluster velocity
            
            // Cluster demolition
            if (scr.objTp == ObjectType.NestedCluster)
                DemolishNestedCluster (scr);
            else if (scr.objTp == ObjectType.ConnectedCluster)
                DemolishConnectedCluster (scr, detachColliders);
            
            // Demolition executed
            scr.limitations.demolitionShould = false;

            // Delete if cluster was completely demolished
            if (scr.limitations.demolished == true)
                RayfireMan.DestroyFragment (scr, null);

            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connected Cluster Demolition
        /// /////////////////////////////////////////////////////////

        // Demolish connected cluster and return detached shards
        public static List<RFShard> DemolishConnectedCluster (RayfireRigid scr, Collider[] detachColliders = null)
        {
            // Get colliders to detach
            if (detachColliders == null)
                detachColliders = GetDetachColliders (scr);
            
            // No colliders to detach
            if (detachColliders.Length == 0)
                return null; 
            
            // Get detach shards area and remove them from cluster. Includes not connected solo shards from cluster
            List<RFShard> detachShards = DetachShardsByColliders (scr, detachColliders);
            
            // No shards to detach. Cluster not demolished
            if (detachShards.Count == 0)
                return null;
            
            // Get amount of clusters to create and amount of edge shards
            int clusterAmount = Random.Range (scr.clsDemol.mxAm, scr.clsDemol.mnAm + 1);
            
            // All Shards was detached TODO 
            // if (scr.clusterDemolition.cluster.shards.Count == 0)
            //     return;
            
            // All shards should be clusterized to one cluster. Stop
            // if (SameClusterCheck(scr, detachShards, shardAmount, clusterAmount) == true)
            //    return;


            
            
            // Clear fragments in case of previous demolition
            if (scr.HasFragments == true)
                scr.fragments.Clear();
            
            // Dynamic cluster connectivity check, all clusters are equal, pick biggest to keep as original 
            if (scr.simTp == SimType.Dynamic || scr.simTp == SimType.Sleeping || scr.simTp == SimType.Kinematic || scr.simTp == SimType.Inactive)
            {
                // Check left cluster shards for connectivity and collect not connected child clusters. Should be before ClusterizeDetachShards
                RFCluster.ConnectivityCheck (scr.clsDemol.cluster);
             
                // Cluster is not connected. If not main cluster then set biggest child cluster shards to original cluster. 
                RFCluster.ReduceChildClusters (scr.clsDemol.cluster);
            }

            /*
            // Kinematic/Inactive cluster, Connectivity check if cluster has uny shards. Main cluster keeps all not activated
            if (scr.simTp == SimType.Kinematic || scr.simTp == SimType.Inactive)
            {
                // Connectivity check. Separate not connected groups to child clusters
                RFCluster.ConnectivityUnyCheck (scr.clsDemol.cluster);
                
                // No shards in cluster. Cluster is not connected. If not main cluster then set biggest child cluster shards to original cluster. 
                if (scr.clsDemol.cluster.shards.Count == 0)
                    RFCluster.ReduceChildClusters (scr.clsDemol.cluster);
            }
            */
            
            // TODO Set solo uny detached shards back to cluster or prevent from collection
            
            // Clusterize detached shards if needed. Update child clusters and detached solo shards list
            ClusterizeDetachShards (scr, detachShards, clusterAmount, 0);

            // Init final cluster ops
            PostDemolitionCluster (scr, detachShards);
            
            return detachShards;
        }

        // Get colliders to detach
        static Collider[] GetDetachColliders (RayfireRigid scr)
        {
            // TODO instead overlap, get contact shard, go through all neibs and collect all in radius,
            // TODO exclude and mark all not in radius, stop when there is no grow anymore
           
            // Get damaged colliders
            if (scr.objTp == ObjectType.ConnectedCluster && scr.damage.shr == true)
            {
                List<Collider> damagedCollider = new List<Collider>();
                for (int i = 0; i < scr.clsDemol.cluster.shards.Count; i++)
                    if (scr.clsDemol.cluster.shards[i].dm > scr.damage.max)
                        damagedCollider.Add (scr.clsDemol.cluster.shards[i].col);
                
                // Detach damaged shards
                if (damagedCollider.Count > 0)
                {
                    scr.clsDemol.damageRadius = 0f;
                    return damagedCollider.ToArray();
                }
            }
            
            // Get colliders by damage radius and reset it
            if (scr.clsDemol.damageRadius > 0)
            {
                Collider[] colliders = Physics.OverlapSphere (scr.limitations.contactVector3, scr.clsDemol.damageRadius, 1 << scr.gameObject.layer);
                scr.clsDemol.damageRadius = 0f;
                return colliders;
            }
            
            // Get detach colliders by manual damage radius
            if (scr.clsDemol.type == RFDetachType.WorldUnits)
                if (scr.clsDemol.units > 0)
                    return Physics.OverlapSphere (scr.limitations.contactVector3, scr.clsDemol.units, 1 << scr.gameObject.layer);
            
            // Use all colliders if contactRadius is 100%
            if (scr.clsDemol.ratio == 100)
                return scr.physics.cc.ToArray();
            
            // Get colliders by contactRadius by overlap
            float contactRadius = scr.limitations.bboxSize / 100f * scr.clsDemol.ratio;
            return Physics.OverlapSphere (scr.limitations.contactVector3, contactRadius, 1 << scr.gameObject.layer);
        }
        
        // Create runtime clusters
        static List<RFShard> DetachShardsByColliders (RayfireRigid scr, Collider[] detachColliders)
        {
            // Collect detach shards. Mark removed shards
            List<RFShard>     detachShards        = new List<RFShard>();
            HashSet<Collider> detachCollidersHash = new HashSet<Collider>(detachColliders);

            // Detach shards for dynamic/sleeping cluster. Detach all
            for (int i = scr.physics.cc.Count - 1; i >= 0; i--)
            {
                // Skip not activatable uny shard
                if (scr.clsDemol.cluster.shards[i].uny == true && scr.clsDemol.cluster.shards[i].act == false)
                    continue;
                    
                // Collect other shards
                if (detachCollidersHash.Contains (scr.physics.cc[i]) == true)
                {
                    detachShards.Add (scr.clsDemol.cluster.shards[i]);
                    scr.clsDemol.cluster.shards.RemoveAt (i);
                    scr.physics.cc.RemoveAt (i);
                }
            }
            
            // No detach shards. Cluster was not demolished
            if (detachShards.Count == 0)
                return detachShards;
            
            // Original cluster has only one shard left. Add it to all detached shards
            if (scr.clsDemol.cluster.shards.Count == 1)
            {
                detachShards.Add (scr.clsDemol.cluster.shards[0]);
                scr.clsDemol.cluster.shards.Clear();
                scr.physics.cc.Clear();
            }

            // Update shards cluster data before reinit left and detached shards neib data
            for (int i = 0; i < detachShards.Count; i++)
                detachShards[i].cluster  = null;
            
            // Original cluster still has shards
            if (scr.clsDemol.cluster.shards.Count > 0)
            {
                // Remove neib shards which are not in current cluster anymore
                RFShard.ReinitNeibs (scr.clsDemol.cluster.shards);
                
                // TODO detach shards with one neib now which had detached shards as neibs
                
                // Collect solo shards, remove from cluster
                RFCluster.DetachSoloShards (scr.clsDemol.cluster, detachShards);
            }
            
            return detachShards;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Nested cluster demolition
        /// /////////////////////////////////////////////////////////
        
        // Demolish nested cluster
        static void DemolishNestedCluster (RayfireRigid scr)
        {
            // Set demolished state. IMPORTANT should be here
            scr.limitations.demolished = true;
            
            List<RFShard> detachShards = new List<RFShard>(scr.clsDemol.cluster.shards.Count);
            for (int i = 0; i < scr.clsDemol.cluster.shards.Count; i++)
                detachShards.Add (scr.clsDemol.cluster.shards[i]);

            // Clear list if not going to be used
            if (scr.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
                scr.clsDemol.cluster.shards.Clear();

            // Create child clusters and shards
            PostDemolitionCluster (scr, detachShards);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Post Demolition
        /// /////////////////////////////////////////////////////////
        
        // Final ops at connected cluster demolition, slice, collapse
        public static void PostDemolitionCluster (RayfireRigid scr, List<RFShard> detachShards)
        {
            // Prepare
            if (scr.fragments == null) 
                scr.fragments = new List<RayfireRigid>();
            else 
                scr.fragments.Clear();

            // Create Rigid Shards
            SetupDetachShards (scr, detachShards);
            
            // Create Rigid Child cluster
            CreateChildClusters (scr, scr.clsDemol.cluster.childClusters);
            
            // Update properties
            UpdateOriginalCluster (scr);
            
            // Set velocity
            RFPhysic.SetFragmentsVelocity (scr);
            
            // Fading
            RFFade.FadeClusterShards(scr, detachShards);
            
            // Init particles
            RFPoolingEmitter.SetHostDemolition(scr);
            
            // Init sound
            RFSound.DemolitionSound(scr.sound, scr.limitations.bboxSize);
            
            // Event
            RFDemolitionEvent.RigidDemolitionEvent (scr);
            
            // Set layer and tag
            RFFragmentProperties.SetLayer(scr);
            RFFragmentProperties.SetTag(scr);
        }
        
        // Create runtime clusters
        static void SetupDetachShards (RayfireRigid scr, List<RFShard> detachShards)
        {
            // No shards to create
            if (detachShards.Count == 0)
                return;

            // Add rigid component to detached children in case they have no RigidRoot parent
            if (scr.rigidRoot == null)
                AddRigidComponent (scr, detachShards);
            else if (scr.rigidRoot != null)
                SetRigidRootShard (scr, detachShards);
        }
        
        // Create Rigid Child cluster
        static void CreateChildClusters (RayfireRigid scr, List<RFCluster> childClusters)
        {
            // No child clusters to create
            if (childClusters == null || childClusters.Count == 0)
                return;

            // Create child clusters
            for (int i = 0; i < childClusters.Count; i++)
            {
                // Set demolishable state
                childClusters[i].demolishable = scr.clsDemol.cDm;
                
                // Copy Range data for runtime clusters
                RFCollapse.CopyRangeData (childClusters[i], scr.clsDemol.cluster);
                
                // Create cluster
                CreateClusterRuntime (scr, childClusters[i]);
            }
        }

        // Demolish connected cluster by colliders
        static void UpdateOriginalCluster (RayfireRigid scr)
        {
            // All shards were detached. Set demolished state
            if (scr.clsDemol.cluster.shards.Count == 0)
            {
                scr.physics.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                scr.physics.rb.isKinematic = true;
                scr.limitations.demolished = true;
                
                // Remove from minor cluster list
                if (scr.objTp == ObjectType.ConnectedCluster)
                    if (scr.clsDemol.cluster.mainCluster != null && scr.clsDemol.cluster.mainCluster.rigid != null)
                        scr.clsDemol.cluster.mainCluster.rigid.clsDemol.minorClusters.Remove (scr.clsDemol.cluster);
                return;
            }

            // Original cluster shards not going to be changed anymore. Reinit colliders list
            RFPhysic.CollectClusterColliders (scr, scr.clsDemol.cluster);
            
            // Update mass
            RFPhysic.SetDensity (scr);
            
            // Uny sim has priority over ClsDemol Sim. Get first uny shard and use its sim by overlapped Uny
            SetClusterSimulationType (scr, scr.clsDemol.cluster);
            
            // Cluster was demolished but not completely, reuse what left 
            scr.limitations.birthTime = Time.time;
            
            // Update reduced bound
            scr.clsDemol.cluster.bound = RFCluster.GetShardsBound (scr.clsDemol.cluster.shards);
            scr.limitations.bboxSize = scr.clsDemol.cluster.bound.size.magnitude;
            
            // Reset original cluster center of mass because of detached colliders: solo shards and child clusters
            scr.physics.rb.ResetCenterOfMass();

            // Set velocity after demolition
            scr.physics.rb.velocity = scr.physics.velocity * scr.physics.dm;
        }
        
        // Demolish connected cluster by colliders
        static void UpdateOriginalClusterOLD (RayfireRigid scr)
        {
            // All shards were detached. Set demolished state
            if (scr.clsDemol.cluster.shards.Count == 0)
            {
                scr.physics.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                scr.physics.rb.isKinematic = true;
                scr.limitations.demolished = true;
                
                // Remove from minor cluster list
                if (scr.objTp == ObjectType.ConnectedCluster)
                    if (scr.clsDemol.cluster.mainCluster != null && scr.clsDemol.cluster.mainCluster.rigid != null)
                        scr.clsDemol.cluster.mainCluster.rigid.clsDemol.minorClusters.Remove (scr.clsDemol.cluster);
                return;
            }

            // Original cluster shards not going to be changed anymore. Reinit colliders list
            RFPhysic.CollectClusterColliders (scr, scr.clsDemol.cluster);
            
            // Update mass
            RFPhysic.SetDensity (scr);
            
            // Set dynamic if cluster inherit from kinematic/inactive but has no uny shards
            if (scr.simTp == SimType.Kinematic || scr.simTp == SimType.Inactive)
                if(scr.activation.con == true && scr.clsDemol.cluster.UnyieldingByShard == false)
                    scr.Activate();
            
            // Cluster was demolished but not completely, reuse what left 
            scr.limitations.birthTime = Time.time;
            
            // Update reduced bound
            scr.clsDemol.cluster.bound = RFCluster.GetShardsBound (scr.clsDemol.cluster.shards);
            scr.limitations.bboxSize   = scr.clsDemol.cluster.bound.size.magnitude;
            
            // Reset original cluster center of mass because of detached colliders: solo shards and child clusters
            scr.physics.rb.ResetCenterOfMass();

            // Set velocity after demolition
            scr.physics.rb.velocity = scr.physics.velocity * scr.physics.dm;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation
        /// /////////////////////////////////////////////////////////
        
        // Uny sim has priority over ClsDemol Sim. Get first uny shard and use its sim by overlapped Uny
        static SimType GetClusterSimType(RayfireRigid scr, RFCluster cluster)
        {
            if (cluster.UnyieldingByShard == true)
                return cluster.UnyieldingSimByShard;
            
            return (SimType)scr.clsDemol.sim;
        }
        
        // Set simulation type for cluster based on its Rigid and Uny shards. Activate if Inactive/Kinematik to Dynamic
        static void SetClusterSimulationType(RayfireRigid scr, RFCluster cluster)
        {
            // Uny sim has priority over ClsDemol Sim. Get first uny shard and use its sim by overlapped Uny
            SimType clusterSim = GetClusterSimType(scr, cluster);
            
            // Change original cluster sim state if different
            if (scr.simTp != clusterSim)
            {
                // Activate if should be dynamic
                if (clusterSim == SimType.Dynamic)
                    scr.Activate();
                
                // Deactivate
                else if (clusterSim == SimType.Inactive || clusterSim == SimType.Kinematic)
                {
                    scr.simTp = clusterSim;
                    
                    // Change rigidbody sim
                    RFPhysic.SetSimulationType (scr.physics.rb, scr.simTp, scr.objTp, scr.physics.gr, scr.physics.si); 
                    
                    // Start Inactive damping coroutine
                    scr.InactiveCors();
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Slice
        /// /////////////////////////////////////////////////////////
        
        // Slice connected cluster
        public static void SliceConnectedCluster (RayfireRigid scr)
        {
            // Get detach distance
            float detachDistance = 0f;
            if (scr.clsDemol.type == RFDetachType.WorldUnits)
                if (scr.clsDemol.units > 0)
                    detachDistance = scr.clsDemol.units;
            
            // Use all colliders if contactRadius is 100%
            if (scr.clsDemol.type == RFDetachType.RatioToSize)
                detachDistance = scr.limitations.bboxSize / 100f * scr.clsDemol.ratio;
            
            // Get two clusters for planes sides
            List<RFShard> detachShards = new List<RFShard>();
            List<RFShard> cluster1Shards = new List<RFShard>();
            List<RFShard> cluster2Shards = new List<RFShard>();

            // Separate shards by slice plane
            Vector3 shardPos;
            Plane plane = new Plane(scr.limitations.slicePlanes[1], scr.limitations.slicePlanes[0]);
            scr.limitations.slicePlanes.Clear();
            for (int s = 0; s < scr.clsDemol.cluster.shards.Count; s++)
            {
                // Save position
                shardPos = scr.clsDemol.cluster.shards[s].tm.position;
               
                // Check distance and add to detach shards if too close
                if (detachDistance > 0)
                {
                    if (Math.Abs(plane.GetDistanceToPoint (shardPos)) < detachDistance)
                    {
                        detachShards.Add (scr.clsDemol.cluster.shards[s]);
                        scr.clsDemol.cluster.shards[s].cluster = null;
                        continue;
                    }
                }
                
                // Get plane side
                if (plane.GetSide (shardPos) == true)
                    cluster1Shards.Add (scr.clsDemol.cluster.shards[s]);
                else
                    cluster2Shards.Add (scr.clsDemol.cluster.shards[s]);
            }
            
            // Check clusters for solo shards and send them to detach shards
            if (cluster1Shards.Count == 1)
            {
                detachShards.Add (cluster1Shards[0]);
                cluster1Shards.Clear();
            }
            if (cluster2Shards.Count == 1)
            {
                detachShards.Add (cluster2Shards[0]);
                cluster2Shards.Clear();
            }
            
            // No detach shards and One of the cluster equal to original cluster. Stop
            if (detachShards.Count == 0)
                if (cluster1Shards.Count == scr.clsDemol.cluster.shards.Count ||
                    cluster2Shards.Count == scr.clsDemol.cluster.shards.Count)
                    return;
            
            // Dynamic cluster connectivity check, all clusters are equal, pick biggest to keep as original 
            if (scr.simTp == SimType.Dynamic || scr.simTp == SimType.Sleeping)
            {
                // Prepare child clusters list
                if (cluster1Shards.Count >= 2 || cluster2Shards.Count >= 2)
                    if (scr.clsDemol.cluster.childClusters == null)
                        scr.clsDemol.cluster.childClusters = new List<RFCluster>();
                    else
                        scr.clsDemol.cluster.childClusters.Clear();
                
                // Setup cluster by one slice plane side shards. Connectivity check
                SetupPlaneShards (scr, cluster1Shards, detachShards);
                SetupPlaneShards (scr, cluster2Shards, detachShards);
             
                // Cluster is not connected. If not main cluster then set biggest child cluster shards to original cluster. 
                RFCluster.ReduceChildClusters (scr.clsDemol.cluster);
            }
            
            // Kinematic/ Inactive cluster, Connectivity check if cluster has uny shards. Main cluster keeps all not activated
            if (scr.simTp == SimType.Kinematic || scr.simTp == SimType.Inactive)
            {
                // Remove detach shards and child clusters shards from main cluster shards
                for (int i = scr.clsDemol.cluster.shards.Count - 1; i >= 0; i--)
                    if (scr.clsDemol.cluster.shards[i].cluster != scr.clsDemol.cluster)
                        scr.clsDemol.cluster.shards.RemoveAt (i);
                
                // Reset neibs
                RFShard.ReinitNeibs (scr.clsDemol.cluster.shards);
                
                // Check for uny connectivity
                RFCluster.ConnectivityUnyCheck (scr.clsDemol.cluster);
                
                // No shards in cluster. Cluster is not connected. If not main cluster then set biggest child cluster shards to original cluster. 
                if (scr.clsDemol.cluster.shards.Count == 0)
                    RFCluster.ReduceChildClusters (scr.clsDemol.cluster);
            }
            
            // Detach shards not going to be changed anymore. Reinit detach shards neib. 
            if (detachShards.Count > 0)
            {
                // DO LATER
                RFShard.ReinitNeibs (detachShards);
                
                // Get point in other way 
                scr.limitations.contactVector3 = plane.ClosestPointOnPlane (detachShards[0].tm.position);
                scr.limitations.contactNormal  = plane.normal;
                    
                // Get amount of clusters to create and amount of edge shards
                int clusterAmount = Random.Range (scr.clsDemol.mxAm, scr.clsDemol.mnAm + 1);
                
                // Clusterize detach shards but over plane
                ClusterizeDetachShards (scr, detachShards, clusterAmount, 1);
            }
            
            // Init final cluster ops
            PostDemolitionCluster (scr, detachShards);
        }

        // Setup cluster by one slice plane side shards. Connectivity check
        static void SetupPlaneShards (RayfireRigid scr, List<RFShard> clusterShards, List<RFShard> detachShards)
        {
            // Reinit cluster neibs
            if (clusterShards.Count >= 2)
            {
                RFCluster cluster    = new RFCluster();
                cluster.id           = 2;
                cluster.shards       = clusterShards;
                cluster.demolishable = true;
                for (int i = 0; i < cluster.shards.Count; i++)
                    cluster.shards[i].cluster = cluster;
                
                // Set main cluster
                cluster.mainCluster = scr.clsDemol.cluster;
                
                // Reset neibs 
                RFShard.ReinitNeibs (cluster.shards);
                
                // Remove not connected solo shards, shards without neibs
                for (int i = cluster.shards.Count - 1; i >= 0; i--)
                {
                    if (cluster.shards[i].neibShards.Count == 0)
                    {
                        detachShards.Add (cluster.shards[i]);
                        cluster.shards[i].cluster = null;
                        cluster.shards.RemoveAt (i);
                    }
                }

                // Cluster had only solo shards
                if (cluster.shards.Count == 0)
                    return;

                // Check connectivity, store all less not connected clusters as child cluster
                RFCluster.ConnectivityCheck (cluster);
                
                // Cluster is not connected. Set biggest child cluster shards to original cluster. Cant be 1 child cluster here
                RFCluster.ReduceChildClusters (cluster);
                
                // Collect original or not connected clusters
                scr.clsDemol.cluster.childClusters.Add (cluster);
                if (cluster.HasChildClusters == true)
                {
                    for (int c = 0; c < cluster.childClusters.Count; c++)
                        scr.clsDemol.cluster.childClusters.Add (cluster.childClusters[c]);
                    cluster.childClusters.Clear();
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition /Slicing common
        /// /////////////////////////////////////////////////////////
        
        // Demolish connected cluster by colliders
        static void ClusterizeDetachShards (RayfireRigid scr, List<RFShard> detachShards, int clusterAmount, int sortType)
        {
            // Clustering disabled or one solo shard. Nothing to clusterize
            if (scr.clsDemol.sAr == 100 || detachShards.Count <= 1)
                return;

            // TODO complete clusterization
            if (scr.clsDemol.sAr == 0)
            {
                
            }

            // Amount of solo shards
            int centerShardsAmount = detachShards.Count * scr.clsDemol.sAr / 100;
            
            // Not enough solo shards for clustering
            if (detachShards.Count - centerShardsAmount <= 1)
                return;
            
            // Shards less than clusters needed or group is too small to be clustered. Stop
            if (detachShards.Count <= clusterAmount)
                return;
 
            // Set up child cluster
            if (scr.clsDemol.cluster.childClusters == null)
                scr.clsDemol.cluster.childClusters = new List<RFCluster>();
            int startIndex = scr.clsDemol.cluster.childClusters.Count;
            
            // Preserve center shards
            RFShard[] center = null;
            if (centerShardsAmount > 0)
            {
                // Get center shards
                if (sortType == 0)
                    center = RFShard.SortByDistanceToPoint (detachShards.ToArray(), scr.limitations.contactVector3, centerShardsAmount);
                else if (sortType == 1)
                    center = RFShard.SortByDistanceToPlane (detachShards.ToArray(), scr.limitations.contactVector3, scr.limitations.contactNormal, centerShardsAmount);
                if (center != null)
                {
                    // Remove center shards from detach shards before slice them
                    HashSet<RFShard> centerHash = new HashSet<RFShard>(center);
                    for (int i = detachShards.Count - 1; i >= 0; i--)
                        if (centerHash.Contains (detachShards[i]) == true)
                            detachShards.RemoveAt (i);
                    
                    // Change center shards cluster to reinit detach shards neibs
                    for (int i = 0; i < center.Length; i++)
                        center[i].cluster = scr.clsDemol.cluster;
                }
            }

            // Separate group of shards to several child clusters
            DivideAllShards (scr.clsDemol.cluster, detachShards, clusterAmount);
            
            // Disable colliders TODO prevent empty clusters
            // DetachEdgeShards (scr, scr.clusterDemolition.cluster, detachShards, scr.clusterDemolition.edgeShardArea);
            
            // Detach shards with one neib from clusters
            DetachOneNeibShards (scr.clsDemol.cluster.childClusters, detachShards, centerShardsAmount, startIndex);

            // Add center shards back
            if (center != null)
            {
                // Nullify center shards cluster back
                for (int i = 0; i < center.Length; i++)
                    center[i].cluster = null;
                detachShards.AddRange (center);
            }
        }
        
        // Create runtime clusters
        public static void CreateClusterRuntime (RayfireRigid scr, RFCluster cluster)
        {
            if (cluster.shards.Count == 1)
                Debug.Log ("Solo cluster warning: " + scr.name);
            
            // Register in main cluster
            if (scr.objTp == ObjectType.ConnectedCluster)
            {
                if (scr.clsDemol.cluster != null && scr.clsDemol.cluster.mainCluster != null)
                {
                    cluster.mainCluster = scr.clsDemol.cluster.mainCluster;
                    scr.clsDemol.cluster.mainCluster.childClusters.Add (cluster);
                }

                if (cluster.mainCluster != null && cluster.mainCluster.rigid != null)
                {
                    if (cluster.mainCluster.rigid.clsDemol.minorClusters == null)
                        cluster.mainCluster.rigid.clsDemol.minorClusters = new List<RFCluster>();
                    cluster.mainCluster.rigid.clsDemol.minorClusters.Add (cluster);
                }
            }
            
            // Set bound if has not
            if (cluster.bound.size.magnitude == 0)
            {
                if (scr.objTp == ObjectType.ConnectedCluster)
                    cluster.bound = RFCluster.GetShardsBound (cluster.shards);
                else if (scr.objTp == ObjectType.NestedCluster)
                    cluster.bound = RFCluster.GetClusterBound(cluster);
            }

            // Create root for new connected cluster, set it's parent and register in storage
            if (cluster.tm == null)
            {
                // Create root
                RFCluster.CreateClusterRoot (cluster, cluster.shards[0].tm.position, scr.tsf.rotation,
                    scr.gameObject.layer, scr.gameObject.tag, scr.gameObject.name + nameApp + cluster.id);
                
                // Set parent and register
                RayfireMan.SetParentByManager (cluster.tm, scr.tsf);
            }
            
            // Set parent for nested cluster root, register in storage if not resettable 
            else
                RayfireMan.SetParentByManager (cluster.tm, scr.tsf, scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset);

            // Parent to main root. Nested cluster already has all shards rooted
            if (scr.objTp == ObjectType.ConnectedCluster)
            {
                for (int s = 0; s < cluster.shards.Count; s++)
                {
                    // Important. Disable and enable colliders before and after reparent
                    cluster.shards[s].col.enabled = false;
                    cluster.shards[s].tm.parent   = cluster.tm;
                    cluster.shards[s].col.enabled = true;

                    // Set cluster tm as parent for Rigid to track parent tm later
                    if (cluster.shards[s].rigid != null)
                        cluster.shards[s].rigid.rtP = cluster.tm;
                }
            }
            
            // Not for RigidRoot operations. RigidRoot has same Rigid as source scr and cluster Rigid
            if (scr != cluster.rigid)
            {
                // Check if already has rigid but it was not referenced, add if has not
                if (cluster.rigid == null)
                {
                    cluster.rigid = cluster.tm.gameObject.GetComponent<RayfireRigid>();
                    if (cluster.rigid == null)
                        cluster.rigid = cluster.tm.gameObject.AddComponent<RayfireRigid>();
                }

                // Collect fragment
                if (scr.fragments == null)
                    scr.fragments = new List<RayfireRigid>();
                scr.fragments.Add (cluster.rigid);

                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (cluster.rigid);

                // Set custom fragment simulation type if not inherited
                RFDemolitionMesh.SetClusterSimulationType (cluster.rigid, scr.simTp);
                
                // Copy particles
                RFPoolingParticles.CopyParticlesRigid (scr, cluster.rigid);
            }
            
            // Source Rigid has RigidRoot parent
            if (scr.rigidRoot != null)
            {
                // Set parent RigidRoot and Collect cluster in RigidRoot to delete at reset
                cluster.rigid.rigidRoot = scr.rigidRoot;
                cluster.rigid.rigidRoot.clusters.Add (cluster);
                
                // Register in storage to delete cluster tm in case of its demolition
                RayfireMan.inst.storage.RegisterRoot (cluster.tm);
            }
            
            // Set to mesh TODO why?
            cluster.rigid.physics.ct = RFColliderType.Mesh;
            
            // Set dynamic if cluster inherit from kinematic/inactive but has no uny shards
            if (cluster.rigid.simTp == SimType.Kinematic || cluster.rigid.simTp == SimType.Inactive)
                if (cluster.rigid.activation.con == true && cluster.UnyieldingByShard == false)
                    cluster.rigid.simTp = SimType.Dynamic;
            
            // Uny sim has priority over ClsDemol Sim. Get first uny shard and use its sim by overlapped Uny
            cluster.rigid.simTp = GetClusterSimType(cluster.rigid, cluster);
            
            // Set demolishable state for detached area clusters
            cluster.rigid.dmlTp = cluster.demolishable == true 
                ? DemolitionType.Runtime 
                : DemolitionType.None;
            
            // Do not destroy fragment because cluster could be reused 
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset)
                cluster.rigid.reset.action = RFReset.PostDemolitionType.DeactivateToReset;
            
            // Set cluster
            cluster.initialized = true;
            cluster.rigid.clsDemol.cluster = cluster;
            
            // Set colliders list
            RFPhysic.CollectClusterColliders (cluster.rigid, cluster.rigid.clsDemol.cluster);

            // Set initial shards amount
            cluster.rigid.clsDemol.am = cluster.rigid.clsDemol.cluster.shards.Count;
            
            // Turn on
            cluster.rigid.Initialize();
            
            // Nested cluster 
            if (cluster.rigid.objTp == ObjectType.NestedCluster)
            {
                // Copy depth
                cluster.rigid.limitations.depth = scr.limitations.depth;
                
                // Set current depth
                cluster.rigid.limitations.currentDepth = scr.limitations.currentDepth + 1;
                
                // last demolition depth
                if (scr.limitations.depth != 0 && cluster.rigid.limitations.currentDepth >= scr.limitations.depth)
                    cluster.rigid.dmlTp = DemolitionType.None;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Cluster Init
        /// /////////////////////////////////////////////////////////
        
        // Separate group of shards to several clusters by half
        static void DivideAllShards (RFCluster cluster, List<RFShard> detachShards, int amount)
        {
            // Get starting search index to exclude first child cluster from Connectivity check
            int startIndex = cluster.childClusters.Count;
            
            // Remove neib shards which still belongs to some cluster. Detach shards has no cluster
            RFShard.ReinitNeibs (detachShards);
            
            // Create base child cluster with detach shards
            RFCluster baseCLuster = new RFCluster();
            
            // Set main cluster
            baseCLuster.mainCluster = cluster.mainCluster == null 
                ? cluster 
                : cluster.mainCluster;

            // Set uniq id after main cluster defined
            baseCLuster.id = RFCluster.GetUniqClusterId (baseCLuster);
            
            // Set shards
            for (int i = detachShards.Count - 1; i >= 0; i--)
            {
                if (detachShards[i].neibShards.Count > 0)
                {
                    baseCLuster.shards.Add (detachShards[i]);
                    detachShards[i].cluster = baseCLuster;
                    detachShards.RemoveAt (i);
                }
            }
            cluster.childClusters.Add (baseCLuster);
            
            // Base cluster neib check
            RFShard.ReinitNeibs (baseCLuster.shards);
            
            // SLice to half amount - 1 times
            for (int i = 0; i < amount - 1; i++)
            {
                // Get biggest child cluster
                int biggestInd = 0;
                int biggestAmount = 0;
                for (int b = startIndex; b < cluster.childClusters.Count; b++)
                    if (cluster.childClusters[b].shards.Count > biggestAmount)
                    {
                        biggestInd = b;
                        biggestAmount = cluster.childClusters[b].shards.Count;
                    }

                // Biggest child cluster is very small. Stop
                if (cluster.childClusters[biggestInd].shards.Count < 4)
                    break;
                
                // Slice biggest group
                DivideShards (cluster, cluster.childClusters[biggestInd]);

                // Biggest cluster was not separated. stop
                if (biggestAmount == cluster.childClusters[biggestInd].shards.Count)
                    break;
            }

            // Check new child clusters for solo shards
            for (int c = cluster.childClusters.Count - 1; c >= startIndex; c--)
            {
                for (int s = cluster.childClusters[c].shards.Count - 1; s >= 0; s--)
                {
                    if (cluster.childClusters[c].shards[s].neibShards.Count == 0)
                    {
                        detachShards.Add (cluster.childClusters[c].shards[s]);
                        cluster.childClusters[c].shards[s].cluster = null;
                        cluster.childClusters[c].shards.RemoveAt (s);
                    }
                }

                // Remove clusters with no shards. All was solo and removed
                if (cluster.childClusters[c].shards.Count == 0)
                    cluster.childClusters.RemoveAt (c);
            }
            
            // Check for connectivity of child cluster
            for (int c = startIndex; c < cluster.childClusters.Count; c++)
            {
                // Connectivity
                RFCluster.ConnectivityCheck (cluster.childClusters[c]);

                // Cluster is not connected. Set biggest child cluster shards to original cluster. Cant be 1 child cluster here
                RFCluster.ReduceChildClusters (cluster.childClusters[c]);
            }

            // Set their child cluster as current child cluster and clear list
            for (int c = cluster.childClusters.Count - 1; c >= startIndex; c--)
            {
                if (cluster.childClusters[c].HasChildClusters == true)
                {
                    cluster.childClusters.AddRange (cluster.childClusters[c].childClusters);
                    cluster.childClusters[c].childClusters = null;
                } 
            }
        }

        // Separate group of shards to half. Do not return solo shards
        static void DivideShards (RFCluster mainCluster, RFCluster childCluster)
        {
            // Get group bound
            childCluster.bound = RFCluster.GetShardsBoundByPosition (childCluster.shards);
            
            // Get slice plane at middle of longest bound edge
            Plane plane = RFShard.GetSlicePlane (childCluster.bound);

            // Separate by plane and collect indexes of separated shards
            List<int> indexList = new List<int>(childCluster.shards.Count / 2);
            for (int i = 0; i < childCluster.shards.Count; i++)
                if (plane.GetSide (childCluster.shards[i].tm.position) == true)
                    indexList.Add (i);

            // One of the group contains only one shard. Group is too small. Stop.
            if (indexList.Count <= 1 || indexList.Count > childCluster.shards.Count - 2)
                return;

            // Create new group list and remove from input list
            RFCluster newChildCluster = new RFCluster();
            
            // Set main cluster
            newChildCluster.mainCluster = mainCluster.mainCluster == null 
                ? mainCluster 
                : mainCluster.mainCluster;
            
            // Set uniq id after main cluster defined
            newChildCluster.id = RFCluster.GetUniqClusterId (newChildCluster);

            // Set shards
            newChildCluster.shards = new List<RFShard>(indexList.Count);
            for (int i = indexList.Count - 1; i >= 0; i--)
            {
                newChildCluster.shards.Add (childCluster.shards[indexList[i]]);
                childCluster.shards[indexList[i]].cluster = newChildCluster;
                childCluster.shards.RemoveAt (indexList[i]);
            }
            
            // Collect new cluster
            mainCluster.childClusters.Add (newChildCluster);
            
            // Remove neib shards which still belongs to some cluster. Detach solo shards
            RFShard.ReinitNeibs (newChildCluster.shards);
            RFShard.ReinitNeibs (childCluster.shards);
        }
        
        // Detach shards from child clusters at edges
        public static void DetachEdgeShards (RFCluster cluster, List<RFShard> detachShards, int edgeShardArea)
        {
            if (edgeShardArea == 0)
                return;

            for (int i = 0; i < cluster.childClusters.Count; i++)
            {
                if (cluster.childClusters[i].shards.Count >= 5)
                {
                    int amount = cluster.childClusters[i].shards.Count * edgeShardArea / 100;
                    if (amount > 0)
                    {
                        // TODO detach in better way: farthest, lowest shard area, most lost neibs, ect
                        int startAmount = cluster.childClusters[i].shards.Count;
                        for (int j = cluster.childClusters[i].shards.Count - 1; j >= 0; j--)
                        {
                            // Stop if limit reached
                            if (amount == 0)
                                break;
                            
                            // Detach edge shard TODO random order
                            if (cluster.childClusters[i].shards[j].neibShards.Count < cluster.childClusters[i].shards[j].nAm)
                            {
                                amount--;
                                detachShards.Add (cluster.childClusters[i].shards[j]);
                                cluster.childClusters[i].shards[j].cluster = null;
                                cluster.childClusters[i].shards.RemoveAt (j);
                            }
                        }

                        // Reinit cluster
                        if (startAmount > cluster.childClusters[i].shards.Count)
                            RFShard.ReinitNeibs (cluster.childClusters[i].shards);
                    }
                }
            }
        }

        // Detach one shard with one neib from clusters
        static void DetachOneNeibShards (List<RFCluster> childClusters, List<RFShard> detachShards, int edgeAmount, int startIndex)
        {
            // Check every detached child cluster
            while (edgeAmount >= detachShards.Count)
            {
                // Detach one shard with one neib from every cluster
                int detachAmount = detachShards.Count;
                for (int c = childClusters.Count - 1; c >= startIndex; c--)
                {
                    // Detach one shard with one neib
                    DetachOneNeibShard (childClusters[c], detachShards);

                    // Enough edge shards
                    if (edgeAmount >= detachShards.Count)
                        return;
                }

                // No cluster with shard with one neib. Stop while
                if (detachShards.Count == detachAmount)
                    return;
            }
        }

        // Detach one shard with one neib
        static void DetachOneNeibShard (RFCluster cls, List<RFShard> detachShards)
        {
            if (cls.shards.Count >= 3)
            {
                // Check every shard
                for (int s = cls.shards.Count - 1; s >= 0; s--)
                {
                    // Check amount of neibs
                    if (cls.shards[s].neibShards.Count == 1)
                    {
                        // Collect shard with one neib
                        detachShards.Add (cls.shards[s]);
                        cls.shards[s].cluster = null;
                        
                        // Clean up neib shard
                        for (int i = cls.shards[s].neibShards[0].neibShards.Count - 1; i >= 0; i--)
                            if (cls.shards[s].neibShards[0].neibShards[i].cluster == null)
                                cls.shards[s].neibShards[0].RemoveNeibAt (i);
                            
                        // Remove from cluster
                        cls.shards.RemoveAt (s);
                        
                        // Enough detach shards
                        if (cls.shards.Count <= 2)
                            return;
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Add rigid
        /// /////////////////////////////////////////////////////////
        
        // Add rigid component to transform list
        static void AddRigidComponent (RayfireRigid scr, List<RFShard> shardList)
        {
            // Get parent for connected cluster detached shards 
            Transform tm = RayfireMan.GetParentByManager (scr);
            
            // Add Rigid component to detached shards
            for (int i = 0; i < shardList.Count; i++)
                AddRigidComponent (scr, shardList[i], tm);
        }
        
        // Add rigid component to transform list
        static void AddRigidComponent (RayfireRigid scr, RFShard shard, Transform parent)
        {
            // Set parent
            shard.tm.parent = parent;
            
            // Add new component if shard has not rigid
            if (shard.rigid == null)
            {
                // Add Rigid
                shard.rigid = shard.tm.gameObject.AddComponent<RayfireRigid>();
                
                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (shard.rigid);
                
                // Set custom fragment simulation type if not inherited
                RFDemolitionMesh.SetClusterSimulationType (shard.rigid, scr.simTp);
                
                // Turn off demolition for solo fragments
                if (scr.clsDemol.sDm == false)
                    shard.rigid.dmlTp = DemolitionType.None;
            }
            else
            {
                // Add rigid body to Rigid if it was deleted because of clustering
                if (shard.rigid.physics.rb == null)
                    shard.rigid.physics.rb = shard.rigid.gameObject.AddComponent<Rigidbody>();
                
                // Set density. After collider defined TODO save mass at first apply, reuse now
                RFPhysic.SetDensity (shard.rigid);

                // Set drag properties
                RFPhysic.SetDrag (shard.rigid);
            }
            
            // Skip excluded                                    
            if (shard.rigid.physics.exclude == true)
                return;

            // Collect fragment
            scr.fragments.Add (shard.rigid);
            
            // Set unyielding
            shard.rigid.activation.uny = shard.uny;
            shard.rigid.activation.atb = shard.act;
            
            // Copy particles
            RFPoolingParticles.CopyParticlesRigid (scr, shard.rigid);
            
            // Set to mesh 
            shard.rigid.objTp = ObjectType.Mesh;
            shard.rigid.physics.ct = RFColliderType.Mesh;
            
            // Set dynamic if cluster inherit from kinematic/inactive but has no uny shards
            if (shard.rigid.simTp == SimType.Kinematic || shard.rigid.simTp == SimType.Inactive)
                if (shard.rigid.activation.con == true && shard.uny == false)
                {
                    shard.rigid.simTp = SimType.Dynamic;
                }

            // Do not destroy fragment because cluster could be reused 
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset)
                shard.rigid.reset.action = RFReset.PostDemolitionType.DeactivateToReset;
            
            // Update depth level and amount
            shard.rigid.limitations.currentDepth = scr.limitations.currentDepth + 1;
            
            // Turn on
            shard.rigid.Initialize();

            // Set rb to track later in case of rigidRoot reset with connectivity + clusterize + demolishable
            shard.rb = shard.rigid.physics.rb;
            
            // IMPORTANT. Set mesh collider convex for gun impact detection
            if (shard.rigid.objTp == ObjectType.Mesh)
                if (shard.rigid.physics.mc != null)
                    if (shard.rigid.physics.mc is MeshCollider == true)
                        (shard.rigid.physics.mc as MeshCollider).convex = true;
        }

        /// /////////////////////////////////////////////////////////
        /// RigidRoot shards -> Connected Cluster -> RigidRoot shards
        /// /////////////////////////////////////////////////////////
        
        // Set demolished Connected CLuster shards back to parent RigidRoot
        static void SetRigidRootShard(RayfireRigid scr, List<RFShard> shards)
        {
            for (int i = 0; i < shards.Count; i++)
            {
                // Set parent to rigidroot
                shards[i].tm.parent = scr.rigidRoot.tm;

                // Set as dynamic
                shards[i].sm = SimType.Dynamic;
            } 
                
            // Add RigidBody and set physics properties TODO won't affect shards with Rigid
            RFPhysic.SetPhysics(shards, scr.physics);
            
            // TODO inherit velocity
            // TODO Init demolition particles
            // TODO Init Fade
        }
        
        /// /////////////////////////////////////////////////////////
        /// Get
        /// /////////////////////////////////////////////////////////

        // All shards should be clusterized to one cluster. Stop
        static bool SameClusterCheck(RayfireRigid scr, List<RFShard> detachShards, int shardAmount, int clusterAmount)
        {
            if (shardAmount == detachShards.Count && clusterAmount == 1)
            {
                scr.limitations.demolitionShould = false;
                scr.clsDemol.cluster.shards = detachShards;
                for (int i = 0; i < scr.clsDemol.cluster.shards.Count; i++)
                    scr.clsDemol.cluster.shards[i].cluster = scr.clsDemol.cluster;

                RFPhysic.CollectClusterColliders (scr, scr.clsDemol.cluster);
                return true;
            }

            return false;
        }
        
        // Had child cluster
        public bool HasChildClusters { get { return cluster.childClusters != null && cluster.childClusters.Count > 0; } }
        public bool HasMinorClusters { get { return minorClusters != null && minorClusters.Count > 0; } }
        
        /// /////////////////////////////////////////////////////////
        /// Compatibility
        /// /////////////////////////////////////////////////////////

        public ConnectivityType connectivity {
            get { return cnt; }
            set { cnt = value; } }
        public float minimumArea {
            get { return mAr; }
            set { mAr = value; } }
        public float minimumSize {
            get { return mSz; }
            set { mSz = value; } }
        public int percentage {
            get { return pct; }
            set { pct = value; } }
        public int shardArea {
            get { return sAr; }
            set { sAr = value; } }
        public bool shardDemolition {
            get { return sDm; }
            set { sDm = value; } }
        public int minAmount {
            get { return mnAm; }
            set { mnAm = value; } }
        public int maxAmount {
            get { return mxAm; }
            set { mxAm = value; } }
        public bool demolishable {
            get { return cDm; }
            set { cDm = value; } }
    }
}