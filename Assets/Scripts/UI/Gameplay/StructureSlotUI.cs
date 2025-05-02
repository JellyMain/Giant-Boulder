using System;
using Quests;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
    public class StructureSlotUI : MonoBehaviour
    {
        [SerializeField] private QuestData questDataToUnlock;
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
