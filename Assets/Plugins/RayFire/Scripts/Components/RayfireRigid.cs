using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-rigid-component/")]
    public class RayfireRigid : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }

        // UI
        public                                                InitType              initialization = InitType.ByMethod;
        [FormerlySerializedAs ("simulationType")] public      SimType               simTp          = SimType.Dynamic;
        [FormerlySerializedAs ("objectType")]     public      ObjectType            objTp          = ObjectType.Mesh;
        [FormerlySerializedAs ("demolitionType")] public      DemolitionType        dmlTp          = DemolitionType.None;
        public                                                RFPhysic              physics        = new RFPhysic();
        public                                                RFActivation          activation     = new RFActivation();
        public                                                RFLimitations         limitations    = new RFLimitations();
        [FormerlySerializedAs ("meshDemolition")]      public RFDemolitionMesh      mshDemol       = new RFDemolitionMesh();
        [FormerlySerializedAs ("clusterDemolition")]   public RFDemolitionCluster   clsDemol       = new RFDemolitionCluster();
        [FormerlySerializedAs ("referenceDemolition")] public RFReferenceDemolition refDemol       = new RFReferenceDemolition();
        public                                                RFSurface             materials      = new RFSurface();
        public                                                RFDamage              damage         = new RFDamage();
        public                                                RFFade                fading         = new RFFade();
        public                                                RFReset               reset          = new RFReset();
        
        // Hidden
        public                                          bool                initialized;
        public                                          List<RayfireRigid>  fragments;
        [FormerlySerializedAs ("cacheRotation")] public Quaternion          chRot; // NOTE. Should be public, otherwise rotation error on demolition.
        [FormerlySerializedAs ("transForm")]     public Transform           tsf;
        [FormerlySerializedAs ("rootChild")]     public Transform           rtC;
        [FormerlySerializedAs ("rootParent")]    public Transform           rtP;
        [FormerlySerializedAs ("meshFilter")]    public MeshFilter          mFlt;
        [FormerlySerializedAs ("meshRenderer")]  public MeshRenderer        mRnd;
        public                                          SkinnedMeshRenderer skr;
        public                                          RayfireRestriction  rest;
        public                                          RayfireSound        sound;
       
        // Non Serialized
        [NonSerialized] public bool                corState;
        [NonSerialized] public List<Transform>     particleList;
        [NonSerialized] public List<RayfireDebris> debrisList;
        [NonSerialized] public List<RayfireDust>   dustList;
        [NonSerialized] public RFDictionary[]      subIds;
        [NonSerialized] public Vector3[]           pivots;
        [NonSerialized] public Mesh[]              meshes;
        [NonSerialized] public RayfireRigid        meshRoot;
        [NonSerialized] public RayfireRigidRoot    rigidRoot;
        [NonSerialized] public int                 debrisState = 1; // 1 - debrisList have  to be collected at Initialize
        [NonSerialized] public int                 dustState = 1;   // 0 - dustList already set by other object, skip collecting
        
        // Events
        public RFDemolitionEvent  demolitionEvent  = new RFDemolitionEvent();
        public RFActivationEvent  activationEvent  = new RFActivationEvent();
        public RFRestrictionEvent restrictionEvent = new RFRestrictionEvent();

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Awake Mesh input
            MeshInput();
            
            // Initialize at start
            if (initialization == InitType.AtStart)
                Initialize();
        }
        
        // Initialize 
        public void Initialize()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Not initialized
            if (initialized == false)
            {
                // Init Awake methods
                AwakeMethods();

                // Init sound
                RFSound.InitializationSound(sound, limitations.bboxSize);
            }
            
            // TODO add reinit for already initialized objects in case of property change
        }
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();

            // Set components for mesh / skinned mesh / clusters
            SetComponentsBasic();

            // Set particles
            RFPoolingParticles.InitializeParticles(this);
            
            // Init mesh root.
            if (SetupMeshRoot() == true)
                return;
            
            // Check for user mistakes
            RFLimitations.Checks(this);
            
            // Set components for mesh / skinned mesh / clusters
            SetComponentsPhysics();

            // Initialization Mesh input
            if (mshDemol.inp == RFDemolitionMesh.MeshInputType.AtInitialization)
                MeshInput();
            
            // Precache meshes at awake
            RFDemolitionMesh.Awake(this);

            // Skinned mesh
            SetSkinnedMesh();

            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Set Start variables
            SetObjectType();
            
            // Runtime ops
            if (Application.isPlaying == true)
            {
                // Start all coroutines
                StartAllCoroutines();

                // Object initialized
                initialized = true;
            }
        }

        // Set skinned mesh
        void SetSkinnedMesh()
        {
            // Skinned mesh FIXME
            if (objTp == ObjectType.SkinnedMesh)
            {
                // Reset rigid data
                Default();

                // Set physics properties
                physics.destructible = physics.Destructible;
                
                if (Application.isPlaying == true)
                    initialized = true;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Enable/Disable
        /// /////////////////////////////////////////////////////////
        
        // Disable
        void OnDisable()
        {
            // Set coroutines states
            corState                    = false;
            activation.velocityCorState = false;
            activation.offsetCorState   = false;
        }

        // Activation
        void OnEnable()
        {
            // Start cors // TODO add support for fragment caching and the rest cors:skinned
            if (gameObject.activeSelf == true && initialized == true && corState == false)
            {
                StartAllCoroutines();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Setup
        /// /////////////////////////////////////////////////////////

        // Editor Setup
        public void EditorSetup()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Setup mesh root
            if (objTp == ObjectType.MeshRoot)
                EditorSetupMeshRoot();

            // Setup clusters
            if (objTp == ObjectType.ConnectedCluster || objTp == ObjectType.NestedCluster)
                RFDemolitionCluster.ClusterizeEditor (this);
        }
        
        // Editor Reset
        public void ResetSetup()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Reset setup for mesh root
            if (objTp == ObjectType.MeshRoot)
                ResetMeshRootSetup();
            
            // Reset Setup for clusters 
            if (objTp == ObjectType.ConnectedCluster || objTp == ObjectType.NestedCluster)
                RFDemolitionCluster.ResetClusterize (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Awake ops
        /// /////////////////////////////////////////////////////////
        
        // Define basic components
        public void SetComponentsBasic()
        {
            // Set shatter component
            mshDemol.sht = mshDemol.use == true 
                ? GetComponent<RayfireShatter>() 
                : null;
            
            // Tm
            tsf = GetComponent<Transform>();
            
            // Mesh/Renderer components
            if (objTp == ObjectType.Mesh)
            {
                mFlt   = GetComponent<MeshFilter>();
                mRnd = GetComponent<MeshRenderer>();
            }
            else if (objTp == ObjectType.SkinnedMesh)
                skr = GetComponent<SkinnedMeshRenderer>();
            
            rest = GetComponent<RayfireRestriction>();

            // Add missing mesh renderer
            if (mFlt != null && mRnd == null)
                mRnd = gameObject.AddComponent<MeshRenderer>();

            // Init reset lists
            if (reset.action == RFReset.PostDemolitionType.DeactivateToReset)
                limitations.desc = new List<RayfireRigid>();
        }
        
        // Define components
        public void SetComponentsPhysics()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Physics components
            physics.rb = GetComponent<Rigidbody>();
            physics.mc = GetComponent<Collider>();
            
            // Mesh Set collider
            if (objTp == ObjectType.Mesh)
                RFPhysic.SetRigidCollider (this);
            
            // Cluster check
            if (objTp == ObjectType.NestedCluster || objTp == ObjectType.ConnectedCluster) 
                RFDemolitionCluster.Clusterize (this);
            
            // Rigid body
            if (Application.isPlaying == true)
                if (simTp != SimType.Static)
                    if (physics.rb == null)
                        physics.rb = gameObject.AddComponent<Rigidbody>();
        }

        /// /////////////////////////////////////////////////////////
        /// MeshRoot
        /// /////////////////////////////////////////////////////////

        // Setup mesh root editor ops
        void EditorSetupMeshRoot()
        {
            // Check if manager should be destroyed after setup
            bool destroyMan = RayfireMan.inst == null;

            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Reset
            ResetMeshRootSetup();
                
            // Setup
            SetupMeshRoot();
                
            // Destroy manager
            if (destroyMan == true && RayfireMan.inst != null)
                DestroyImmediate (RayfireMan.inst.transform.gameObject);
        }
        
        // Init mesh root. Copy Rigid component for children with mesh
        bool SetupMeshRoot()
        {
            if (objTp == ObjectType.MeshRoot)
            {
                // Get transform for Editor setup
                //if (transForm == null)
                //    transForm = GetComponent<Transform>();
                
                // Stop if already initiated
                if (limitations.demolished == true || physics.exclude == true)
                    return true;

                // Save tm
                physics.SaveInitTransform (transform);

                // MeshRoot Integrity check
                if (Application.isPlaying == true)
                    RFLimitations.MeshRootCheck(this);

                // Add Rigid to mesh Root children
                if (HasFragments == false)
                    AddMeshRootRigid(transform);
                
                // Init in runtime. DO not if editor setup
                if (Application.isPlaying == true)
                {
                    for (int i = 0; i < fragments.Count; i++)
                    {
                        fragments[i].Initialize();
                        fragments[i].meshRoot = this;
                    }
                }

                // Editor only ops
                if (Application.isPlaying == false)
                {
                    for (int i = 0; i < fragments.Count; i++)
                    {
                        // Set basic fragments components for collider apply
                        fragments[i].SetComponentsBasic();

                        // Set bound and size for connection size by bounding box
                        RFLimitations.SetBound (fragments[i]);
                    }
                    
                    // Add colliders to speedup. Editor only. Frags get collider at runtime in Initialize()
                    RFPhysic.SetupMeshRootColliders (this);
                }
                
                // Ignore neib collisions
                RFPhysic.SetIgnoreColliders (physics, fragments);
                
                // Runtime only ops
                if (Application.isPlaying == true)
                {
                    // Copy components. 
                    RayfireShatter.CopyRootMeshShatter (this, fragments);
                    RFPoolingParticles.CopyParticlesMeshroot (this, fragments);
                    
                    // Copy sound
                    sound = GetComponent<RayfireSound>();
                    RFSound.CopySound (sound, fragments);
                }
                
                // Set unyielding 
                RayfireUnyielding.MeshRootSetup (this);

                // Initialize connectivity
                InitConnectivity();
                
                // Turn off demolition and physics
                if (Application.isPlaying == true)
                {
                    dmlTp  = DemolitionType.None;
                    physics.exclude = true;
                    initialized     = true;
                }

                return true;
            }

            return false;
        }
        
        // Add Rigid to mesh Root children
        void AddMeshRootRigid(Transform tm)
        {
            // Get children
            List<Transform> children = new List<Transform>(tm.childCount);
            for (int i = 0; i < tm.childCount; i++)
                children.Add (tm.GetChild (i));
            
            // Add Rigid to child with mesh
            fragments = new List<RayfireRigid>();
            for (int i = 0; i < children.Count; i++)
            {
                MeshFilter mf = children[i].GetComponent<MeshFilter>();
                if (mf != null)
                {
                    // Get rigid
                    RayfireRigid childRigid = children[i].gameObject.GetComponent<RayfireRigid>();
                    
                    // Mark Rigid as custom Rigid component to keep it at Mesh Root Reset
                    if (childRigid != null)
                        childRigid.rtP = tm;

                    // Add new and copy props from parent
                    if (childRigid == null)
                    {
                        childRigid = children[i].gameObject.AddComponent<RayfireRigid>();
                        CopyPropertiesTo (childRigid);
                        
                        // Copy Runtime caching properties. They are disabled for base copy
                        childRigid.mshDemol.ch.CopyFrom (mshDemol.ch);
                    }
                    
                    // Set meshfilter
                    childRigid.mFlt = mf;

                    // Collect
                    fragments.Add (childRigid);

                    // Set parent meshRoot. IMPORTANT needed in case of custom Rigid
                    childRigid.meshRoot = this;
                }
            }
        }
        
        // Init connectivity if has
        void InitConnectivity()
        {
            activation.cnt = GetComponent<RayfireConnectivity>();
            if (activation.cnt != null && activation.cnt.rigidRootHost == null)
            {
                activation.cnt.meshRootHost = this;
                activation.cnt.Initialize();
            }
            
            // Warnings
            if (RayfireMan.debugStateStatic == true)
                if (activation.con == true && activation.cnt == null)
                    RayfireMan.Log ("RayFireRigid: " + name + " object has enabled Connectivity activation but has no Connectivity component.", gameObject);
        }
        
        // Reset MeshRoot Setup
        void ResetMeshRootSetup()
        {
            // Reset Connectivity
            if (activation.cnt != null)
                activation.cnt.ResetSetup();
            activation.cnt = null;
            
            // ReSet unyielding 
            RayfireUnyielding.ResetMeshRootSetup (this);
            
            // Destroy new Rigid and clear custom Rigid components
            if (HasFragments == true)
            {
                if (physics.cc != null)
                {
                    // Clean fragments
                    for (int i = fragments.Count - 1; i >= 0; i--)
                        if (fragments[i] == null)
                            fragments.RemoveAt (i);

                    // Destroy colliders added by setup
                    HashSet<Collider> collidersHash = new HashSet<Collider> (physics.cc);
                    for (int i = 0; i < fragments.Count; i++)
                        if (fragments[i].physics.mc != null)
                            if (collidersHash.Contains (fragments[i].physics.mc) == false)
                                DestroyImmediate (fragments[i].physics.mc);
                    physics.cc = null;

                    // Destroy Rigids added by setup
                    for (int i = 0; i < fragments.Count; i++)
                        if (fragments[i].rtP == null)
                            DestroyImmediate (fragments[i]);
                        else
                        {
                            fragments[i].rtP        = null;
                            fragments[i].mFlt       = null;
                            fragments[i].mRnd       = null;
                            fragments[i].physics.mc = null;
                            fragments[i].meshRoot   = null;
                        }
                }
            }

            // Reset common
            tsf          = null;
            physics.ign = null;
            fragments          = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Start ops
        /// /////////////////////////////////////////////////////////
        
        // Set Start variables
        public void SetObjectType ()
        {
            if (objTp == ObjectType.Mesh ||
                objTp == ObjectType.NestedCluster ||
                objTp == ObjectType.ConnectedCluster)
            
                // Reset rigid data
                Default();
                
                // Set physics properties
                SetPhysics();
        }
        
        // Reset rigid data
        public void Default()
        {
            // Reset
            limitations.LocalReset();
            mshDemol.LocalReset();
            clsDemol.LocalReset();
            
            limitations.birthTime = Time.time + Random.Range (0f, 0.05f);
           
            // Birth position for activation check
            physics.SaveInitTransform (tsf);

            // Set bound and size
            RFLimitations.SetBound(this);

            // Backup original layer
            RFActivation.BackupActivationLayer (this);

            // meshDemolition.properties.layerBack = gameObject.layer;
            // gameObject.tag;
        }
        
        // Set physics properties
        void SetPhysics()
        {
            // Excluded from sim
            if (physics.exclude == true)
                return;

            // MeshCollider physic material preset. Set new or take from parent 
            RFPhysic.SetColliderMaterial (this);

            // Set debris collider material
            // if (HasDebris == true) RFPhysic.SetParticleColliderMaterial (debrisList);
            
            // Ops with rigidbody applied
            if (physics.rb != null)
            {
                // Set physical simulation type. Important. Should after collider material define
                if (Application.isPlaying == true)
                    RFPhysic.SetSimulationType (physics.rb, simTp, objTp, physics.gr, physics.si, physics.st);

                // Do not set convex, mass, drag for static
                if (simTp == SimType.Static)
                    return;
                
                // Set drag properties
                RFPhysic.SetDrag (this);
                
                // Convex collider meshCollider. After SetSimulation Type to turn off convex for kinematic
                RFPhysic.SetColliderConvex (this);

                // Set density. After collider defined
                RFPhysic.SetDensity (this);
            }

            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Start all coroutines
        public void StartAllCoroutines()
        {
            // Stop if static
            if (simTp == SimType.Static)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Offset fade
            if (fading.byOffset > 0)
                RayfireMan.inst.AddToOffsetFadeCor (this);
            
            // Start inactive coroutines
            InactiveCors();

            // Cache velocity data for fragments 
            RayfireMan.inst.AddToPhysicCor (this);

            // All coroutines are running
            corState = true;
        }

        // Start inactive coroutines
        public void InactiveCors()
        {
            // Activation by velocity\offset coroutines
            if (simTp == SimType.Inactive || simTp == SimType.Kinematic)
            {
                if (activation.vel > 0)
                    RayfireMan.inst.AddToVelocityActivationCor(this);

                if (activation.off > 0)
                    RayfireMan.inst.AddToOffsetActivationCor (this);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition types
        /// /////////////////////////////////////////////////////////
        
        // Awake Mesh input // TODO add checks in case has input mesh but mesh input is off
        public void MeshInput()
        {
            if (objTp == ObjectType.Mesh && 
                dmlTp == DemolitionType.Runtime && 
                mshDemol.inp == RFDemolitionMesh.MeshInputType.AtStart)
            {
                // Set components for mesh / skinned mesh / clusters
                SetComponentsBasic();

                // Input
                RFFragment.InputMesh (this);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Collision check
        protected virtual void OnCollisionEnter (Collision collision)
        {
            // No demolition allowed
            if (dmlTp == DemolitionType.None)
                return;
            
            // Check if collision data needed
            if (limitations.CollisionCheck(this) == false)
                return;

            // Demolish object check
            if (DemolitionState() == false) 
                return;

            // Tag check. IMPORTANT keep length check for compatibility with older builds
            if (limitations.tag.Length > 0 && limitations.tag != "Untagged" && collision.collider.CompareTag (limitations.tag) == false)
                return;
            
            // Check if collision demolition passed
            if (CollisionDemolition (collision) == true)
            {
                limitations.demolitionShould = true;
                RayfireMan.inst.AddToDemolitionCor (this);
            }
        }
        
        // Check if collision demolition passed
        protected virtual bool CollisionDemolition (Collision collision)
        {
            // Final object solidity
            float finalSolidity = physics.solidity * limitations.sol * RayfireMan.inst.globalSolidity;

            // Demolition by collision
            if (limitations.col == true)
            {
                // Collision with kinematic object. Uses collision.impulse
                if (limitations.KinematicCollisionCheck(collision, finalSolidity) == true)
                    return true;

                // Collision force checks. Uses relativeVelocity
                if (limitations.ContactPointsCheck(collision, finalSolidity) == true)
                    return true;
            }

            // Demolition by accumulated damage collision
            if (damage.en == true && damage.col == true)
                if (limitations.DamagePointsCheck(collision, this) == true)
                    return true;

            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Demolition available state
        public bool State ()
        {
            // Object already demolished
            if (limitations.demolished == true)
                return false;
           
            // Object already passed demolition state and demolishing is in progress
            if (mshDemol.ch.inProgress == true)
                return false;
            
            // Bad mesh check
            if (mshDemol.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                return false;
            
            // Max amount check
            if (RayfireMan.MaxAmountCheck == false)
                return false;
            
            // Depth level check
            if (limitations.depth > 0 && limitations.currentDepth >= limitations.depth)
                return false;
            
            // Min Size check. Min Size should be considered and size is less than
            if (limitations.bboxSize < limitations.size)
                return false;

            // Safe frame
            if (Time.time - limitations.birthTime < limitations.time)
                return false;
           
            // Static type objects can not be demolished
            if (simTp == SimType.Static)
                return false;
            
            // Static objects can not be demolished
            if (gameObject.isStatic == true)
                return false;
            
            // Fading
            if (fading.state == 2)
                return false;
            
            return true;
        }
        
        // Check if object should be demolished
        public virtual bool DemolitionState ()
        {
            // No demolition allowed
            if (dmlTp == DemolitionType.None)
                return false;
            
            // Non destructible material
            if (physics.destructible == false)
                return false;
            
            // Visibility check
            if (Visible == false)
                return false;
            
            // Demolition available check
            if (State() == false)
                return false;

            return true;
        }
        
        // Demolish object even if its demolition type is none
        public void DemolishForced()
        {
            // Cache velocity
            if (physics.rb != null)
                physics.velocity = physics.rb.velocity;
            
            // TODO obj without rb: save tm, set dmlShould, compare tm at next frame at dml
            
            // Demolish
            Demolish();
        }

        // Demolish object
        public void Demolish()
        {
            // Initialize if not
            if (initialized == false)
            {
                Initialize();
            }
            
            // Demolish mesh or cluster to reference
            if (RFReferenceDemolition.DemolishReference(this) == false)
                return;
            
            // Demolish mesh and create fragments. Stop if runtime caching or no meshes/fragments were created
            if (RFDemolitionMesh.DemolishMesh (this) == true)
            {
                // Check for inactive/kinematic fragments with unyielding
                RayfireUnyielding.SetUnyieldingFragments (this, false);

                // Set children with mesh as additional fragments
                RFDemolitionMesh.ChildrenToFragments(this);
                
                // Clusterize runtime fragments. RUNTIME dml type ONLY
                RFDemolitionMesh.SetupRuntimeConnectedCluster (this, false);
                    
                // Setup awake connectivity
                RFDemolitionMesh.SetupRuntimeConnectivity(this, false);
            }
            else
                return;
            
            // Demolish cluster to children nodes 
            if (RFDemolitionCluster.DemolishCluster (this) == true)
                return;

            // Check fragments and proceed TODO separate flow for connected cls demolition
            if (limitations.demolished == false)
            {
                limitations.demolitionShould = false;
                dmlTp = DemolitionType.None;
                return;
            }
            
            // Connectivity check
            activation.CheckConnectivity();
            
            // Fragments initialisation
            InitMeshFragments();
            
            // Init particles
            RFPoolingEmitter.SetHostDemolition(this);

            // Init sound
            RFSound.DemolitionSound(sound, limitations.bboxSize);

            // Event
            RFDemolitionEvent.RigidDemolitionEvent (this);
            
            // Destroy demolished object
            RayfireMan.DestroyFragment (this, rtP, reset.destroyDelay);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fragments
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid properties from parent to fragments
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Set local meshRoot
            if (objTp == ObjectType.MeshRoot)
                toScr.meshRoot = this;
            else if (meshRoot != null)
                    toScr.meshRoot = meshRoot;

            // Object type
            toScr.objTp = objTp;
            if (objTp == ObjectType.MeshRoot || objTp == ObjectType.SkinnedMesh)
                toScr.objTp = ObjectType.Mesh;
            
            // Sim type
            toScr.simTp = simTp;
            
            // Demolition type
            toScr.dmlTp = dmlTp;
            if (objTp != ObjectType.MeshRoot)
                if (dmlTp != DemolitionType.None)
                    toScr.dmlTp = DemolitionType.Runtime;

            // Copy physics
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            toScr.limitations.CopyFrom (limitations);
            toScr.mshDemol.CopyFrom (mshDemol);
            toScr.clsDemol.CopyFrom (clsDemol);

            // Copy reference demolition props
            if (objTp == ObjectType.MeshRoot)
                toScr.refDemol.CopyFrom (refDemol);
            
            toScr.materials.CopyFrom (materials);
            toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            toScr.reset.CopyFrom (reset, objTp);
        }
        
        // Fragments initialisation
        public void InitMeshFragments()
        {
            // No fragments
            if (HasFragments == false)
                return;
            
            // Set velocity
            RFPhysic.SetFragmentsVelocity (this);
            
            // Sum total new fragments amount
            RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (fragments.Count);
            
            // Set ancestor and descendants 
            RFLimitations.SetAncestor (this);
            RFLimitations.SetDescendants (this);

            // Fading. move to fragment
            if (fading.onDemolition == true)
                fading.DemolitionFade (fragments);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Manual methods
        /// /////////////////////////////////////////////////////////
        
        // Clear cache info
        public void DeleteCache()
        {
            meshes = null;
            pivots = null;
            subIds = null;
        }
        
        // Delete fragments
        public void DeleteFragments()
        {
            // Destroy root
            if (rtC != null)
            {
                if (Application.isPlaying == true)
                    Destroy (rtC.gameObject);
                else
                    DestroyImmediate (rtC.gameObject);

                // Clear ref
                rtC = null;
            }

            // Clear array
            fragments = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Blade
        /// /////////////////////////////////////////////////////////

        // Add new slice plane
        public void AddSlicePlane (Vector3[] slicePlane)
        {
            // Not even amount of slice data
            if (slicePlane.Length % 2 == 1)
                return;

            // Add slice plane data
            if (limitations.slicePlanes == null)
                limitations.slicePlanes = new List<Vector3>();
            limitations.slicePlanes.AddRange (slicePlane);

            // Add to demolition cor for slice
            RayfireMan.inst.AddToDemolitionCor(this);
        }
        
        // Slice object
        public void Slice()
        {
            // Check for slices
            if (HasSlices == false)
            {
                RayfireMan.Log (RFLimitations.rigidStr + name + " has no defined slicing planes.", gameObject);
                return;
            }
            
            // Slice
            if (IsMesh == true)
            {
                // Slice. Stop if failed
                if (RFDemolitionMesh.SliceMesh (this) == false)
                    return;
                
                // Set children with mesh as additional fragments
                RFDemolitionMesh.ChildrenToFragments(this);
            }
            else if (objTp == ObjectType.ConnectedCluster)
                RFDemolitionCluster.SliceConnectedCluster (this);

            // Particles
            RFPoolingEmitter.SetHostDemolition(this);

            // Sound
            RFSound.DemolitionSound(sound, limitations.bboxSize);
            
            // Event
            RFDemolitionEvent.RigidDemolitionEvent (this);

            // Destroy original
            if (IsMesh == true)
                RayfireMan.DestroyFragment (this, rtP, reset.destroyDelay);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Caching into meshes over several frames
        public void CacheFrames()
        {
            StartCoroutine (mshDemol.ch.RuntimeCachingCor(this));
        }

        /// /////////////////////////////////////////////////////////
        /// Public methods
        /// /////////////////////////////////////////////////////////

        // Save init transform. Birth tm for activation check and reset
        [ContextMenu("SaveInitTransform")]
        public void SaveInitTransform ()
        {
            // Rigid save tm
            if (objTp == ObjectType.Mesh)
                physics.SaveInitTransform (tsf);
            
            // Mesh Root save tm
            else if (objTp == ObjectType.MeshRoot)
            {
                if (HasFragments == true)
                {
                    // Save for Rigids
                    for (int i = 0; i < fragments.Count; i++)
                         if (fragments[i] != null)
                            fragments[i].physics.SaveInitTransform (fragments[i].tsf);

                    // Save is connectivity backup cluster
                    if (activation.cnt != null && reset.connectivity == true )
                        if (activation.cnt.backup != null)
                            RFBackupCluster.SaveTmRecursive (activation.cnt.backup.cluster);
                }
            }
        }
        
        // Apply damage
        public bool ApplyDamage (float damageValue, Vector3 damagePoint, float damageRadius = 0f, Collider coll = null)
        {
            return RFDamage.ApplyDamage (this, damageValue, damagePoint, damageRadius, coll);
        }
        
        // Activate inactive object
        public void Activate(bool connCheck = true)
        {
            if (objTp != ObjectType.MeshRoot)
                RFActivation.ActivateRigid (this, connCheck);
            else
                for (int i = 0; i < fragments.Count; i++)
                    RFActivation.ActivateRigid (fragments[i], connCheck);
        }
        
        // Fade this object
        public void Fade()
        {
            if (objTp != ObjectType.MeshRoot)
                RFFade.FadeRigid (this);
            else
                for (int i = 0; i < fragments.Count; i++)
                    RFFade.FadeRigid (fragments[i]);
        }
        
        // Reset object
        public void ResetRigid()
        {
            RFReset.ResetRigid (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Destroy
        public void DestroyObject(GameObject go) { Destroy (go); }
        public void DestroyRigid(RayfireRigid rigid) { Destroy (rigid); }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Fragments/Meshes check
        public bool HasFragments { get { return fragments != null && fragments.Count > 0; } }
        public bool HasMeshes { get { return meshes != null && meshes.Length > 0; } }
        public bool HasDebris { get { return debrisList != null && debrisList.Count > 0; } }
        public bool HasDust   { get { return dustList != null && dustList.Count > 0; } }
        bool        HasSlices { get { return limitations.slicePlanes != null && limitations.slicePlanes.Count > 0; } }
        public bool IsCluster { get { return objTp == ObjectType.ConnectedCluster || objTp == ObjectType.NestedCluster; } }
        bool        IsMesh    { get { return objTp == ObjectType.Mesh || objTp == ObjectType.SkinnedMesh; } }
        
        // Check if object visible  // TODO add cluster visibility support
        bool Visible
        { get {
                if (objTp == ObjectType.Mesh && mRnd != null) return mRnd.isVisible;
                if (objTp == ObjectType.SkinnedMesh && skr != null) return skr.isVisible;
                return true; }}

        // CLuster Integrity
        public float AmountIntegrity
        { get {
                if (objTp == ObjectType.ConnectedCluster)
                    return  clsDemol.cluster.shards.Count * 100f / clsDemol.am;
                return 0f; }}
        
        /// /////////////////////////////////////////////////////////
        /// Compatibility
        /// /////////////////////////////////////////////////////////
        
        public SimType simulationType {
            get { return simTp; }
            set { simTp = value; } }
        public ObjectType objectType {
            get { return objTp; }
            set { objTp = value; } }
        public DemolitionType demolitionType {
            get { return dmlTp; }
            set { dmlTp = value; } }
        public RFDemolitionMesh meshDemolition {
            get { return mshDemol; }
            set { mshDemol = value; } }
        public RFDemolitionCluster clusterDemolition {
            get { return clsDemol; }
            set { clsDemol = value; } }
        public RFReferenceDemolition referenceDemolition {
            get { return refDemol; }
            set { refDemol = value; } }
        public Quaternion cacheRotation {
            get { return chRot; }
            set { chRot = value; } }
        public MeshFilter meshFilter {
            get { return mFlt; }
            set { mFlt = value; } }
        public MeshRenderer meshRenderer {
            get { return mRnd; }
            set { mRnd = value; } }
        public Transform transForm {
            get { return tsf; }
            set { tsf = value; } }
        public Transform rootChild {
            get { return rtC; }
            set { rtC = value; } } 
        public Transform rootParent {
            get { return rtP; }
            set { rtP = value; } }
    }
}


