using System.Collections.Generic;
using Quests;
using Sirenix.OdinInspector;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/QuestsConfig", fileName = "QuestsConfig")]
    public class QuestsConfig : ScriptableObject
    {
        [AssetList(Path = "StaticData/Quests", AutoPopulate = true)]
        public List<QuestDataBase> quests;
    }
}
