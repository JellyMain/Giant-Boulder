using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;


namespace UI
{
    public class SceneLoader
    {
        private readonly LoadingScreen loadingScreen;


        public SceneLoader(LoadingScreen loadingScreen)
        {
            this.loadingScreen = loadingScreen;
        }


        public void Load(string sceneName, Action callback = null)
        {
            LoadScene(sceneName, callback).Forget();
        }


        private async UniTaskVoid LoadScene(string sceneName, Action callback)
        {
            loadingScreen.Show();

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