using System;
using UnityEngine;


namespace TerrainGenerator
{
    [Serializable]
    public class TerrainRegion
    {
        public string name;
        public Color regionColor;
        [Range(0, 1)] public float regionHeight;
    }
}