using System;
using UnityEngine;

namespace RayFire
{
    /// <summary>
    /// Rayfire man physical material class.
    /// </summary>
    public class RFMaterial
    {
        private string         name;
        public  bool           destructible;
        public  int            solidity;
        public  float          density;
        public  float          drag;
        public  float          angularDrag;
        public  PhysicMaterial material;
        public  float          dynamicFriction;
        public  float          staticFriction;
        public  float          bounciness;
        
        public RFMaterial(
            string Name, 
            float Density, 
            float Drag, 
            float AngularDrag, 
            int Solidity, 
            bool Destructible, 
            float DynFriction,
            float StFriction, 
            float Bounce)
        {
            name            = Name;
            density         = Density;
            drag            = Drag;
            angularDrag     = AngularDrag;
            solidity        = Solidity;
            destructible    = Destructible;
            dynamicFriction = DynFriction;
            staticFriction  = StFriction;
            bounciness      = Bounce;
        }

        // Get Physic material
        public PhysicMaterial Material
        {
            get
            {
                PhysicMaterial physMat = new PhysicMaterial();
                physMat.name = name;
                physMat.dynamicFriction = dynamicFriction;
                physMat.staticFriction = staticFriction;
                physMat.bounciness = bounciness;
                physMat.frictionCombine = PhysicMaterialCombine.Minimum;
                return physMat;
            }
        }
    }

    /// <summary>
    /// Rayfire man physical material preset class.
    /// </summary>
    [Serializable]
    public class RFMaterialPresets
    {
        // UI properties. Do not change for material change
        public MaterialType   type;
        public bool           dest;
        public int            sol;
        public float          dens;
        public float          drag;
        public float          ang;
        public PhysicMaterial mat;
        public float          dyn;
        public float          stat;
        public float          bnc;
        
        // Actual materials to change properties via code.
        public RFMaterial   heavyMetal;
        public RFMaterial   lightMetal;
        public RFMaterial   denseRock;
        public RFMaterial   porousRock;
        public RFMaterial   concrete;
        public RFMaterial   brick;
        public RFMaterial   glass;
        public RFMaterial   rubber;
        public RFMaterial   ice;
        public RFMaterial   wood;
        
        public RFMaterialPresets()
        {
            heavyMetal = new RFMaterial ("HeavyMetal", 11f,  0f, 0.05f, 80, false, 0.75f, 0.7f,  0.17f);
            lightMetal = new RFMaterial ("LightMetal", 8f,   0f, 0.05f, 50, false, 0.71f, 0.72f, 0.14f);
            denseRock  = new RFMaterial ("DenseRock",  4f,   0f, 0.05f, 22, true,  0.88f, 0.87f, 0.14f);
            porousRock = new RFMaterial ("PorousRock", 2.5f, 0f, 0.05f, 12, true,  0.84f, 0.82f, 0.16f);
            concrete   = new RFMaterial ("Concrete",   3f,   0f, 0.05f, 18, true,  0.81f, 0.83f, 0.15f);
            brick      = new RFMaterial ("Brick",      2.3f, 0f, 0.05f, 10, true,  0.76f, 0.75f, 0.13f);
            glass      = new RFMaterial ("Glass",      1.8f, 0f, 0.05f, 3,  true,  0.53f, 0.53f, 0.2f);
            rubber     = new RFMaterial ("Rubber",     1.4f, 0f, 0.05f, 1,  false, 0.95f, 0.98f, 0.93f);
            ice        = new RFMaterial ("Ice",        1f,   0f, 0.05f, 2,  true,  0.07f, 0.07f, 0f);
            wood       = new RFMaterial ("Wood",       0.7f, 0f, 0.05f, 4,  true,  0.75f, 0.73f, 0.22f);
        }

        // Create physic material if it was not applied by user
        public void SetMaterials()
        {
            if (heavyMetal.material == null) heavyMetal.material = heavyMetal.Material;
            if (lightMetal.material == null) lightMetal.material = lightMetal.Material;
            if (denseRock.material == null) denseRock.material   = denseRock.Material;
            if (porousRock.material == null) porousRock.material = porousRock.Material;
            if (concrete.material == null) concrete.material     = concrete.Material;
            if (brick.material == null) brick.material           = brick.Material;
            if (glass.material == null) glass.material           = glass.Material;
            if (rubber.material == null) rubber.material         = rubber.Material;
            if (ice.material == null) ice.material               = ice.Material;
            if (wood.material == null) wood.material             = wood.Material;
        }

        // Get density by material Type
        public float Density (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.density; 
                case MaterialType.Brick: return brick.density;
                case MaterialType.Glass: return glass.density;
                case MaterialType.Rubber: return rubber.density;
                case MaterialType.Ice: return ice.density;
                case MaterialType.Wood: return wood.density;
                case MaterialType.HeavyMetal: return heavyMetal.density;
                case MaterialType.LightMetal: return lightMetal.density;
                case MaterialType.DenseRock: return denseRock.density;
                case MaterialType.PorousRock: return porousRock.density;
            }
            return 2f;
        }
        
