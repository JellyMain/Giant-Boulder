using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
using RayFire.DotNet;
#endif

namespace RayFire
{
    [Serializable]
    public class RFDemolitionMesh
    {
        public enum MeshInputType
        {
            AtStart          = 3,
            AtInitialization = 6,
            AtDemolition     = 9
        }
        
        public enum ConvertType
        {
            Disabled         = 0,
            ConnectedCluster = 2,
            Connectivity     = 4
        }

        [FormerlySerializedAs ("amount")]      public    int                  am;
        [FormerlySerializedAs ("variation")]   public    int                  var;
        [FormerlySerializedAs ("depthFade")]   public    float                dpf;
        [FormerlySerializedAs ("contactBias")] public    float                bias;
        [FormerlySerializedAs ("seed")]        public    int                  sd;
        [FormerlySerializedAs ("useShatter")]  public    bool                 use;
        [FormerlySerializedAs ("addChildren")] public    bool                 cld;
        [FormerlySerializedAs ("simType")]     public    FragSimType          sim;
        public                                           ConvertType          cnv;
        [FormerlySerializedAs ("meshInput")]      public MeshInputType        inp;
        [FormerlySerializedAs ("properties")]     public RFFragmentProperties prp;
        [FormerlySerializedAs ("runtimeCaching")] public RFRuntimeCaching     ch;
        [FormerlySerializedAs ("scrShatter")]     public RayfireShatter       sht;
        
        // Non serialized
        [NonSerialized] public int       badMesh;
        [NonSerialized] public int       shatterMode;
        [NonSerialized] public int       totalAmount;
        [NonSerialized] public int       innerSubId;
        [NonSerialized] public Mesh      mesh;
        [NonSerialized] public RFShatter rfShatter;
        
        // Hidden
        [HideInInspector] public Quaternion rotStart;
        
        static string fragmentStr = "_fr_";

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDemolitionMesh()
        {
            InitValues();
            LocalReset();
            prp = new RFFragmentProperties();
            ch  = new RFRuntimeCaching();
        }
        
        // Starting values
        void InitValues()
        {
            am          = 15;
            var         = 0;
            dpf         = 0.5f;
            bias        = 0f;
            sd          = 1;
            use         = false;
            cld         = true;
            cnv         = 0;
            sim         = FragSimType.Dynamic;
            inp         = MeshInputType.AtDemolition;
            sht         = null;
            shatterMode = 1;
            innerSubId  = 0;
            rotStart    = Quaternion.identity;
            mesh        = null;
            rfShatter   = null;
        }

        // Reset
        public void LocalReset()
        {
            badMesh     = 0;
            totalAmount = 0;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
            
            prp.InitValues();
            ch.InitValues();
        }
        
        // Copy from
        public void CopyFrom (RFDemolitionMesh source)
        {
            am   = source.am;
            var  = source.var;
            dpf  = source.dpf;
            sd   = source.sd;
            bias = source.bias;
            use  = false;
            cld  = source.cld;
            cnv  = source.cnv;
            sim  = source.sim;
            inp  = source.inp;
            inp  = MeshInputType.AtDemolition;

            prp.CopyFrom (source.prp);
            ch = new RFRuntimeCaching();

            LocalReset();

            shatterMode = 1;
            innerSubId  = 0;
            rotStart    = Quaternion.identity;

            mesh      = null;
            rfShatter = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Demolish single mesh to fragments
        public static bool DemolishMesh (RayfireRigid scr)
        {
            // Object demolition
            if (scr.objTp != ObjectType.Mesh && scr.objTp != ObjectType.SkinnedMesh)
                return true;

            // Skip if reference
            if (scr.dmlTp == DemolitionType.ReferenceDemolition)
                return true;

            // Already has fragments
            if (scr.HasFragments == true)
            {
                // Set tm 
                scr.rtC.position = scr.tsf.position;
                scr.rtC.rotation = scr.tsf.rotation;

                // Set parent
                RayfireMan.SetParentByManager (scr.rtC.transform);

                // Activate root and fragments
                scr.rtC.gameObject.SetActive (true);

                // Set demolished state
                scr.limitations.demolished = true;
                
                // Skip coroutines start if Awake prefragment and Convert
                if (scr.dmlTp == DemolitionType.AwakePrefragment && scr.mshDemol.cnv != ConvertType.Disabled)
                    return true;

                // Start all coroutines
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].StartAllCoroutines();

                return true;
            }
            
            // Has unity meshes - create fragments
            if (scr.HasMeshes == true)
            {
                scr.fragments              = CreateFragments (scr);
                scr.limitations.demolished = true;
                return true;
            }

            // Still has no Unity meshes - cache Unity meshes
            if (scr.HasMeshes == false)
            {
                // Cache unity meshes
                CacheRuntime (scr);

                // Caching in progress. Stop demolition
                if (scr.mshDemol.ch.inProgress == true)
                    return false;
                
                // Fragmentation on not supported platforms. approve and set dml to none
                if (scr.meshes == null)
                {
                    scr.limitations.demolished = false;
                    return true;
                }

                // Has unity meshes - create fragments
                if (scr.HasMeshes == true)
                {
                    scr.fragments              = CreateFragments (scr);
                    scr.limitations.demolished = true;
                    return true;
                }
            }

            return false;
        }

