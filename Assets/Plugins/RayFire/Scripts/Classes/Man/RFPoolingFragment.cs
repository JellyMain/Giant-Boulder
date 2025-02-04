using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace RayFire
{
    /// <summary>
    /// Rayfire Man fragment pooling class.
    /// </summary>
    [Serializable]
    public class RFPoolingFragment
    {
        // UI
        public                                     bool enable;
        public                                     bool reuse;
        [FormerlySerializedAs ("capacity")] public int  minCap;
        public                                     int  maxCap;

        // Non serialized
        [NonSerialized]        Transform           root;
        [NonSerialized]        GameObject          host;
        [NonSerialized]        MeshFilter          mf;
        [NonSerialized]        MeshRenderer        mr;
        [NonSerialized]        RayfireRigid        rg;
        [NonSerialized]        Rigidbody           rb;
        [NonSerialized] public RayfireRigid        rgInst;
        [NonSerialized] public Queue<RayfireRigid> queue;
        [NonSerialized] public bool                inProgress;

        // Static
        public static int rate = 2;

        // Constructor
        public RFPoolingFragment()
        {
            enable = true;
            minCap = 60;
            reuse  = false;
            maxCap = 120;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Create pool root
        public void CreatePoolRoot (Transform manTm)
        {
            // Already has pool root
            if (root != null)
                return;
            
            GameObject poolGo = new GameObject ("Pool_Fragments");
            root          = poolGo.transform;
            root.position = manTm.position;
            root.parent   = manTm;
        }

        // Create pool object
        public void CreateInstance (Transform manTm)
        {
            // Return if not null
            if (rgInst != null)
                return;

            // Create pool instance
            rgInst = CreateRigidInstance();

            // Set tm
            rgInst.tsf.SetPositionAndRotation (manTm.position, manTm.rotation);
            rgInst.tsf.parent   = root;
        }

        // Create pool object
        public RayfireRigid CreateRigidInstance()
        {
            host = new GameObject ("rg");
            host.SetActive (false);
            
            mf                        = host.AddComponent<MeshFilter>();
            mr                        = host.AddComponent<MeshRenderer>();
            rg                        = host.AddComponent<RayfireRigid>();
            rb                        = host.AddComponent<Rigidbody>();
            rb.interpolation          = RayfireMan.inst.interpolation;
            rb.collisionDetectionMode = RayfireMan.inst.meshCollision;
            rg.initialization         = RayfireRigid.InitType.AtStart;
            rg.tsf              = host.transform;
            rg.mFlt             = mf;
            rg.mRnd           = mr;
            rg.physics.rb      = rb;

            return rg;
        }

        // Get pool object
        public RayfireRigid GetPoolObject ()
        {
            if (enable == true)
            {
                while (queue.Count > 0)
                {
                    // Check if destroyed with demolished cluster
                    if (queue.Peek() == null)
                        queue.Dequeue();
                    else
                        return queue.Dequeue();
                }
            }

            return CreatePoolObject (RayfireMan.inst.transForm);
        }

        // Create pool object
        RayfireRigid CreatePoolObject (Transform manTm)
        {
            // Create instance if null
            if (rgInst == null)
                CreateInstance (manTm);

            // Create
            return Object.Instantiate (rgInst, root);
        }

        // Destroy Rigid or reset back to pool
        public void DestroyOrReset(RayfireRigid rgBack, float lifeTime)
        {
            // Destroy if backpooling disabled or max capacity reached
            if (reuse == false || queue.Count > maxCap)
            {
                if (lifeTime <= 0)
                    Object.Destroy (rgBack.gameObject, lifeTime);
                else
                    Object.Destroy (rgBack.gameObject);
            }

            // Add to backpooling
            else
            {
                RigidPoolReset (rgBack);
            }
        }
        
        // Keep full pool 
        public IEnumerator StartPoolingCor (Transform manTm)
        {
            const float delayTime = 0.5f;
            queue = new Queue<RayfireRigid>(minCap);
            WaitForSeconds delay = new WaitForSeconds (delayTime);
            
            // Create some in advance for quick test demolitions
            for (int i = 0; i < 30; i++)
                if (queue.Count < minCap)
                    queue.Enqueue (CreatePoolObject (manTm));
            
            // Pooling loop
            inProgress = true;
            while (enable == true)
            {
                // Create if not enough
                if (queue.Count < minCap)
                    for (int i = 0; i < rate; i++)
                        queue.Enqueue (CreatePoolObject (manTm));

                // Wait next frame
                yield return delay;
            }
            inProgress = false;
        }
        
        // Reset Rigid for pooling
        void RigidPoolReset(RayfireRigid rgBack)
        {
            // Set tm
            rgBack.tsf.parent        = root;
            rgBack.tsf.localPosition = Vector3.zero;
            rgBack.tsf.localRotation = Quaternion.identity;
            rgBack.tsf.localScale    = Vector3.one;

            // Reset properties
            GlobalReset (rgBack);
            
            // Add back to queue
            queue.Enqueue (rgBack);
        }
        
        // Reset Rigid back to pool
        public static void GlobalReset(RayfireRigid scr)
        {
            scr.initialization = RayfireRigid.InitType.ByMethod;
            scr.simTp = SimType.Dynamic;
            scr.objTp = ObjectType.Mesh;
            scr.dmlTp = DemolitionType.None;
            
            scr.physics.GlobalReset(); // TODO reset rigidbody and mesh collider props
            scr.activation.GlobalReset();
            scr.limitations.GlobalReset();; // TODO bound and contact point
            scr.mshDemol.GlobalReset();
            scr.clsDemol.GlobalReset();
            scr.refDemol.GlobalReset();
            scr.materials.GlobalReset();
            scr.damage.GlobalReset();
            scr.fading.GlobalReset();
            scr.reset.GlobalReset();
            
            // Hidden
            scr.initialized = false;
            scr.fragments   = null;
            scr.chRot       = Quaternion.identity;
            scr.rtC         = null;
            scr.rtP         = null;
            scr.rest        = null;
            scr.sound       = null;
            
            // Non Serialized
            scr.corState     = false;
            scr.particleList = null;
            scr.debrisList   = null;
            scr.dustList     = null;
            scr.subIds       = null;
            scr.pivots       = null;
            scr.meshes       = null;
            scr.meshRoot     = null;
            scr.rigidRoot    = null;
            
            // Reset components
            if (scr.mFlt != null)
                scr.mFlt.sharedMesh = null;
            else
                scr.mFlt = scr.gameObject.AddComponent<MeshFilter>();
            if (scr.mRnd != null)
            {
                scr.mRnd.sharedMaterial  = null;
                scr.mRnd.sharedMaterials = new Material[] {null}; // TODO reset other properties
            }
            else
                scr.mRnd = scr.gameObject.AddComponent<MeshRenderer>();
            
            scr.skr                          = null;

            // TODO Reset
            /*
            scr.demolitionEvent  = new RFDemolitionEvent();
            scr.activationEvent  = new RFActivationEvent();
            scr.restrictionEvent = new RFRestrictionEvent();
            */
        }
        
    }
}
