using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFReferenceDemolition
    {
        public enum ActionType
        {
            Instantiate = 0,
            SetActive   = 1
        }

        // UI
        [FormerlySerializedAs ("ref")] [FormerlySerializedAs ("reference")] public GameObject       rfs;
        [FormerlySerializedAs ("randomList")]                               public List<GameObject> rnd;
        [FormerlySerializedAs ("action")]                                   public ActionType       act;
        [FormerlySerializedAs ("addRigid")]                                 public bool             add;
        [FormerlySerializedAs ("inheritScale")]                             public bool             scl;
        [FormerlySerializedAs ("inheritMaterials")]                         public bool             mat;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFReferenceDemolition()
        {
            InitValues();
        }
        
        void InitValues()
        {
            rfs        = null;
            rnd       = null;
            add         = true;
            scl     = true;
            mat = false;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
        }
        
        // Copy from
        public void CopyFrom (RFReferenceDemolition source)
        {
            rfs        = source.rfs;
            rnd       = source.rnd;
            add         = source.add;
            scl     = source.scl;
            mat = source.mat;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////   
        
        public bool HasRandomRefs { get { return rnd != null && rnd.Count > 0; } }
        
        // Get reference
        public GameObject GetReference()
        {
            // Return reference if action type is SetActive
            if (act == ActionType.SetActive)
            {
                // Return single ref
                if (rfs != null && HasRandomRefs == false)
                    return rfs;
                
                // Get random ref
                if (HasRandomRefs == true)
                    return rnd[Random.Range (0, rnd.Count)];
                
                // Reference not defined or destroyed
                if (rfs == null)
                    return null;
                
                // Reference is prefab asset
                if (rfs.scene.rootCount == 0)
                    return null;
                
                return rfs;
            }

            if (act == ActionType.Instantiate)
            {
                // Get random ref
                if (HasRandomRefs == true)
                    return rnd[Random.Range (0, rnd.Count)];
                
                // Return single ref
                if (rfs != null && HasRandomRefs == false)
                    return rfs;
            }
            
            return null;
        }
        
        // Demolish object to reference
        public static bool DemolishReference (RayfireRigid scr)
        {
            if (scr.dmlTp == DemolitionType.ReferenceDemolition)
            {
                // Demolished
                scr.limitations.demolished = true;
                
                // Turn off original
                scr.gameObject.SetActive (false);
                
                // Get reference
                GameObject refGo = scr.refDemol.GetReference();

                // Has no reference
                if (refGo == null)
                    return true;

                // Check if reference has already initialized Rigid
                RayfireRigid refScr = refGo.gameObject.GetComponent<RayfireRigid>();
                if (refScr != null && refScr.initialized == true)
                {
                    RayfireMan.Log (RFLimitations.rigidStr + scr.name + " Demolition Reference object has already initialized Rigid. Set By Method Initialization type or Deactivate reference.", scr.gameObject);
                    return true;
                }
                
                // Set object to swap
                GameObject instGo = GetInstance (scr, refGo);

                // Set root to manager or to the same parent
                RayfireMan.SetParentByManager (instGo.transform, scr.tsf);
                
                // Set tm
                scr.rtC = instGo.transform;
                
                // Copy scale
                if (scr.refDemol.scl == true)
                    scr.rtC.localScale = scr.tsf.localScale;

                // Inherit materials
                InheritMaterials (scr, instGo);

                // Clear list for fragments
                scr.fragments = new List<RayfireRigid>();
                
                // Check root for rigid props
                RayfireRigid instScr = instGo.gameObject.GetComponent<RayfireRigid>();

                // Reference Root has not rigid. Add to
                if (instScr == null && scr.refDemol.add == true)
                {
                    // Add rigid and copy
                    instScr = instGo.gameObject.AddComponent<RayfireRigid>();

                    // Copy rigid
                    scr.CopyPropertiesTo (instScr);

                    // Set fragments sim type
                    RFDemolitionMesh.SetFragmentSimulationType (instScr, scr.simTp);
                    
                    // Copy particles from demolished rigid to instanced rigid
                    RFPoolingParticles.CopyParticlesRigid (scr, instScr);   
                    
                    // Single mesh
                    if (instGo.transform.childCount == 0)
                    {
                        instScr.objTp = ObjectType.Mesh;
                    }

                    // Multiple meshes
                    if (instGo.transform.childCount > 0)
                    {
                        instScr.objTp = ObjectType.MeshRoot;
                    }
                }

                // Activate and init rigid
                instGo.transform.gameObject.SetActive (true);

                // Reference has rigid
                if (instScr != null)
                {
                    // Init if not initialized yet
                    instScr.Initialize();
                    
                    // Create rigid for root children
                    if (instScr.objTp == ObjectType.MeshRoot)
                    {
                        // Collect referenced fragments
                        scr.fragments.AddRange (instScr.fragments);
                    }

                    // Get ref rigid
                    else if (instScr.objTp == ObjectType.Mesh || instScr.objTp == ObjectType.SkinnedMesh)
                    {
                        // Disable runtime caching
                        instScr.mshDemol.ch.tp = CachingType.Disabled;
                        
                        // Instance has no meshes
                        if (instScr.mFlt == null && instScr.skr == null)
                            return true;

                        // Demolish mesh instance
                        RFDemolitionMesh.DemolishMesh(instScr);
                        
                        // Collect fragments
                        if (instScr.HasFragments == true)
                            scr.fragments.AddRange (instScr.fragments);
                        
                        // Destroy instance
                        RayfireMan.DestroyFragment (instScr, instScr.rtP, 1f);
                    }

                    // Get ref rigid
                    else if (instScr.objTp == ObjectType.NestedCluster || instScr.objTp == ObjectType.ConnectedCluster)
                    {
                        instScr.Default();
                        
                        // Copy contact data
                        instScr.limitations.contactPoint   = scr.limitations.contactPoint;
                        instScr.limitations.contactVector3 = scr.limitations.contactVector3;
                        instScr.limitations.contactNormal  = scr.limitations.contactNormal;
                        
                        // Demolish
                        RFDemolitionCluster.DemolishCluster (instScr);
                        
                        // Collect new fragments
                        scr.fragments.AddRange (instScr.fragments);
                        
                        // Collect demolished cluster
                        if (instScr.clsDemol.cluster.shards.Count > 0)
                            scr.fragments.Add (instScr);
                    }
                }

                else
                {
                    Rigidbody rb = instGo.GetComponent<Rigidbody>();
                    if (rb != null && scr.physics.rb != null)
                    {
                        rb.velocity        = scr.physics.rb.velocity;
                        rb.angularVelocity = scr.physics.rb.angularVelocity;
                    }
                }
            }

            return true;
        }

        // Get final instance accordingly to action type
        static GameObject GetInstance (RayfireRigid scr, GameObject refGo)
        {
            GameObject instGo;
            
            // Instantiate turned off reference with null parent
            if (scr.refDemol.act == ActionType.Instantiate)
            {
                instGo = Object.Instantiate (refGo, scr.tsf.position, scr.tsf.rotation);
                instGo.name = refGo.name;
            }
                
            // Set active
            else
            {
                instGo                    = refGo;
                instGo.transform.position = scr.tsf.position;
                instGo.transform.rotation = scr.tsf.rotation;
            }
            
            return instGo;
        }

        // Inherit materials from original object to referenced fragments
        static void InheritMaterials (RayfireRigid scr, GameObject instGo)
        {
            if (scr.refDemol.mat == true)
            {
                Renderer[] renderers = instGo.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        int min = Math.Min (scr.mRnd.sharedMaterials.Length, renderers[r].sharedMaterials.Length);
                        for (int m = 0; m < min; m++)
                            renderers[r].sharedMaterials[m] = scr.mRnd.sharedMaterials[m];
                    }
            }
        }
    }
}