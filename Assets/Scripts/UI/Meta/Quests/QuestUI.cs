using System;
using Progress;
using Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Meta.Quests
{
    public class QuestUI : MonoBehaviour, IProgressUpdater
    {
        private Quest quest;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image progressFill;
        private int collectedAmount;


        public void SetQuest(Quest quest)
        {
            this.quest = quest;
        }
        

        private void Start()
        {
            titleText.text = quest.questTitle;
        }


        private void OnEnable()
        {
            UpdateFill();
        }


        private void UpdateFill()
        {
            float normalizedProgress = (float)collectedAmount / quest.targetCoinsAmount;
            progressFill.fillAmount = normalizedProgress;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            if (playerProgress.questsData.questsIdProgressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
            {
                collectedAmount = questProgress.collectedCoins;
            } 
            
        }
    }
}
