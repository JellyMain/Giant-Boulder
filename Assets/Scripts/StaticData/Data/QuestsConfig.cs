using System.Collections.Generic;
using Quests;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/QuestsConfig", fileName = "QuestsConfig")]
    public class QuestsConfig: ScriptableObject
    {
        public List<Quest> quests;
    }
}
