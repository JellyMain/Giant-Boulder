using System;
using Const;
using Cysharp.Threading.Tasks;
using Factories;
using Quests;
using UI.Meta.Quests;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;


namespace UI.Meta
{
    public class QuestsWindow : WindowBase
    {
        [SerializeField] private Transform questsContainer;
        [SerializeField] private Canvas canvas;
        private QuestService questService;
        private MetaUIFactory metaUIFactory;



        [Inject]
        private void Construct(QuestService questService, MetaUIFactory metaUIFactory)
        {
            this.questService = questService;
            this.metaUIFactory = metaUIFactory;
        }


        protected override void OnStart()
        {
            base.OnStart();
            SetCamera();
            InitQuests().Forget();
        }


        private async UniTaskVoid InitQuests()
        {
            foreach (QuestData quest in questService.CurrentQuests)
            {
                QuestUI spawnedQuestUI = await metaUIFactory.CreateQuestUI(questsContainer);
                spawnedQuestUI.SetQuestData(quest);
            }
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
