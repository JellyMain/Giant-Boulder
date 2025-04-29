using System;
using UnityEngine;


namespace StaticData.Data
{
    [Serializable]
    public class ScoreAnimations
    {
        public float scoreMinDisappearTime = 0.4f;
        public float scoreMaxDisappearTime = 0.8f;
        public float scoreMinScale = 2;
        public float scoreMaxScale = 10;
        public float maxScoreTextRotation = 45;
        public float minScoreTextRotation = -45;
        public float spawnedTextAppearTime = 0.3f;
        public float scoreTextScalePunchScaleTime = 0.3f;
        public Gradient scoreGradient;
    }
}