        // Create fragments by mesh and pivots array
        static List<RayfireRigid> CreateFragments (RayfireRigid scr)
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>(scr.meshes.Length);

            // Stop if has no any meshes
            if (scr.meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (scr);
            
            // Name 
            string baseName  = scr.gameObject.name + fragmentStr;
            
            // Set rotation to precache rotation
            if (scr.dmlTp == DemolitionType.AwakePrecache)
                scr.rtC.transform.rotation = scr.chRot;
            
            // Create fragment objects
            MeshesToObjects (scr, scrArray, baseName);
            
            // Fix transform for precached fragments
            if (scr.dmlTp == DemolitionType.AwakePrecache)
                scr.rtC.rotation = scr.tsf.rotation;

            // Fix runtime caching rotation difference. Get rotation difference and add to root
            if (scr.dmlTp == DemolitionType.Runtime && scr.mshDemol.ch.tp != CachingType.Disabled)
            {
                Quaternion cacheRotationDif = scr.tsf.rotation * Quaternion.Inverse (scr.mshDemol.rotStart);
                scr.rtC.rotation = cacheRotationDif * scr.rtC.rotation;
            }
            
            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (scr.skr == null)
                scr.rtC.localScale = Vector3.one;
            
            // Set root to manager
            RayfireMan.SetParentByManager (scr.rtC);
            
            // Ignore neib collisions
            RFPhysic.SetIgnoreColliders (scr.physics, scrArray);
            
            return scrArray;
        }
        
        // SLice mesh
        public static bool SliceMesh(RayfireRigid scr)
        {
            // Empty lists
            scr.DeleteCache();
            scr.DeleteFragments();
    
            // SLice
            RFFragment.SliceMeshes (ref scr.meshes, ref scr.pivots, ref scr.subIds, scr, scr.limitations.slicePlanes);
            
            // TODO check if has slicePlanes
            
            // Remove plane info 
            Plane forcePlane = new Plane (scr.limitations.slicePlanes[1], scr.limitations.slicePlanes[0]);
            scr.limitations.slicePlanes.Clear();
            
            // Stop
            if (scr.HasMeshes == false)
                return false;

            // Get fragments
            scr.fragments = CreateSlices(scr);

            // Check for sliced inactive/kinematic with unyielding
            RayfireUnyielding.SetUnyieldingFragments (scr, true);

            // TODO check for fragments
            
            // Set demolition 
            scr.limitations.demolished = true;
            
            // Fragments initialisation
            scr.InitMeshFragments();
            
            // Add force
            if (scr.limitations.sliceForce != 0)
            {
                foreach (var frag in scr.fragments)
                {
                    // Skip inactive fragments
                    if (scr.limitations.affectInactive == false && frag.simTp == SimType.Inactive)
                        continue;
                    
                    // Apply force
                    Vector3 closestPoint = forcePlane.ClosestPointOnPlane (frag.transform.position);
                    Vector3 normalVector = (frag.tsf.position - closestPoint).normalized;
                    frag.physics.rb.AddForce (normalVector * scr.limitations.sliceForce, ForceMode.VelocityChange);

                    /* TODO force to spin fragments based on blades direction
                    normalVector = new Vector3 (-1, 0, 0);
                    frag.physics.rigidBody.AddForceAtPosition (normalVector * scr.limitations.sliceForce, closestPoint, ForceMode.VelocityChange);
                    */
                }
            }

            return true;
        }
        
