using Coins;
using Const;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Progress;
using Quests;
using Scenes;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using Zenject;


namespace Cheats
{
    public class CheatConsole : MonoBehaviour
    {
        private SaveLoadService saveLoadService;
        private CurrencyService currencyService;
        private SceneLoader sceneLoader;
        private GameStateMachine gameStateMachine;
        private QuestsService questsService;



        [Inject]
        private void Construct(SaveLoadService saveLoadService, CurrencyService currencyService,
            SceneLoader sceneLoader, GameStateMachine gameStateMachine, QuestsService questsService)
        {
            this.saveLoadService = saveLoadService;
            this.currencyService = currencyService;
            this.sceneLoader = sceneLoader;
            this.gameStateMachine = gameStateMachine;
            this.questsService = questsService;
        }
        
        


#if UNITY_EDITOR


        [Button]
        private void GiveCoins(int coins)
        {
            currencyService.AddCoins(coins);
            saveLoadService.SaveProgress();
        }


        [Button]
        private void EndGame()
        {
            saveLoadService.SaveProgress();
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, () => gameStateMachine.Enter<LoadMetaState>());
        }


        [Button]
        private void CompleteQuest(string questId)
        {
            foreach (var quest in questsService.ActiveQuestsProgressUpdaters.Keys)
            {
                if (quest.questId == questId)
                {
                    questsService.ActiveQuestsProgressUpdaters[quest].isCompleted = true;
                }
            }
        }

#endif
    }
}
