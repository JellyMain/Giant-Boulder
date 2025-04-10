using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/GameConfig", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public float maxTime = 10;
    }
}