        // Create slices by mesh and pivots array
        static List<RayfireRigid> CreateSlices (RayfireRigid scr)
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>(scr.meshes.Length);

            // Stop if has no any meshes
            if (scr.meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (scr);
            
            // Name 
            string baseName  = scr.gameObject.name + "_";
            
            // Create fragment objects
            MeshesToObjects (scr, scrArray, baseName);
            
            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (scr.skr == null)
                scr.rtC.localScale = Vector3.one;
            
            // Set root to manager
            RayfireMan.SetParentByManager (scr.rtC);
            
            // Empty lists
            scr.DeleteCache();

            return scrArray;
        }
        
        // Create objects for meshes
        static void MeshesToObjects(RayfireRigid scr, List<RayfireRigid> scrArray, string baseName)
        {
            // Tag and layer
            int    baseLayer = RFFragmentProperties.GetLayer(scr);
            string baseTag   = RFFragmentProperties.GetTag(scr);

            // Set tag and layer to root. IMPORTANT. Some overlaps uses mask by root layer
            scr.rtC.gameObject.layer = baseLayer;
            scr.rtC.gameObject.tag   = baseTag;
            
            // Get original mats
            Material[] mats = scr.skr != null
                ? scr.skr.sharedMaterials
                : scr.mRnd.sharedMaterials;
            
            // Create fragment objects
            for (int i = 0; i < scr.meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst.fragments.rgInst == null
                    ? RayfireMan.inst.fragments.CreateRigidInstance()
                    : RayfireMan.inst.fragments.GetPoolObject();

                // Setup
                rfScr.tsf.position     = scr.tsf.position + scr.pivots[i];
                rfScr.tsf.parent       = scr.rtC;
                rfScr.name             = baseName + i;
                rfScr.gameObject.tag   = baseTag;
                rfScr.gameObject.layer = baseLayer;
                rfScr.mFlt.sharedMesh  = scr.meshes[i];
                rfScr.rtP              = scr.rtC;
                
                // Copy properties from parent to fragment node
                scr.CopyPropertiesTo (rfScr);

                // Set custom fragment simulation type if not inherited
                SetFragmentSimulationType (rfScr, scr.simTp);
                
                // Copy particles
                RFPoolingParticles.CopyParticlesRigid (scr, rfScr);
                
                // Set collider
                RFPhysic.SetFragmentCollider (rfScr, scr.meshes[i]);
                
                // Copy Renderer properties
                CopyRenderer (scr, rfScr.mRnd, scr.meshes[i].bounds);
                
                // Turn on
                rfScr.gameObject.SetActive (true);

                // Update depth level and amount
                rfScr.limitations.currentDepth = scr.limitations.currentDepth + 1;
                rfScr.mshDemol.am    = (int)(rfScr.mshDemol.am * rfScr.mshDemol.dpf);
                if (rfScr.mshDemol.am < 3)
                    rfScr.mshDemol.am = 3;
                
                // Disable outer mat for depth fragments
                if (rfScr.limitations.currentDepth >= 1)
                    rfScr.materials.oMat = null;

                // Set multymaterial
                RFSurface.SetMaterial (scr.subIds, mats, scr.materials, rfScr.mRnd, i, scr.meshes.Length);
                    
                // Set mass by mass value accordingly to parent
                if (rfScr.physics.mb == MassType.MassProperty)
                    RFPhysic.SetMassByParent (rfScr.physics, rfScr.limitations.bboxSize, scr.physics.ms, scr.limitations.bboxSize);
                
                // Add in array
                scrArray.Add (rfScr);
            }
        }

        // Set custom fragment simulation type if not inherited
        public static void SetFragmentSimulationType (RayfireRigid frag, SimType sim)
        {
            frag.simTp = sim;
            if (frag.mshDemol.sim != FragSimType.Inherit)
                frag.simTp = (SimType)frag.mshDemol.sim;
        }
        
