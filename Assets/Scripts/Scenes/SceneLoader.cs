using System;
using Cysharp.Threading.Tasks;
using Progress;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Scenes
{
    public class SceneLoader
    {
        private readonly LoadingScreen loadingScreen;
        private readonly SaveLoadService saveLoadService;


        public SceneLoader(LoadingScreen loadingScreen, SaveLoadService saveLoadService)
        {
            this.loadingScreen = loadingScreen;
            this.saveLoadService = saveLoadService;
        }


        public void Load(string sceneName, Action callback = null)
        {
            LoadScene(sceneName, callback).Forget();
        }


        private async UniTaskVoid LoadScene(string sceneName, Action callback)
        {
            loadingScreen.Show();
            saveLoadService.Cleanup();

            AsyncOperation loadNextScene = SceneManager.LoadSceneAsync(sceneName);

            while (!loadNextScene.isDone)
            {
                await UniTask.Yield();
            }
            
            callback?.Invoke();

            loadingScreen.Hide();
        }
    }
}