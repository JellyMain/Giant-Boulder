namespace RayFire
{
    
    /// <summary>
    /// Rayfire man fragment parent type.
    /// </summary>
    public enum FragmentParentType
    {
        Manager      = 0,
        LocalParent  = 1,
        GlobalParent = 2
            
    }
    
    /// <summary>
    /// Rayfire man demolition quota action type.
    /// </summary>
    public enum QuotaType
    {
        Skip     = 0,
        Postpone = 1
            
    }
    
    /// <summary>
    /// Rayfire axes type.
    /// </summary>
    public enum AxisType
    {
        XRed   = 0,
        YGreen = 1,
        ZBlue  = 2
    }

    /// <summary>
    /// Rayfire planes type.
    /// </summary>
    public enum PlaneType
    {
        XY = 0,
        XZ = 1,
        YZ = 2
    }

    /// <summary>
    /// Rayfire Shatter fragmentation type.
    /// </summary>
    public enum FragType
    {
        Voronoi   = 0,
        Splinters = 1,
        Slabs     = 2,
        Radial    = 4,
        Hexagon   = 3,
        Custom    = 5,
        //Mirrored  = 6,
        Slices    = 7,
        Bricks    = 9,
        Voxels    = 10,
        Tets      = 11,
        Decompose = 15
    }

    /// <summary>
    /// Rayfire Rigid demolition type.
    /// </summary>
    public enum DemolitionType
    {
        None                 = 0, // Object not demolished
        Runtime              = 1, // Demolish during runtime
        AwakePrecache        = 2, // Precalculate mesh and pivot array or calculate them in Awake if they are empty
        AwakePrefragment     = 3, // Prefragment and keep fragments disabled as children or prefragment in awake
        ReferenceDemolition  = 9
    }
    
    /// <summary>
    /// Rayfire Rigid runtime caching type.
    /// </summary>
    public enum CachingType
    {
        Disabled             = 0,
        ByFrames            = 1,
        ByFragmentsPerFrame = 2
    }
    
    /// <summary>
    /// Rayfire Rigid fragments fading type.
    /// </summary>
    public enum FadeType
    {
        None         = 0, // Fragments stay as dynamic objects forever
        SimExclude   = 1, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and stay in scene forever
        FallDown     = 2, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation, fall under ground and then destroyed.
        ScaleDown    = 3, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and scale down during fade time, then destroyed
        MoveDown     = 4, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation, move under ground and then destroyed.
        Destroy      = 5, // Fragments stay as dynamic during lifetime/while moved, then destroyed
        SetStatic    = 8, // Fragments stay as dynamic during lifetime/while moved, then set to static
        SetKinematic = 9  // Fragments stay as dynamic during lifetime/while moved, then set to kinematic
        
        // ,Combine      = 11, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and stay in scene forever
    }
    
    /// <summary>
    /// Rayfire Rigid fragments fading life type.
    /// </summary>
    public enum RFFadeLifeType
    {
        ByLifeTime              = 4,
        BySimulationAndLifeTime = 8
    }

    /// <summary>
    /// Rayfire Rigid fragments physical material type.
    /// </summary>
    public enum MaterialType
    {
        HeavyMetal = 0,
        LightMetal = 1,
        DenseRock  = 2,
        PorousRock = 3,
        Concrete   = 4,
        Brick      = 5,
        Glass      = 6,
        Rubber     = 7,
        Ice        = 8,
        Wood       = 9
    }

    /// <summary>
    /// Rayfire Rigid fragments mass type.
    /// </summary>
    public enum MassType
    {
        MaterialDensity    = 0,
        MassProperty       = 1,
        RigidBodyComponent = 2
    }
    
    /// <summary>
    /// Rayfire Rigid object type.
    /// </summary>
    public enum ObjectType
    {
        Mesh             = 0,
        MeshRoot         = 1,
        SkinnedMesh      = 2,
        NestedCluster    = 4,
        ConnectedCluster = 5
    }

    /// <summary>
    /// Rayfire Rigid object simulation type.
    /// </summary>
    public enum SimType
    {
        Dynamic   = 0, // Fall down, get affected by other objects
        Sleeping  = 1,
        Inactive  = 2, // Do not fall down, stop impulse
        Kinematic = 3,
        Static    = 4
    }
    
    /// <summary>
    /// Rayfire Rigid fragments simulation type.
    /// </summary>
    public enum FragSimType
    {
        Dynamic   = 0, 
        Sleeping  = 1,
        Inactive  = 2, 
        Kinematic = 3,
        Inherit   = 4
    }

    /// <summary>
    /// Rayfire Connectivity and Rayfire Rigid Connected Cluster connectivity type.
    /// </summary>
    public enum ConnectivityType
    {
        ByBoundingBox             = 0,
        ByTriangles               = 1,
        ByPolygons                = 3,
        ByBoundingBoxAndTriangles = 2,
        ByBoundingBoxAndPolygons  = 4,
    }
    
    /// <summary>
    /// Rayfire Shatter fragmentation mode type.
    /// </summary>
    public enum FragmentMode
    {
        Runtime = 0,
        Editor  = 1
    }
    
    /// <summary>
    /// Rayfire Rigid colliders type.
    /// </summary>
    public enum RFColliderType
    {
        Mesh   = 0,
        Box    = 1,
        Sphere = 2,
        None   = 4
    }

}