        // Set custom fragment simulation type if not inherited
        public static void SetClusterSimulationType (RayfireRigid frag, SimType sim)
        {
            frag.simTp = sim;
            if (frag.clsDemol.sim != FragSimType.Inherit)
                frag.simTp = (SimType)frag.clsDemol.sim;
        }
                
        // Copy mesh renderer properties
        static void CopyRenderer (RayfireRigid scr, MeshRenderer trg, Bounds bounds)
        {
            // Shadow casting
            if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > bounds.size.magnitude)
                trg.shadowCastingMode = ShadowCastingMode.Off;
            
            /*
            trg.receiveGI = scr.meshRenderer.receiveGI;
            trg.rayTracingMode            = scr.meshRenderer.rayTracingMode;
            trg.lightProbeUsage           = scr.meshRenderer.lightProbeUsage;
            trg.reflectionProbeUsage      = scr.meshRenderer.reflectionProbeUsage;
            trg.allowOcclusionWhenDynamic = scr.meshRenderer.allowOcclusionWhenDynamic;
            */
        }

        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Start cache fragment meshes. Instant or runtime
        static void CacheRuntime (RayfireRigid scr)
        {
            // Reuse existing cache
            if (scr.reset.action == RFReset.PostDemolitionType.DeactivateToReset && 
                scr.reset.mesh == RFReset.MeshResetType.ReuseFragmentMeshes)
                if (scr.HasMeshes == true)
                    return;

            // Clear all mesh data
            scr.DeleteCache();

            // Cache meshes
            if (scr.mshDemol.ch.tp == CachingType.Disabled)
                CacheInstant(scr);
            else
                scr.CacheFrames();
        }
        
        // Instant caching into meshes
        static void CacheInstant (RayfireRigid scr)
        {
            // Input mesh, setup
            if (RFFragment.InputMesh (scr) == false)
                return;

            // Create fragments
            RFFragment.CacheMeshesInst (ref scr.meshes, ref scr.pivots, ref scr.subIds, scr);
        }       
        
        /// /////////////////////////////////////////////////////////
        /// Precache and Prefragment
        /// /////////////////////////////////////////////////////////  
        
        // Precache meshes at awake
        public static void Awake(RayfireRigid scr)
        {
            // Not mesh
            if (scr.objTp != ObjectType.Mesh)
                return;
                
            // Precache
            if (scr.dmlTp == DemolitionType.AwakePrecache)
                PreCache(scr);

            // Precache and prefragment
            if (scr.dmlTp == DemolitionType.AwakePrefragment)
            {
                PreCache(scr);
                Prefragment(scr);
            }
        }

        // PreCache meshes
        static void PreCache(RayfireRigid scr)
        {
            // Save and disable bias
            float bias = scr.mshDemol.bias;
            scr.mshDemol.bias = 0;
                
            // Cache frag meshes
            CacheInstant (scr);
                
            // Restore bias
            scr.mshDemol.bias = bias;
        }
        
        // Predefine fragments
        static void Prefragment(RayfireRigid scr)
        {
            // Delete existing
            scr.DeleteFragments();

            // Create fragments from cache
            scr.fragments = CreateFragments(scr);
                
            // Stop
            if (scr.HasFragments == false)
            {
                scr.dmlTp = DemolitionType.None;
                return;
            }
            
            // Set physics properties
            for (int i = 0; i < scr.fragments.Count; i++)
            {
                scr.fragments[i].SetComponentsBasic();
                scr.fragments[i].SetComponentsPhysics();
                scr.fragments[i].SetObjectType();

                // Increment demolition depth. Disable if last
                scr.fragments[i].limitations.currentDepth = 1;
                if (scr.limitations.depth == 1)
                    scr.fragments[i].dmlTp = DemolitionType.None;
            }
            
            // Copy Uny state to fragments in case object has Uny components
            RayfireUnyielding.SetUnyieldingFragments(scr, false);
            
            // Clusterize awake fragments
            SetupRuntimeConnectedCluster (scr, true);
            
            // Deactivate fragments root
            if (scr.rtC != null)
                scr.rtC.gameObject.SetActive (false);
            
            // Setup awake connectivity
            SetupRuntimeConnectivity(scr, true);
        }
        
