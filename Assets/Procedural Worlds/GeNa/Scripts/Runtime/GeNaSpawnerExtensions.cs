using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    public static class GeNaSpawnerExtensions
    {
        #region Adding Palette Entries
        public static void AddBrushTexture(this Resource resource, Texture2D texture)
        {
            resource.AddBrushTexture(texture, resource.Palette);
        }
        public static void AddBrushTexture(this Resource resource, Texture2D texture, Palette palette)
        {
            //TODO : Manny : Add Brush Texture shouldn't be an internal method (should be an extension)
            bool emptySet = resource.BrushTextures == null || resource.BrushTextures.Count < 1;
            // Using a set to avoid duplications
            HashSet<Texture2D> set = emptySet ? new HashSet<Texture2D>() : new HashSet<Texture2D>(resource.BrushTextures);
            if (texture != null)
            {
                int id = palette.AddObject(texture);
                if (palette.IsValidID(id))
                {
                    resource.BrushTextureIDs.Add(id);
                    set.Add(texture);
                }
            }
            resource.BrushTextures = new List<Texture2D>(set);
            // Select it and update the texture if the set was empty
            if (emptySet || resource.BrushTXIndex < 0)
            {
                resource.BrushTXIndex = 0;
                resource.UpdateBrushTexture();
            }
        }
        public static void AddSpawner(this Prototype prototype, GeNaSpawner spawner, Palette palette)
        {
            GameObject gameObject = spawner.gameObject;
            if (gameObject != null)
            {
                int id = palette.AddObject(gameObject);
                if (palette.IsValidID(id))
                    prototype.SpawnerPaletteID = id;
            }
            // prototype.LoadReferences(palette);
        }
        public static void AddPrefab(this Resource resource, GameObject prefab, Palette palette)
        {
            if (prefab != null)
            {
                int id = palette.AddObject(prefab);
                if (palette.IsValidID(id))
                {
                    resource.AssetID = palette.GetGUID(id);
                    resource.PrefabPaletteID = id;
                    resource.Prefab = prefab;
                }
            }
            // resource.LoadReferences(palette);
        }
        public static void AddSubSpawner(this Resource resource, GameObject subSpawnerPrefab, Palette palette)
        {
            if (subSpawnerPrefab != null)
            {
                int id = palette.AddObject(subSpawnerPrefab);
                if (palette.IsValidID(id))
                {
                    resource.SubSpawnerPaletteID = id;
                    GeNaSpawner subSpawner = subSpawnerPrefab.GetComponent<GeNaSpawner>();
                    if (subSpawner != null)
                    {
                        subSpawner.Deserialize();
                        resource.SubSpawnerData = subSpawner.SpawnerData;
                        //subSpawner.LoadAllReferences();
                    }
                }
            }
            // resource.LoadReferences(palette);
        }
        public static void AddMaskImage(this SpawnCriteria spawnCriteria, Texture2D maskImage, Palette palette)
        {
            if (maskImage != null)
            {
                int id = palette.AddObject(maskImage);
                if (palette.IsValidID(id))
                {
                    spawnCriteria.MaskImagePaletteID = id;
                    spawnCriteria.MaskImage = maskImage;
                }
            }
            // spawnCriteria.LoadReferences(palette);
        }
        public static void AddDetailPrototype(this Resource resource, GameObject gameObject, Palette palette)
        {
            if (gameObject != null)
            {
                int id = palette.AddObject(gameObject);
                if (palette.IsValidID(id))
                {
                    resource.AssetID = palette.GetGUID(id);
                    resource.DetailPrototypeData.prototypeGameObjectPaletteID = id;
                    resource.DetailPrototypeData.prototype = gameObject;
                }
            }
            // terrainDetailPrototypeData.LoadReferences(palette);
        }
        public static void AddDetailPrototype(this Resource resource, Texture2D texture2D, Palette palette)
        {
            if (texture2D != null)
            {
                int id = palette.AddObject(texture2D);
                if (palette.IsValidID(id))
                {
                    resource.AssetID = palette.GetGUID(id);
                    resource.DetailPrototypeData.prototypeTexturePaletteID = id;
                    resource.DetailPrototypeData.prototypeTexture = texture2D;
                }
            }
// terrainDetailPrototypeData.LoadReferences(palette);
        }
        public static void AddTerrainLayerAsset(this Resource resource, Texture2D terrainTexture2D, Palette palette)
        {
            if (terrainTexture2D != null)
            {
                int id = palette.AddObject(terrainTexture2D);
                if (palette.IsValidID(id))
                {
                    resource.AssetID = palette.GetGUID(id);
                    resource.TexturePrototypeData.terrainTextureAssetPaletteID = id;
                    resource.TexturePrototypeData.terrainTexture2DAsset = terrainTexture2D;
                }
            }
            // terrainTexturePrototypeData.LoadReferences(palette);
        }
        public static void AddBrushTextures(this TerrainModifier terrainModifier, List<Texture2D> brushTextures, Palette palette)
        {
            terrainModifier.BrushTextureIDs.Clear();
            foreach (Texture2D brushTexture in brushTextures)
            {
                if (brushTexture == null)
                    continue;
                int id = palette.AddObject(brushTexture);
                if (palette.IsValidID(id))
                {
                    terrainModifier.BrushTextureIDs.Add(id);
                }
            }
        }
        #endregion
    }
}