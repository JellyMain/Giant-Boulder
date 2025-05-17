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
using UnityEngine.UI;
using Zenject;


namespace UI.Meta.Quests
{
    public class QuestsWindow : WindowBase
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Button dailyQuestsButton;
        [SerializeField] private Button mainQuestsButton;
        [SerializeField] private Button sideQuestsButton;
        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private RectTransform questSpawnPoint;
        [SerializeField] private List<RectTransform> spawnedQuestsEndPoints;
        [SerializeField] private RectTransform removedQuestEndPoint;
        private QuestsService questsService;
        private MetaUIFactory metaUIFactory;
        private PersistentPlayerProgress persistentPlayerProgress;
        private List<QuestUI> initializedQuestsUI;
        private QuestType currentSection = QuestType.MainQuest;



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
            SetSectionButtons();
            SetCamera();
            InitializeQuests(QuestType.MainQuest).Forget();
        }


        private void SetSectionButtons()
        {
            mainQuestsButton.onClick.AddListener(ShowMainQuests);
            sideQuestsButton.onClick.AddListener(ShowSideQuests);
            dailyQuestsButton.onClick.AddListener(ShowDailyQuests);
        }


        private async UniTask InitializeQuests(QuestType questType)
        {
            QuestsIdProgressDictionary questProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            initializedQuestsUI = new List<QuestUI>(3);


            foreach (QuestData quest in questsService.SortedActiveQuests[questType])
            {
                QuestUI questUI = await InitQuestUI(quest);
                questUI.OnQuestClaimed += ReplaceClaimedQuest;
                initializedQuestsUI.Add(questUI);
            }

            await ArrangeQuests();
        }



        private async void ReplaceClaimedQuest(QuestUI oldQuestUI)
        {
            oldQuestUI.OnQuestClaimed -= ReplaceClaimedQuest;
            QuestData newQuestData = questsService.ReplaceQuest(oldQuestUI.QuestData);
            QuestUI newQuestUI = await InitQuestUI(newQuestData);
            initializedQuestsUI.Remove(oldQuestUI);
            initializedQuestsUI.Add(newQuestUI);
            await AnimateRemove(oldQuestUI);

            await ArrangeQuests();
        }





        private async UniTask ArrangeQuests()
        {
            for (int i = 0; i < 3; i++)
            {
                QuestUI questUI = initializedQuestsUI[i];
                Vector3 endPoint = spawnedQuestsEndPoints[i].position;

                questUI.transform.DOMove(endPoint, 0.6f).SetEase(Ease.OutExpo);

                await UniTask.WaitForSeconds(0.1f);
            }
        }


        private async Task<QuestUI> InitQuestUI(QuestData quest)
        {
            QuestUI spawnedQuestUI = await metaUIFactory.CreateQuestUI(questsContainer);
            spawnedQuestUI.transform.position = questSpawnPoint.position;
            spawnedQuestUI.InitQuestData(quest);
            return spawnedQuestUI;
        }


        private async void ShowDailyQuests()
        {
            if (currentSection != QuestType.DailyQuest)
            {
                currentSection = QuestType.DailyQuest;
                await RemovePreviousSectionQuests();
                InitializeQuests(QuestType.DailyQuest).Forget();
            }
        }


        private async void ShowMainQuests()
        {
            if (currentSection != QuestType.MainQuest)
            {
                currentSection = QuestType.MainQuest;
                await RemovePreviousSectionQuests();
                InitializeQuests(QuestType.MainQuest).Forget();
            }
        }


        private async void ShowSideQuests()
        {
            if (currentSection != QuestType.SideQuest)
            {
                currentSection = QuestType.SideQuest;
                await RemovePreviousSectionQuests();
                InitializeQuests(QuestType.SideQuest).Forget();
            }
        }


        private async UniTask RemovePreviousSectionQuests()
        {
            foreach (var questUI in initializedQuestsUI)
            {
                AnimateRemove(questUI);
                await UniTask.WaitForSeconds(0.05f);
            }

            await UniTask.WaitForSeconds(0.15f);
        }



        private async UniTask AnimateRemove(QuestUI questUI)
        {
            await questUI.transform.DOMove(removedQuestEndPoint.position, 0.6f).SetEase(Ease.OutExpo)
                .OnComplete(() => Destroy(questUI.gameObject)).AsyncWaitForCompletion().AsUniTask();
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
