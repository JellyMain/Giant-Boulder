using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/GameConfig", fileName = "GameConfig")]
    public class GameConfig : SerializedScriptableObject
    {
        public float maxTimerTime = 10;
        public int maxScaleScore = 2000;
        public float superBoulderTime = 5;
        public int maxScoreForObject = 5000;
        public Dictionary<int, RageScaleStageMultipliersPair> rageScaleStageMultiplierPointsMap;
    }
}