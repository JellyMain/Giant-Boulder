using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaTerrainDecorator))]
    public class GeNaTerrainDecoratorEditor : GeNaDecoratorEditor<GeNaTerrainDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Terrain Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaTerrainDecorator>(command);
        protected GeNaTerrainDecorator[] m_tree;
        protected TerrainTools m_terrainTools;
        public TerrainTools TerrainTools
        {
            get
            {
                if (m_terrainTools == null)
                {
                    GeNaManager gm = GeNaManager.GetInstance();
                    if (gm != null)
                    {
                        m_terrainTools = gm.TerrainTools;
                    }
                }
                return m_terrainTools;
            }
        }
        protected void SelectTree(bool isSelected)
        {
            Transform transform = Decorator.transform;
            Transform root = transform.root;
            m_tree = root.GetComponentsInChildren<GeNaTerrainDecorator>();
            foreach (GeNaTerrainDecorator tree in m_tree)
                tree.IsSelected = isSelected;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Decorator == null)
                return;
            SelectTree(true);
        }
        protected void OnDisable()
        {
            if (Decorator == null)
                return;
            SelectTree(false);
        }
        public override void OnSceneGUI()
        {
            GeNaTerrainDecorator decorator = target as GeNaTerrainDecorator;
            if (decorator == null)
                return;
            Transform transform = decorator.transform;
            TerrainModifier modifier = decorator.TerrainModifier;
            GeNaEditorUtility.RenderTerrainModifier(transform, modifier);
            TerrainEntity terrainEntity = modifier.GenerateTerrainEntity();
            if (terrainEntity != null)
            {
                TerrainTools.Visualize(terrainEntity);
                terrainEntity.Dispose();
            }
        }
        protected void AddBrushTexture(Texture2D texture2D)
        {
            var terrainModifier = Decorator.TerrainModifier;
            if (GeNaSpawner != null)
            {
                var palette = GeNaSpawner.Palette;
                if (palette != null)
                {
                    int id = palette.AddObject(texture2D);
                    if (palette.IsValidID(id))
                    {
                        terrainModifier.AddBrushTexture(texture2D);
                        if (!terrainModifier.BrushTextureIDs.Contains(id))
                            terrainModifier.BrushTextureIDs.Add(id);
                    }
                }
            }
            else
            {
                terrainModifier.AddBrushTexture(texture2D);
            }
            GUI.changed = true;
        }
        protected void RemoveBrushTexture(int index)
        {
            var terrainModifier = Decorator.TerrainModifier;
            var texture2D = terrainModifier.GetBrushTexture(index);
            if (texture2D == null)
                return;
            if (GeNaSpawner != null)
            {
                var palette = GeNaSpawner.Palette;
                if (palette != null)
                {
                    var paletteEntry = palette.GetPaletteEntry(texture2D);
                    if (paletteEntry != null)
                    {
                        int id = paletteEntry.ID;
                        if (terrainModifier.BrushTextureIDs.Contains(id))
                            terrainModifier.BrushTextureIDs.Remove(id);
                    }
                }
            }
            else
            {
                terrainModifier.RemoveBrushTexture(index);
            }
            GUI.changed = true;
        }
        protected override void SettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorUtils.TerrainModifier(Decorator.TerrainModifier, helpEnabled, AddBrushTexture, RemoveBrushTexture);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaTerrainDecorator decorator = (GeNaTerrainDecorator)o;
                    decorator.TerrainModifier.CopyFrom(Decorator.TerrainModifier);
                    EditorUtility.SetDirty(decorator);
                }
            }
        }
    }
}