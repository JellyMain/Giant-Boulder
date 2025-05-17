using System.Collections.Generic;
using Quests;
using Quests.Enums;
using Sirenix.OdinInspector;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/QuestsConfig", fileName = "QuestsConfig")]
    public class QuestsConfig : SerializedScriptableObject
    {
        public Dictionary<QuestType, List<QuestData>> quests;



#if UNITY_EDITOR


        [Button]
        private void SetQuests()
        {
            quests = new Dictionary<QuestType, List<QuestData>>();

            string folderPath = "Assets/StaticData/Quests";

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(QuestData)}", new[] { folderPath });
            QuestData[] allQuests = new QuestData[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                allQuests[i] = AssetDatabase.LoadAssetAtPath<QuestData>(assetPath);
            }


            foreach (QuestData questData in allQuests)
            {
                if (!quests.ContainsKey(questData.questType))
                {
                    quests[questData.questType] = new List<QuestData>();
                }

                quests[questData.questType].Add(questData);
            }
        }


#endif
    }
}
