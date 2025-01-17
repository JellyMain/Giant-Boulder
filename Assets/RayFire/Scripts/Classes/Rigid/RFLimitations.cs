using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [Serializable]
    public class RFLimitations
    {
        [FormerlySerializedAs ("byCollision")] public  bool   col;
        [FormerlySerializedAs ("solidity")]    public  float  sol;
        public                                         string tag;
        public                                         int    depth;
        public                                         float  time;
        public                                         float  size;
        [FormerlySerializedAs ("visible")]      public bool   vis;
        [FormerlySerializedAs ("sliceByBlade")] public bool   bld;
        public                                         Bounds bound;
        
        // Non serialized
        [NonSerialized] public List<Vector3> slicePlanes;
        [NonSerialized] public ContactPoint  contactPoint;
        [NonSerialized] public Vector3       contactVector3;
        [NonSerialized] public Vector3       contactNormal;
        [NonSerialized] public bool          demolitionShould;
        [NonSerialized] public bool          demolished;
        [NonSerialized] public float         birthTime;
        [NonSerialized] public float         bboxSize;
        [NonSerialized] public int           currentDepth;
        
        // Blade props
        [NonSerialized] public float         sliceForce;
        [NonSerialized] public bool          affectInactive;
        
        // Family data. Do not nullify in Reset
        [NonSerialized] public RayfireRigid       anc;  // ancestor
        [NonSerialized] public List<RayfireRigid> desc; // descendants
        
        // Static
        static        float  kinematicCollisionMult = 7f;
        static        string rootStr                = "_root";
        public static string rigidStr               = "RayFire Rigid: ";
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFLimitations()
        {
            InitValues();
            LocalReset();
        }
        
        // Copy from
        public void CopyFrom (RFLimitations source)
        {
            col   = source.col;
            sol   = source.sol;
            depth = source.depth;
            time  = source.time;
            size  = source.size;
            tag   = source.tag;
            vis   = source.vis;
            bld   = source.bld;
            
            // Do not copy currentDepth. Set in other place
            
            LocalReset();
        }

        // Starting values
        void InitValues()
        {
            col   = true;
            sol   = 0.1f;
            depth = 1;
            time  = 0.2f;
            size  = 0.1f;
            tag   = "Untagged";
            vis   = false;
            bld   = false;
            anc   = null;
            desc  = null;
        }

        // Reset
        public void LocalReset()
        {
            slicePlanes      = null;
            contactVector3   = Vector3.zero;
            contactNormal    = Vector3.down;
            demolitionShould = false;
            demolished       = false;
            affectInactive   = false;
            currentDepth     = 0;
            birthTime        = 0f;
            sliceForce       = 0;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
            
            // TODO
            // bound = new Bounds ();
            // contactPoint = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // MeshRoot Integrity check
        public static void MeshRootCheck (RayfireRigid scr)
        {
            // Null fragments
            if (scr.HasFragments == true)
            {
                for (int f = 0; f < scr.fragments.Count; f++)
                {
                    if (scr.fragments[f] == null)
                    {
                        RayfireMan.Log (rigidStr + scr.name + " object has missing fragments. Reset Setup and used Editor Setup again", scr.gameObject);
                        scr.fragments = new List<RayfireRigid>();
                        break;
                    }
                }
            }
        }
        
        // Check for user mistakes
        public static void Checks (RayfireRigid scr)
        {
            // ////////////////
            // Sim Type
            // ////////////////
            
            // Static and demolishable
            if (scr.simTp == SimType.Static)
            {
                if (scr.dmlTp != DemolitionType.None)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simTp.ToString() + " but Demolition Type is not None. Static object can not be demolished. Demolition Type set to None.", scr.gameObject);
                    scr.dmlTp = DemolitionType.None;
                }
            }
            
            // Non static simulation but static property
            if (scr.simTp != SimType.Static)
            {
                if (scr.gameObject.isStatic == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simTp.ToString() + " but object is Static. Turn off Static checkbox in Inspector.", scr.gameObject);
                }
            }
           
            // ////////////////
            // Object Type
            // ////////////////
            
            // Object can not be simulated as mesh
            if (scr.objTp == ObjectType.Mesh)
            {
                // Has no mesh
                if (scr.mFlt == null || scr.mFlt.sharedMesh == null)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Object Type set to " + scr.objTp.ToString() + " but object has no mesh. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
                
                // Awake or Runtime Convertation to Connectivity but fragments are not Inactive/Kinematik
                if (scr.mshDemol.cnv == RFDemolitionMesh.ConvertType.Connectivity)
                {
                    if (scr.mshDemol.sim != FragSimType.Inactive && scr.mshDemol.sim != FragSimType.Kinematic)
                    {
                        RayfireMan.Log (rigidStr + scr.name + " Convert property set to " + scr.mshDemol.cnv.ToString() + " but Fragments Sim Type is not Inactive or Kinematik. Convertation disabled.", scr.gameObject);
                        scr.mshDemol.cnv = RFDemolitionMesh.ConvertType.Disabled;
                    }
                }

                // Not readable mesh 
                if (scr.dmlTp != DemolitionType.None && scr.dmlTp != DemolitionType.ReferenceDemolition)
                {
                    if (scr.mFlt != null && scr.mFlt.sharedMesh != null && scr.mFlt.sharedMesh.isReadable == false)
                    {
                        RayfireMan.Log (rigidStr + scr.name + " Mesh is not readable. Demolition type set to None. Open Import Settings and turn On Read/Write Enabled property", scr.mFlt.gameObject);
                        scr.dmlTp         = DemolitionType.None;
                        scr.mshDemol.badMesh = 10;
                    }
                }
            }
            
            // Object can not be simulated as cluster
            else if (scr.objTp == ObjectType.NestedCluster || scr.objTp == ObjectType.ConnectedCluster)
            {
                if (scr.tsf.childCount == 0)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Object Type set to " + scr.objTp.ToString() + " but object has no children. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as mesh
            else if (scr.objTp == ObjectType.SkinnedMesh)
            {
                if (scr.skr == null)
                    RayfireMan.Log (rigidStr + scr.name + " Object Type set to " + scr.objTp.ToString() + " but object has no SkinnedMeshRenderer. Object Excluded from simulation.", scr.gameObject);
                
                // Excluded from sim by default
                scr.physics.exclude = true;
            }
            
            // ////////////////
            // Demolition Type
            // ////////////////
            
            // Demolition checks
            if (scr.dmlTp != DemolitionType.None)
            {
                // // Static
                // if (scr.simulationType == SimType.Static)
                // {
                //     Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to None.", scr.gameObject);
                //     scr.demolitionType = DemolitionType.None;
                // }
                
                // Set runtime demolition for clusters and skinned mesh
                if (scr.objTp == ObjectType.SkinnedMesh ||
                    scr.objTp == ObjectType.NestedCluster ||
                    scr.objTp == ObjectType.ConnectedCluster)
                {
                    if (scr.dmlTp != DemolitionType.Runtime && scr.dmlTp != DemolitionType.ReferenceDemolition)
                    {
                        RayfireMan.Log (rigidStr + scr.name + " Object Type set to " + scr.objTp.ToString() + " but Demolition Type is " + scr.dmlTp.ToString() + ". Demolition Type set to Runtime.", scr.gameObject);
                        scr.dmlTp = DemolitionType.Runtime;
                    }
                }
                
                // No Shatter component for runtime demolition with Use Shatter on
                if (scr.dmlTp == DemolitionType.Runtime ||
                    scr.dmlTp == DemolitionType.AwakePrecache ||
                    scr.dmlTp == DemolitionType.AwakePrefragment)
                {
                    if (scr.mshDemol.use == true && scr.mshDemol.sht == null)
                    {
                        RayfireMan.Log (rigidStr + scr.name + "Demolition Type is " + scr.dmlTp.ToString() + ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.", scr.gameObject);
                        scr.mshDemol.use = false;
                    }
                }
            }
            
            // None check
            if (scr.dmlTp == DemolitionType.None)
            {
                if (scr.HasMeshes == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to None. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objTp == ObjectType.Mesh && scr.HasFragments == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to None. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }
            }

            // Runtime check
            else if (scr.dmlTp == DemolitionType.Runtime)
            {
                if (scr.HasMeshes == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objTp == ObjectType.Mesh && scr.HasFragments == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }
                
                // No runtime caching for rigid with shatter with tets/slices/glue
                if (scr.mshDemol.use == true && scr.mshDemol.ch.tp != CachingType.Disabled)
                {
                    if (scr.mshDemol.sht.type == FragType.Decompose ||
                        scr.mshDemol.sht.type == FragType.Tets ||
                        scr.mshDemol.sht.type == FragType.Slices || 
                        scr.mshDemol.sht.clusters.enable == true)
                    {
                        RayfireMan.Log (rigidStr + scr.name + " Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.", scr.gameObject);
                        scr.mshDemol.ch.tp = CachingType.Disabled;
                    }
                }
            }

            // Awake precache check
            else if (scr.dmlTp == DemolitionType.AwakePrecache)
            {
                if (scr.HasMeshes == true)
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually precached Unity meshes which were overwritten.", scr.gameObject);
                
                if (scr.HasFragments == true)
                {
                    RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }
            }

            // Awake prefragmented check
            else if (scr.dmlTp == DemolitionType.AwakePrefragment)
            {
                if (RayfireMan.debugStateStatic == true)
                {
                    if (scr.HasFragments == true)
                        RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually prefragmented objects", scr.gameObject);
                    if (scr.HasMeshes == true)
                        RayfireMan.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached Unity meshes.", scr.gameObject);
                }
            }
            
            // Reference demolition
            else if (scr.dmlTp == DemolitionType.ReferenceDemolition)
            {
                if (scr.refDemol.rfs == null && scr.refDemol.HasRandomRefs == false)
                {
                    //Debug.Log (rigidStr + scr.name + " Demolition Type set to Reference Demolition but Reference is not defined.", scr.gameObject);
                }
            }
        }
        
        // Set bound and size
        public static void SetBound (RayfireRigid scr)
        {
            if (scr.objTp == ObjectType.Mesh)
                scr.limitations.bound = scr.mRnd.bounds;
            else if (scr.objTp == ObjectType.SkinnedMesh)
                scr.limitations.bound = scr.skr.bounds;
            else if (scr.objTp == ObjectType.NestedCluster || scr.objTp == ObjectType.ConnectedCluster)
                scr.limitations.bound = RFCluster.GetChildrenBound (scr.tsf);
            scr.limitations.bboxSize = scr.limitations.bound.size.magnitude;
        }
        
        // Set ancestor
        public static void SetAncestor (RayfireRigid scr)
        {
            // Set ancestor to this if it is ancestor
            if (scr.limitations.anc == null)
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].limitations.anc = scr;
            else
                for (int i = 0; i < scr.fragments.Count; i++) 
                    scr.fragments[i].limitations.anc = scr.limitations.anc;
        }
        
        // Set descendants 
        public static void SetDescendants (RayfireRigid scr)
        {
            if (scr.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
                return;

            if (scr.limitations.anc == null)
                scr.limitations.desc.AddRange (scr.fragments);
            else
                scr.limitations.anc.limitations.desc.AddRange (scr.fragments);
        }
        
        // Create root
        public static void CreateRoot (RayfireRigid rfScr)
        {
           GameObject root = new GameObject(rfScr.gameObject.name + rootStr);
           rfScr.rtC          = root.transform;
           rfScr.rtC.position = rfScr.tsf.position;
           rfScr.rtC.rotation = rfScr.tsf.rotation;
           rfScr.rtC.SetParent (rfScr.transform.parent);
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Check if collision data needed
        public bool CollisionCheck(RayfireRigid rigid)
        {
            if (rigid.limitations.col == true)
                return true;
            if (rigid.damage.en == true && rigid.damage.col == true)
                return true;
            return false;
        }
        
        // Set Contact info
        public void SetContactInfo(ContactPoint contact)
        {
            contactPoint   = contact;
            contactVector3 = contactPoint.point;
            contactNormal  = contactPoint.normal;
        }
        
        // Collision with kinematic object. Uses collision.impulse
        public bool KinematicCollisionCheck(Collision collision, float finalSolidity)
        {
            if (collision.rigidbody != null && collision.rigidbody.isKinematic == true)
                if (collision.impulse.magnitude > finalSolidity * kinematicCollisionMult)
                {
                    SetContactInfo (collision.GetContact(0));
                    return true;
                }
            return false;
        }

        // Collision force checks. Uses relativeVelocity
        public bool ContactPointsCheck(Collision collision, float finalSolidity)
        {
            float collisionMagnitude = collision.relativeVelocity.magnitude;
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Set contact point
                SetContactInfo (collision.GetContact(i));
                
                // Demolish if collision high enough
                if (collisionMagnitude > finalSolidity)
                    return true;
            }
            
            return false;
        }
        
        // Collision force checks. Uses relativeVelocity
        public bool DamagePointsCheck(Collision collision, RayfireRigid rigid)
        {
            float collisionMagnitude = collision.relativeVelocity.magnitude;
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Set contact point
                SetContactInfo (collision.GetContact(i));
                
                // Collect damage by collision
                if (rigid.ApplyDamage (collisionMagnitude * rigid.damage.mlt, contactVector3, 0f, collision.contacts[i].thisCollider) == true)
                        return true;
            }
            
            return false;
        }
    }
}