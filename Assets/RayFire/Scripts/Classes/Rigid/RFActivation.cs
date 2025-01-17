using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RayFire
{
    /// <summary>
    /// Rayfire Rigid and RigidRoot activation properties class.
    /// </summary>
    [Serializable]
    public class RFActivation
    {
        [FormerlySerializedAs ("byOffset")]       public float               off;
        [FormerlySerializedAs ("local")]          public bool                loc;
        [FormerlySerializedAs ("byVelocity")]     public float               vel;
        [FormerlySerializedAs ("byDamage")]       public float               dmg;
        [FormerlySerializedAs ("byActivator")]    public bool                act;
        [FormerlySerializedAs ("byImpact")]       public bool                imp;
        [FormerlySerializedAs ("byConnectivity")] public bool                con;
        [FormerlySerializedAs ("unyielding")]     public bool                uny;
        [FormerlySerializedAs ("activatable")]    public bool                atb;
        public                                           bool                l;
        [FormerlySerializedAs ("layer")]   public        int                 lay;
        [FormerlySerializedAs ("connect")] public        RayfireConnectivity cnt; // TODO non serialized
        
        // Non Serialized
        [NonSerialized] public int         lb; // Backup Layer
        [NonSerialized] public bool        activated;
        [NonSerialized] public bool        inactiveCorState;
        [NonSerialized] public bool        velocityCorState;
        [NonSerialized] public bool        offsetCorState;

        // Static
        public static float randomRot = 0.3f;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFActivation()
        {
            InitValues();
            LocalReset();
        }

        void InitValues()
        {
            off = 0f;
            loc = false;
            vel = 0f;
            dmg = 0f;
            act = false;
            imp = false;
            con = false;
            uny = false;
            atb = false;
            l   = false;
            lay = 0;
            cnt = null;
            lb  = 0;
        }
        
        // Turn of all activation properties
        public void LocalReset()
        {
            activated        = false;
            inactiveCorState = false;
            velocityCorState = false;
            offsetCorState   = false;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
        }

        // Copy from
        public void CopyFrom (RFActivation source)
        {
            off = source.off;
            loc = source.loc;
            vel = source.vel;
            dmg = source.dmg;
            act = source.act;
            imp = source.imp;
            con = source.con;
            uny = source.uny;
            atb = source.atb;
            l   = source.l;
            lay = source.lay;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Connectivity check
        public void CheckConnectivity()
        {
            if (con == true && cnt != null)
            {
                cnt.connectivityCheckNeed = true;
                cnt = null;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Activation by velocity and offset
        public IEnumerator InactiveCor  (RayfireRigidRoot scr)
        {
            // Stop if running 
            if (inactiveCorState == true)
                yield break;

            // Set running state
            inactiveCorState = true;
            int shardsAmount;
            
            while (scr.inactiveShards.Count > 0)
            {
                // Remove activated shards
                shardsAmount = scr.inactiveShards.Count - 1;
                for (int i = shardsAmount; i >= 0; i--)
                    if (scr.inactiveShards[i].sm == SimType.Dynamic || scr.inactiveShards[i].rb == null)
                        scr.inactiveShards.RemoveAt (i);
                
                // Velocity activation
                if (scr.activation.vel > 0)
                {
                    shardsAmount = scr.inactiveShards.Count - 1;
                    for (int i = shardsAmount; i >= 0; i--)
                    {
                        if (scr.inactiveShards[i].rb.velocity.magnitude > scr.activation.vel)
                            if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                scr.inactiveShards.RemoveAt (i);
                    }

                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                    {
                        inactiveCorState = false;
                        yield break;
                    }
                }

                // Offset activation
                if (scr.activation.off > 0)
                {
                    shardsAmount = scr.inactiveShards.Count - 1;
                    
                    // By global offset
                    if (scr.activation.loc == false)
                    {
                        for (int i = shardsAmount; i >= 0; i--)
                            if (Vector3.Distance (scr.inactiveShards[i].tm.position, scr.inactiveShards[i].pos) > scr.activation.off)
                                if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                    scr.inactiveShards.RemoveAt (i);
                    }
                    
                    // By local offset
                    else
                    {
                        for (int i = shardsAmount; i >= 0; i--)
                            if (Vector3.Distance (scr.inactiveShards[i].tm.localPosition, scr.inactiveShards[i].los) > scr.activation.off)
                                if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                    scr.inactiveShards.RemoveAt (i);
                    }
                    
                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                    {
                        inactiveCorState = false;
                        yield break;
                    }
                }
                // TODO repeat 30 times per second, not every frame
                yield return null;
            }
            
            inactiveCorState = false;
        }

        /// /////////////////////////////////////////////////////////
        /// Activate Rigid / Shard
        /// /////////////////////////////////////////////////////////

        // Activate inactive object
        public static void ActivateRigid (RayfireRigid scr, bool connCheck = true)
        {
            // Stop if excluded
            if (scr.physics.exclude == true)
                return;

            // Skip not activatable unyielding objects
            if (scr.activation.atb == false && scr.activation.uny == true)
                return;

            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();

            // Turn convex if kinematic activation
            if (scr.simTp == SimType.Kinematic)
            {
                MeshCollider meshCollider = scr.physics.mc as MeshCollider;
                if (meshCollider != null)
                    meshCollider.convex = true;

                // Swap with animated object
                if (scr.physics.rec == true)
                {
                    // Set dynamic before copy
                    scr.simTp                = SimType.Dynamic;
                    scr.physics.rb.isKinematic = false;
                    scr.physics.rb.useGravity  = scr.physics.gr;

                    // Create copy
                    GameObject inst = UnityEngine.Object.Instantiate (scr.gameObject);
                    inst.transform.position = scr.tsf.position;
                    inst.transform.rotation = scr.tsf.rotation;

                    // Save velocity
                    Rigidbody rBody = inst.GetComponent<Rigidbody>();
                    if (rBody != null)
                    {
                        rBody.velocity        = scr.physics.rb.velocity;
                        rBody.angularVelocity = scr.physics.rb.angularVelocity;
                    }

                    // Activate and init rigid
                    scr.gameObject.SetActive (false);
                }
            }

            // RigidRoot Connected Cluster TODO fix. Initiate connectivity check recursively
            // RigidRootClusterActivation (scr);
            
            // Connectivity check
            if (connCheck == true)
                scr.activation.CheckConnectivity();
            
            // Set layer
            SetActivationLayer (scr);
            
            // Set state
            scr.activation.activated = true;

            // Set props
            scr.simTp                  = SimType.Dynamic;
            scr.physics.rb.isKinematic = false; // TODO error at manual activation of stressed connectivity structure
            scr.physics.rb.useGravity  = scr.physics.gr;
            
            // Disable drag
            RFPhysic.SetDrag (scr);

            // Fade on activation
            if (scr.fading.onActivation == true) 
                scr.Fade();

            // Parent
            if (RayfireMan.inst.parent != null)
                scr.gameObject.transform.parent = RayfireMan.inst.parent.transform;
            
            // Init particles on activation
            RFPoolingEmitter.SetHostRigidAct (scr);

            // Activation sound
            RFSound.ActivationSound (scr.sound, scr.limitations.bboxSize);

            // Events
            RFActivationEvent.RigidActivationEvent (scr);
            
            // Add initial rotation if still
            ActivationRandomRotation (scr.physics.rb);
        }

        // Activate Rigid Root shard
        public static bool ActivateShard (RFShard shard, RayfireRigidRoot rigidRoot)
        {
            // Skip not activatable unyielding shards
            if (shard.act == false && shard.uny == true)
                return false;
            
            // Set dynamic sim state
            shard.sm = SimType.Dynamic;
            
            // Activate by Rigid if has rigid
            if (shard.rigid != null)
            {
                if (shard.rigid.objTp == ObjectType.Mesh)
                {
                    ActivateRigid (shard.rigid);
                    return true;
                }

                if (shard.rigid.objTp == ObjectType.ConnectedCluster)
                {
                    if (shard.rigid.activation.activated == false)
                    {
                        ActivateRigid (shard.rigid);
                        return true;
                    }
                }
            }

            // Physics ops
            if (shard.rb != null)
            {
                // Set props
                if (shard.rb.isKinematic == true)
                    shard.rb.isKinematic = false;

                // Turn On Gravity
                shard.rb.useGravity = rigidRoot.physics.gr;
                
                // Disable drag
                RFPhysic.SetDrag (shard, RayfireMan.inst.materialPresets.Drag (rigidRoot.physics.mt), RayfireMan.inst.materialPresets.AngularDrag (rigidRoot.physics.mt));
                
                // Add initial rotation if still
                ActivationRandomRotation (shard.rb);
            }
            
            // Set activation layer
            SetActivationLayer (shard, rigidRoot.activation);

            // Activation Fade TODO input Fade class by RigidRoot or MeshRoot
            if (rigidRoot.fading.onActivation == true)
                RFFade.FadeShard (rigidRoot, shard);

            // Parent
            if (RayfireMan.inst.parent != null)
                shard.tm.parent = RayfireMan.inst.parent.transform;

            // Connectivity check if shards was activated: TODO check only neibs of activated?
            if (rigidRoot.activation.con == true && rigidRoot.activation.cnt != null)
            {
                rigidRoot.activation.cnt.connectivityCheckNeed = true;
            }

            // Init particles on activation
            RFPoolingEmitter.SetHostRigidrootShardAct(rigidRoot, shard);
            
            // Activation sound
            RFSound.ActivationSound (rigidRoot.sound, rigidRoot.cluster.bound.size.magnitude);
            
            // Events
            RFActivationEvent.ShardActivationEvent (shard, rigidRoot);
            
            return true;
        }

        // Add initial rotation if still
        public static void ActivationRandomRotation(Rigidbody rb)
        {
            if (rb.angularVelocity == Vector3.zero)
                rb.angularVelocity = new Vector3 (
                    Random.Range (-randomRot, randomRot), Random.Range (-randomRot, randomRot), 
                    Random.Range (-randomRot, randomRot));
        }

        // Init connectivity check of parent rigidroot at cluster activation TODO change rroot shards om other way
        static void RigidRootClusterActivation(RayfireRigid scr)
        {
            if (scr.objTp == ObjectType.ConnectedCluster && scr.rigidRoot != null)
            {
                for (int i = 0; i < scr.clsDemol.cluster.shards.Count; i++)
                    scr.clsDemol.cluster.shards[i].sm = SimType.Dynamic;
                scr.rigidRoot.activation.cnt.CheckConnectivity();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Activation Layer
        /// /////////////////////////////////////////////////////////
        
        // Set activation layer
        static void SetActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.gameObject.layer = scr.activation.lay;
        }
        
        // ReSet activation layer
        public static void RestoreActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.gameObject.layer = scr.activation.lb;
        }
        
        // Backup original layer in case rigid will change layer after activation
        public static void BackupActivationLayer (RayfireRigid scr)
        {
            if (scr.activation.l == true)
                scr.activation.lb = scr.gameObject.layer;
        }
        
        /// /////////////////////////////////////////////////////////
        /// RigidRoot Activation Layer
        /// /////////////////////////////////////////////////////////
                
        // Set activation layer
        static void SetActivationLayer (RFShard shard, RFActivation activation)
        {
            if (activation.l == true)
                shard.tm.gameObject.layer = activation.lay;
        }
        
        // Set activation layer
        public static void SetActivationLayer (List<RFShard> shards, RFActivation activation, Transform root)
        {
            if (activation.l == true)
            {
                // Set to shards
                for (int s = 0; s < shards.Count; s++)
                    shards[s].tm.gameObject.layer = activation.lay;

                // Set to root as well. IMPORTANT: Runtime demolition doesnt work if shards and root has different layers
                if (root != null)
                    root.gameObject.layer = activation.lay;
            }
        }

        // ReSet layer for activated shards
        public static void RestoreActivationLayer (RayfireRigidRoot root)
        {
            if (root.activation.l == true)
                for (int i = 0; i < root.cluster.shards.Count; i++) 
                    root.cluster.shards[i].tm.gameObject.layer = root.cluster.shards[i].lb;
        }
        
        // Backup original layer in case shard will change layer after activation
        public static void BackupActivationLayer (RayfireRigidRoot root)
        {
            if (root.activation.l == true)
                for (int i = 0; i < root.cluster.shards.Count; i++)
                    root.cluster.shards[i].lb = root.cluster.shards[i].tm.gameObject.layer;
        }
    }
}