        // Clusterize runtime fragments
        public static bool SetupRuntimeConnectedCluster (RayfireRigid rigid, bool awake)
        {
            // Clusterize disabled
            if (rigid.mshDemol.cnv != ConvertType.ConnectedCluster)
                return false;
            
            // Skip if Runtime init and fragments already clusterized in awake.
            if (rigid.dmlTp == DemolitionType.AwakePrefragment && rigid.limitations.demolished == true)
                return false;
            
            // Not mesh demolition
            if (rigid.objTp != ObjectType.Mesh)
                return false;
            
            // Not runtime
            if (rigid.dmlTp == DemolitionType.None || 
                rigid.dmlTp == DemolitionType.ReferenceDemolition)
                return false;

            // No fragments
            if (rigid.HasFragments == false)
                return false;

            // Create Connected cluster Rigid
            RayfireRigid clsRigid = rigid.rtC.gameObject.AddComponent<RayfireRigid>();

            // Copy properties
            rigid.CopyPropertiesTo (clsRigid);
            
            // Copy particles
            RFPoolingParticles.CopyParticlesRigid (rigid, clsRigid);  

            // Destroy particles on fragments
            for (int i = rigid.fragments.Count - 1; i >= 0; i--)
            {
                // Destroy Debris/Dust for all fragments
                if (rigid.fragments[i].HasDebris)
                    for (int d = rigid.fragments[i].debrisList.Count - 1; d >= 0; d--)
                        Object.Destroy (rigid.fragments[i].debrisList[d]);
                if (rigid.fragments[i].HasDust)
                    for (int d = rigid.fragments[i].dustList.Count - 1; d >= 0; d--)
                        Object.Destroy (rigid.fragments[i].dustList[d]);
            }

            // Set properties
            clsRigid.objTp            = ObjectType.ConnectedCluster;
            clsRigid.dmlTp            = DemolitionType.Runtime;
            clsRigid.clsDemol.cluster = new RFCluster();
            
            // Init
            clsRigid.Initialize(); 

            // Set uny states and sim
            RayfireUnyielding[] unyArray = rigid.GetComponents<RayfireUnyielding>();
            if (unyArray.Length > 0)
                for (int i = 0; i < unyArray.Length; i++)
                    if (unyArray[i].enabled == true)
                        RayfireUnyielding.ClusterOverlap(unyArray[i], clsRigid);
            
            // Stop if awake connected cluster
            if (awake == true)
            {
                DestroyComponents (rigid.fragments);
                return true;
            }
            
            // Set contact point for demolition
            clsRigid.limitations.contactPoint   = rigid.limitations.contactPoint;
            clsRigid.limitations.contactNormal  = rigid.limitations.contactNormal;
            clsRigid.limitations.contactVector3 = rigid.limitations.contactVector3;

            // Inherit velocity
            clsRigid.physics.velocity    = rigid.physics.velocity;
            clsRigid.physics.rb.velocity = rigid.physics.velocity;
            
            // Demolish cluster and get solo shards
            List<RFShard> detachShards = RFDemolitionCluster.DemolishConnectedCluster (clsRigid);
  
            // No Shards to detach
            if (detachShards == null || detachShards.Count == 0)
            {
                DestroyComponents (rigid.fragments);
                return true;
            }

            // Get has for all detached objects to keep their Rigid and rigidbody. Should be used even if no detach shards.
            HashSet<Transform> detachTms = new HashSet<Transform>();
            for (int i = 0; i < detachShards.Count; i++)
                detachTms.Add (detachShards[i].tm);
           
            // Destroy fragments rigid, rigidbody for NOT detached shards
            for (int i = rigid.fragments.Count - 1; i >= 0; i--)
            {
                // Destroy rb, rigid
                if (detachTms.Contains (rigid.fragments[i].tsf) == false)
                {
                    Object.Destroy (rigid.fragments[i].physics.rb);
                    Object.Destroy (rigid.fragments[i]);
                    rigid.fragments.RemoveAt (i);
                }
            }

            // TODO add main and child clusters to fragments list. get them in scr.fragments
            
            // Delete if cluster was completely demolished
            if (clsRigid.limitations.demolished == true)
                RayfireMan.DestroyFragment (clsRigid, null);

            return true;
        }

