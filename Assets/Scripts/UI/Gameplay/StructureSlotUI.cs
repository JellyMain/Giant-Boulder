using System;
using Quests;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
    public class StructureSlotUI : MonoBehaviour
    {
        [SerializeField] private QuestDataBase questDataBaseToUnlock;
        [SerializeField] private Image lockedQuestImage;
        [SerializeField] private Image unlockedQuestImage;
        

        private void OnEnable()
        {
            Init();
        }


        private void Init()
        {
            
        }

        
    }
}
