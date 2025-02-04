using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeNa.Core
{
    public enum GetMaterialIDType { Name, ID }

    [System.Serializable]
    public class MaterialDecoratorData
    {
        public string m_presetName;
        [Range(0f, 100f)]
        public float m_chanceOf = 100f;
        public List<MaterialMetaData> m_presetMaterials = new List<MaterialMetaData>();
    }
    [System.Serializable]
    public class MaterialMetaData
    {
        [Range(0f, 100f)]
        public float m_chanceOf = 80f;
        public Material m_material;
        public List<MaterialLODMaterialMataData> m_lodMaterials = new List<MaterialLODMaterialMataData>();
    }

    [System.Serializable]
    public class MaterialDecoratorRendererData
    {
        public Dictionary<int, List<int>> m_materialIDs = new Dictionary<int, List<int>>();
        public Dictionary<int, List<Renderer>> m_meshRenderers = new Dictionary<int, List<Renderer>>();
    }

    [System.Serializable]
    public class MaterialLODMaterialMataData
    {
        public int m_lodID = 0;
        public Material m_material;
    }

    public class GeNaMaterialDecoratorDatabase : ScriptableObject
    {
        [HideInInspector]
        public List<string> m_presets = new List<string>();
        public List<MaterialDecoratorData> m_materialData = new List<MaterialDecoratorData>();
        public bool m_assignMaterialIDByName = true;

        [HideInInspector]
        public bool m_overridePreset = false;
        [HideInInspector]
        public int m_selectedPreset = 0;

        public void GetPresets(bool overrideRefresh = false)
        {
            if (m_materialData.Count > 0)
            {
                if (m_presets.Count - 1 == m_materialData.Count && !overrideRefresh)
                {
                    return;
                }

                m_presets.Clear();
                m_presets.Add("Chance Of");
                foreach (MaterialDecoratorData data in m_materialData)
                {
                    if (data.m_presetMaterials.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(data.m_presetName))
                        {
                            m_presets.Add(data.m_presetName);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Preset '" + data.m_presetName + "' has no materials assigned. This preset will not be added to the dropdown list of available presets. Please assign a material in the profile and click refresh presets");
                    }
                }
            }
        }
        public MaterialMetaData GetPreset(string preset)
        {
            if (m_materialData.Count > 0)
            {
                if (m_overridePreset)
                {
                    if (m_selectedPreset <= m_presets.Count - 1)
                    {
                        MaterialDecoratorData overrideData = m_materialData[m_selectedPreset - 1];
                        if (overrideData != null)
                        {
                            return GetMaterialData(overrideData.m_presetMaterials);
                        }
                    }
                }

                foreach (MaterialDecoratorData decoratorData in m_materialData)
                {
                    if (decoratorData.m_presetName.Contains(preset))
                    {
                        return GetMaterialData(decoratorData.m_presetMaterials);
                    }
                }
            }

            return null;
        }
        public MaterialMetaData GetPreset(int preset)
        {
            if (m_materialData.Count > 0)
            {
                if (m_overridePreset)
                {
                    if (m_selectedPreset <= m_presets.Count - 1)
                    {
                        MaterialDecoratorData overrideData = m_materialData[m_selectedPreset - 1];
                        if (overrideData != null)
                        {
                            return GetMaterialData(overrideData.m_presetMaterials);
                        }
                    }
                }

                MaterialDecoratorData decoratorData = m_materialData[preset - 1];
                if (decoratorData != null)
                {
                    return GetMaterialData(decoratorData.m_presetMaterials);
                }
            }

            return null;
        }
        public MaterialMetaData GetByChanceOf()
        {
            if (m_materialData.Count > 0)
            {
                if (m_overridePreset && m_selectedPreset != 0)
                {
                    if (m_selectedPreset <= m_presets.Count - 1)
                    {
                        MaterialDecoratorData overrideData = m_materialData[m_selectedPreset - 1];
                        if (overrideData != null)
                        {
                            return GetMaterialData(overrideData.m_presetMaterials);
                        }

                        return null;
                    }
                }

                MaterialDecoratorData currentData = GetPresetData(m_materialData);
                if (currentData != null)
                {
                    return GetMaterialData(currentData.m_presetMaterials);
                }
            }

            return null;
        }
        public void Apply(MaterialDecoratorRendererData meshData, MaterialMetaData materialData)
        {
            if (meshData != null && materialData != null)
            {
                if (meshData.m_meshRenderers.Count > 0)
                {
                    for (int j = 0; j < meshData.m_meshRenderers.Count; j++)
                    {
                        List<Renderer> renderers = meshData.m_meshRenderers[j];
                        List<int> materialIDs = meshData.m_materialIDs[j];
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            if (renderers.Count == materialIDs.Count)
                            {
                                if (renderers[i] != null)
                                {
                                    Material[] newMaterials = renderers[i].sharedMaterials;
                                    bool hasApplied = false;
                                    if (materialData.m_lodMaterials.Count > 0)
                                    {
                                        for (int k = 0; k < materialData.m_lodMaterials.Count; k++)
                                        {
                                            if (materialData.m_lodMaterials[k].m_lodID == j)
                                            {
                                                newMaterials[materialIDs[i]] = materialData.m_lodMaterials[k].m_material;
                                                hasApplied = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!hasApplied)
                                    {
                                        newMaterials[materialIDs[i]] = materialData.m_material;
                                    }

                                    renderers[i].materials = newMaterials;

                                    MeshBeenProcessed removeProcessed = renderers[i].GetComponent<MeshBeenProcessed>();
                                    if (removeProcessed != null)
                                    {
                                        DestroyImmediate(removeProcessed);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public Dictionary<int, List<int>> GetMaterialIDFromType(Dictionary<int, List<Renderer>> renderers, List<string> materialNames, int ID, GetMaterialIDType type, out Dictionary<int, List<Renderer>> newRenders)
        {
            newRenders = new Dictionary<int, List<Renderer>>();
            Dictionary<int, List<int>> IDs = new Dictionary<int, List<int>>();
            List<int> materialIDs = new List<int>();
            List<string> matNames = new List<string>();
            if (renderers.Count > 0)
            {
                if (materialNames.Count > 0)
                {
                    for (int x = 0; x < renderers.Count; x++)
                    {
                        materialIDs = new List<int>();
                        List<Renderer> lodRenderers = renderers[x];
                        List<Renderer> newLodRenderers = new List<Renderer>();
                        for (int j = 0; j < lodRenderers.Count; j++)
                        {
                            Renderer renderer = lodRenderers[j];
                            switch (type)
                            {
                                case GetMaterialIDType.Name:
                                {
                                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                                    {
                                        foreach (string materialName in materialNames)
                                        {
                                            if (renderer.sharedMaterials[i] != null)
                                            {
                                                if (renderer.sharedMaterials[i].name.Contains(materialName))
                                                {
                                                    if (matNames.Count > 0)
                                                    {
                                                        bool add = true;
                                                        foreach (string matName in matNames)
                                                        {
                                                            if (matName == renderer.sharedMaterials[i].name)
                                                            {
                                                                add = false;
                                                                break;
                                                            }
                                                        }

                                                        if (add)
                                                        {
                                                            matNames.Add(renderer.sharedMaterials[i].name);
                                                            newLodRenderers.Add(renderer);
                                                            materialIDs.Add(i);
                                                        }
                                                        else
                                                        {
                                                            newLodRenderers.Add(renderer);
                                                            matNames.Add(renderer.sharedMaterials[i].name);
                                                            materialIDs.Add(i);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        newLodRenderers.Add(renderer);
                                                        matNames.Add(renderer.sharedMaterials[i].name);
                                                        materialIDs.Add(i);
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                                case GetMaterialIDType.ID:
                                {
                                    if (ID <= renderer.sharedMaterials.Length - 1)
                                    {
                                        materialIDs.Add(ID);
                                        break;
                                    }
                                    break;
                                }
                            }
                        }

                        IDs.Add(x, materialIDs);
                        newRenders.Add(x, newLodRenderers);
                    }
                }
            }

            return IDs;
        }
        private MaterialMetaData GetMaterialData(List<MaterialMetaData> materialDatas)
        {
            float sumOfChances = materialDatas.Sum(x => x.m_chanceOf);
            float border = 0f;
            float random = UnityEngine.Random.Range(0f, sumOfChances);
            foreach (MaterialMetaData data in materialDatas)
            {
                border += data.m_chanceOf;
                if (border >= random)
                {
                    return data;
                }
            }

            return materialDatas[UnityEngine.Random.Range(0, materialDatas.Count - 1)];
        }
        private MaterialDecoratorData GetPresetData(List<MaterialDecoratorData> datas)
        {
            float sumOfChances = datas.Sum(x => x.m_chanceOf);
            float border = 0f;
            float random = UnityEngine.Random.Range(0f, sumOfChances);
            foreach (MaterialDecoratorData data in datas)
            {
                border += data.m_chanceOf;
                if (border >= random)
                {
                    return data;
                }
            }

            return datas[UnityEngine.Random.Range(0, datas.Count - 1)];
        }
    }
}