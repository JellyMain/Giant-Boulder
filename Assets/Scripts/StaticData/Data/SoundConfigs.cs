using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu (menuName = "StaticData/SoundsConfig", fileName = "SoundsConfig")]
    public class SoundConfigs : ScriptableObject
    {
        public SoundSettings missileExplosionSound;
        public SoundSettings coinCollectedSound;
        public SoundSettings destroyTreeSound;
    }
}