using System;
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
        private QuestService questService;
        

        [Inject]
        private void Construct(QuestService questService)
        {
            this.questService = questService;
        }

        
        public void InitMainQuest()
        {
            QuestDataBase questData = questService.SelectedQuests[0];
            questDescriptionText.text = questData.questDescription;
        }


        public void InitWithCompletedQuest(QuestDataBase completedQuest)
        {
            icon.gameObject.SetActive(false);
            completedIcon.gameObject.SetActive(true);
            questDescriptionText.text = completedQuest.questDescription;
        }
        
        
    }
}