        // Get Drag by material Type
        public float Drag (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.drag; 
                case MaterialType.Brick: return brick.drag;
                case MaterialType.Glass: return glass.drag;
                case MaterialType.Rubber: return rubber.drag;
                case MaterialType.Ice: return ice.drag;
                case MaterialType.Wood: return wood.drag;
                case MaterialType.HeavyMetal: return heavyMetal.drag;
                case MaterialType.LightMetal: return lightMetal.drag;
                case MaterialType.DenseRock: return denseRock.drag;
                case MaterialType.PorousRock: return porousRock.drag;
            }
            return 0f;
        }
        
        // Get AngularDrag by material Type
        public float AngularDrag (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.angularDrag; 
                case MaterialType.Brick: return brick.angularDrag;
                case MaterialType.Glass: return glass.angularDrag;
                case MaterialType.Rubber: return rubber.angularDrag;
                case MaterialType.Ice: return ice.angularDrag;
                case MaterialType.Wood: return wood.angularDrag;
                case MaterialType.HeavyMetal: return heavyMetal.angularDrag;
                case MaterialType.LightMetal: return lightMetal.angularDrag;
                case MaterialType.DenseRock: return denseRock.angularDrag;
                case MaterialType.PorousRock: return porousRock.angularDrag;
            }
            return 0.05f;
        }

        // Get solidity by material type
        public int Solidity (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.solidity; 
                case MaterialType.Brick: return brick.solidity;
                case MaterialType.Glass: return glass.solidity;
                case MaterialType.Rubber: return rubber.solidity;
                case MaterialType.Ice: return ice.solidity;
                case MaterialType.Wood: return wood.solidity;
                case MaterialType.HeavyMetal: return heavyMetal.solidity;
                case MaterialType.LightMetal: return lightMetal.solidity;
                case MaterialType.DenseRock: return denseRock.solidity;
                case MaterialType.PorousRock: return porousRock.solidity;
            }
            return 1;
        }
        
        // Get destructible by material type
        public bool Destructible (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.destructible; 
                case MaterialType.Brick: return brick.destructible;
                case MaterialType.Glass: return glass.destructible;
                case MaterialType.Rubber: return rubber.destructible;
                case MaterialType.Ice: return ice.destructible;
                case MaterialType.Wood: return wood.destructible;
                case MaterialType.HeavyMetal: return heavyMetal.destructible;
                case MaterialType.LightMetal: return lightMetal.destructible;
                case MaterialType.DenseRock: return denseRock.destructible;
                case MaterialType.PorousRock: return porousRock.destructible;
            }
            return true;
        }
        
        // Get DynamicFriction by material Type
        public float DynamicFriction (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.dynamicFriction; 
                case MaterialType.Brick: return brick.dynamicFriction;
                case MaterialType.Glass: return glass.dynamicFriction;
                case MaterialType.Rubber: return rubber.dynamicFriction;
                case MaterialType.Ice: return ice.dynamicFriction;
                case MaterialType.Wood: return wood.dynamicFriction;
                case MaterialType.HeavyMetal: return heavyMetal.dynamicFriction;
                case MaterialType.LightMetal: return lightMetal.dynamicFriction;
                case MaterialType.DenseRock: return denseRock.dynamicFriction;
                case MaterialType.PorousRock: return porousRock.dynamicFriction;
            }
            return 0.5f;
        }
        
        // Get DynamicFriction by material Type
        public float StaticFriction (MaterialType materialType)
        {            
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.staticFriction; 
                case MaterialType.Brick: return brick.staticFriction;
                case MaterialType.Glass: return glass.staticFriction;
                case MaterialType.Rubber: return rubber.staticFriction;
                case MaterialType.Ice: return ice.staticFriction;
                case MaterialType.Wood: return wood.staticFriction;
                case MaterialType.HeavyMetal: return heavyMetal.staticFriction;
                case MaterialType.LightMetal: return lightMetal.staticFriction;
                case MaterialType.DenseRock: return denseRock.staticFriction;
                case MaterialType.PorousRock: return porousRock.staticFriction;
            }
            return 0.5f;
        }
        
        // Get Bounciness by material Type
        public float Bounciness (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return concrete.bounciness; 
                case MaterialType.Brick: return brick.bounciness;
                case MaterialType.Glass: return glass.bounciness;
                case MaterialType.Rubber: return rubber.bounciness;
                case MaterialType.Ice: return ice.bounciness;
                case MaterialType.Wood: return wood.bounciness;
                case MaterialType.HeavyMetal: return heavyMetal.bounciness;
                case MaterialType.LightMetal: return lightMetal.bounciness;
                case MaterialType.DenseRock: return denseRock.bounciness;
                case MaterialType.PorousRock: return porousRock.bounciness;
            }
            return 0.5f;
        }
        
        // Create material by material type
        public static PhysicMaterial PhysicMaterial (MaterialType materialType)
        {
            switch (materialType)
            { 
                case MaterialType.Concrete: return RayfireMan.inst.materialPresets.concrete.material;
                case MaterialType.Brick: return RayfireMan.inst.materialPresets.brick.material;
                case MaterialType.Glass: return RayfireMan.inst.materialPresets.glass.material;
                case MaterialType.Rubber: return RayfireMan.inst.materialPresets.rubber.material;
                case MaterialType.Ice: return RayfireMan.inst.materialPresets.ice.material;
                case MaterialType.Wood: return RayfireMan.inst.materialPresets.wood.material;
                case MaterialType.HeavyMetal: return RayfireMan.inst.materialPresets.heavyMetal.material;
                case MaterialType.LightMetal: return RayfireMan.inst.materialPresets.lightMetal.material;
                case MaterialType.DenseRock: return RayfireMan.inst.materialPresets.denseRock.material;
                case MaterialType.PorousRock: return RayfireMan.inst.materialPresets.porousRock.material;
            }
            return RayfireMan.inst.materialPresets.concrete.material;
        }
    }
}