        // Destroy fragments rigid, rigidbody
        static void DestroyComponents(List<RayfireRigid> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Object.Destroy (list[i].physics.rb);
                Object.Destroy (list[i]);
            }
        }

        // Setup Connectivity, Unyielding and MeshRoot
        public static void SetupRuntimeConnectivity(RayfireRigid scr, bool awake)
        {
            // Connectivity disabled
            if (scr.mshDemol.cnv != ConvertType.Connectivity)
                return;
            
            // Component check
            scr.activation.cnt = scr.GetComponent<RayfireConnectivity>();
            
            // Debug message and return
            if (scr.activation.cnt == null)
            {
                RayfireMan.Log (RFLimitations.rigidStr + scr.name + " Convert property set to " + scr.mshDemol.cnv.ToString() + " but object has no Connectivity component. Convertation disabled.", scr.gameObject);
                scr.mshDemol.cnv = ConvertType.Disabled;
                return;
            }
            
            // Add meshroot Rigid
            RayfireRigid mRoot = scr.rtC.gameObject.GetComponent<RayfireRigid>();
            
            // Skip if Runtime init and fragments already connected in awake.
            if (mRoot != null)
                return;

            mRoot = scr.rtC.gameObject.AddComponent<RayfireRigid>();

            // Set MeshRoot properties
            scr.CopyPropertiesTo (mRoot);
            mRoot.initialization = RayfireRigid.InitType.AtStart;
            mRoot.objTp          = ObjectType.MeshRoot;
            mRoot.dmlTp          = DemolitionType.None;
            mRoot.simTp          = scr.simTp;
            mRoot.activation.con = true;
            
                
            /*
            // TODO set mRoot fragments Rigid sim type to FragSimType
            put SetUnyieldingFragments after this method
            Debug.Log (mRoot.simTp);
            // Set sim type for root as well.
            if (mRoot.mshDemol.sim != FragSimType.Inherit)
                mRoot.simTp = (SimType)scr.mshDemol.sim;
            
            Debug.Log (mRoot.simTp);
            */
            
            // Set Connectivity activation for fragments
            for (int i = 0; i < scr.fragments.Count; i++)
                scr.fragments[i].activation.con = true;

            // Add Connectivity
            RayfireConnectivity mRootConnectivity = scr.rtC.gameObject.AddComponent<RayfireConnectivity>();

            // Copy Connectivity properties
            RayfireConnectivity.CopyTo (scr.activation.cnt, mRootConnectivity);

            // Activate to initialize
            scr.rtC.gameObject.SetActive (true);

            // DeActivate in awake / init at runtime
            if (awake == true)
                scr.rtC.gameObject.SetActive (false);
            else
            {
                mRoot.Initialize();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Children ops
        /// /////////////////////////////////////////////////////////  
        
        // Set children with mesh as additional fragments
        public static void ChildrenToFragments(RayfireRigid scr)
        {
            // Not for clusters
            if (scr.IsCluster == true)
                return;

            // Disabled
            if (scr.mshDemol.cld == false)
                return;
            
            // No children
            if (scr.tsf.childCount == 0)
                return;
            
            // Iterate children TODO precache in awake and use now. Set init type to by method at awake.
            Transform child;
            int childCount = scr.tsf.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                // Get child
                child = scr.tsf.GetChild (i);

                // Skip if has no mesh
                if (child.GetComponent<MeshFilter>() == false)
                    continue;

                // Set parent to main fragments root
                child.parent = scr.rtC;
                
                // Get Already applied Rigid
                RayfireRigid childScr = child.GetComponent<RayfireRigid>();

                // Add new if has no. Copy properties
                if (childScr == null)
                {
                    childScr = child.gameObject.AddComponent<RayfireRigid>();
                    childScr.initialization = RayfireRigid.InitType.ByMethod;
                    scr.CopyPropertiesTo (childScr);
                    
                    // Enable use shatter
                    childScr.mshDemol.sht = child.GetComponent<RayfireShatter>();
                    if (childScr.mshDemol.sht != null)
                        childScr.mshDemol.use = true;
                }
                
                // Set custom fragment simulation type if not inherited
                SetFragmentSimulationType (childScr, scr.simTp);

                // Init
                childScr.Initialize();
                
                // Update depth level and amount
                childScr.limitations.currentDepth = scr.limitations.currentDepth + 1;
                
                // Collect
                scr.fragments.Add (childScr);
            }
        }
    }
}