using System.Collections.Generic;
using System.Threading.Tasks;
using Const;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Factories;
using Progress;
using Quests;
using Quests.Enums;
using UnityEngine;
using Zenject;


namespace UI.Meta.Quests
{
    public class QuestsWindow : WindowBase
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private RectTransform questSpawnPoint;
        [SerializeField] private List<RectTransform> spawnedQuestsEndPoints;
        [SerializeField] private RectTransform removedQuestEndPoint;
        private QuestsService questsService;
        private MetaUIFactory metaUIFactory;
        private PersistentPlayerProgress persistentPlayerProgress;
        private List<QuestUI> initializedQuestsUI;


        [Inject]
        private void Construct(QuestsService questsService, MetaUIFactory metaUIFactory,
            PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.questsService = questsService;
            this.metaUIFactory = metaUIFactory;
        }


        protected override void OnStart()
        {
            base.OnStart();
            SetCamera();
            InitializeQuests().Forget();
        }


        private async UniTaskVoid InitializeQuests()
        {
            QuestsIdProgressDictionary questProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            initializedQuestsUI = new List<QuestUI>(3);

            int index = 0;

            foreach (QuestData quest in questsService.SelectedQuests.Keys)
            {
                RectTransform endPosition = spawnedQuestsEndPoints[index];
                QuestUI questUI = await InitQuestUI(quest);
                AnimateAppear(questUI, endPosition);
                initializedQuestsUI.Add(questUI);
                index++;
                
                
                await UniTask.WaitForSeconds(1);
            }

            await ReplaceCompletedQuests(questProgressDictionary);
        }


        private async UniTask ReplaceCompletedQuests(QuestsIdProgressDictionary questProgressDictionary)
        {
            List<QuestUI> initializedQuestsUICopy = new List<QuestUI>(initializedQuestsUI);

            foreach (QuestUI questUI in initializedQuestsUICopy)
            {
                int questId = questUI.QuestData.uniqueId;

                QuestProgress questProgress = questProgressDictionary.GetValueOrDefault(questId);

                if (questProgress?.questState == QuestState.JustCompleted)
                {
                    QuestData questData = questsService.ReplaceQuest(questUI.QuestData);
                    QuestUI newQuestUI = await InitQuestUI(questData);
                    initializedQuestsUI.Remove(questUI);
                    initializedQuestsUI.Add(newQuestUI);
                    AnimateRemove(questUI);
                }
            }
        }


        private async Task<QuestUI> InitQuestUI(QuestData quest)
        {
            QuestUI spawnedQuestUI = await metaUIFactory.CreateQuestUI(questsContainer);
            spawnedQuestUI.transform.position = questSpawnPoint.position;
            spawnedQuestUI.SetQuestData(quest);
            return spawnedQuestUI;
        }


        private void AnimateAppear(QuestUI questUI, RectTransform endPoint)
        {
            questUI.transform.DOMove(endPoint.position, 2);
        }


        private void AnimateRemove(QuestUI questUI)
        {
            questUI.transform.DOMoveX(removedQuestEndPoint.position.x, 2).OnComplete(() => Destroy(questUI.gameObject));
        }



        private void SetCamera()
        {
            Camera uiCamera = GameObject.FindGameObjectWithTag(RuntimeConstants.Tags.UI_CAMERA).GetComponent<Camera>();
            canvas.worldCamera = uiCamera;
        }


        protected override void CloseWindow()
        {
            Destroy(gameObject);
        }
    }
}
