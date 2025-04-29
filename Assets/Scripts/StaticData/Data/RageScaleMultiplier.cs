using System;
using UnityEngine;


namespace StaticData.Data
{
    [Serializable]
    public class RageScaleMultiplier
    {
        public int multiplier;
        [Range(0, 1)] public float normalizedPointOnScale;
    }
}
