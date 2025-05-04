using System;
using Cysharp.Threading.Tasks;
using DataTrackers;
using DG.Tweening;
using Factories;
using GameLoop;
using Quests;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace UI.Gameplay
{
    public class QuestsPopupWindowUI : MonoBehaviour
    {
        [SerializeField] private RectTransform popupSpawnPoint;
        [SerializeField] private RectTransform popupEndPoint;
        private GameplayUIFactory gameplayUIFactory;
        private QuestsPopupAnimations questsPopupAnimations;
        private GameplayQuestTracker gameplayQuestTracker;


        [Inject]
        private void Construct(GameplayUIFactory gameplayUIFactory, StaticDataService staticDataService,
            GameplayQuestTracker gameplayQuestTracker)
        {
            this.gameplayQuestTracker = gameplayQuestTracker;
            this.gameplayUIFactory = gameplayUIFactory;
            questsPopupAnimations = staticDataService.AnimationsConfig.questsPopupAnimations;
        }




        private void OnDisable()
        {
            UnsubscribeFromTrackedQuests();
        }


        private void Start()
        {
            ShowActiveQuest();
            SubscribeOnTrackedQuests();
        }


        private void SubscribeOnTrackedQuests()
        {
            foreach (QuestProgressUpdater questProgressUpdater in gameplayQuestTracker.TrackedQuests.Keys)
            {
                questProgressUpdater.OnQuestCompleted += OnQuestCompleted;
            }
        }


        private void UnsubscribeFromTrackedQuests()
        {
            foreach (QuestProgressUpdater questProgressUpdater in gameplayQuestTracker.TrackedQuests.Keys)
            {
                questProgressUpdater.OnQuestCompleted -= OnQuestCompleted;
            }
        }


        private void OnQuestCompleted(QuestProgressUpdater questProgressUpdater)
        {
            QuestDataBase questData = gameplayQuestTracker.TrackedQuests[questProgressUpdater];
            ShowCompletedQuest(questData);
        }


        private async void ShowActiveQuest()
        {
            QuestPopupUI questPopupUI = await gameplayUIFactory.CreateQuestPopupUI(transform, popupSpawnPoint.position);
            questPopupUI.InitMainQuest();

            AnimatePopup(questPopupUI);
        }


        private async void ShowCompletedQuest(QuestDataBase completedQuest)
        {
            QuestPopupUI questPopupUI = await gameplayUIFactory.CreateQuestPopupUI(transform, popupSpawnPoint.position);
            questPopupUI.InitWithCompletedQuest(completedQuest);
            AnimatePopup(questPopupUI);
        }


        private void AnimatePopup(QuestPopupUI questPopupUI)
        {
            Sequence sequence = DOTween.Sequence();
            
            sequence.Append(questPopupUI.transform.DOMove(popupEndPoint.position, questsPopupAnimations.appearTime)
                .SetEase(Ease.InOutElastic));
            sequence.AppendInterval(questsPopupAnimations.pauseTime);
            sequence.Append(
                questPopupUI.transform.DOMove(popupSpawnPoint.position, questsPopupAnimations.disappearTime)
                    .SetEase(Ease.InExpo));
            sequence.OnComplete(() => Destroy(questPopupUI.gameObject));
        }
    }
}
