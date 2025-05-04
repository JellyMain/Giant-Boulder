using System;
using System.Linq;
using Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Gameplay
{
    public class QuestPopupUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image completedIcon;
        [SerializeField] private TMP_Text questDescriptionText;
        private QuestsService questsService;
        

        [Inject]
        private void Construct(QuestsService questsService)
        {
            this.questsService = questsService;
        }

        
        public void InitMainQuest()
        {
            QuestData questData = questsService.SelectedQuests.Keys.First();
            questDescriptionText.text = questData.questDescription;
        }


        public void InitWithCompletedQuest(QuestData completedQuest)
        {
            icon.gameObject.SetActive(false);
            completedIcon.gameObject.SetActive(true);
            questDescriptionText.text = completedQuest.questDescription;
        }
        
        
    }
}
