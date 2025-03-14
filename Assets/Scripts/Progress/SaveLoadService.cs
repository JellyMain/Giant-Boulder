using System.Collections.Generic;
using Const;
using DataTrackers;
using UnityEngine;
using Utils;
using Zenject;


namespace Progress
{
    public class SaveLoadService: IInitializable
    {
        public List<IProgressUpdater> globalProgressUpdaters = new List<IProgressUpdater>();
        public List<IProgressSaver> globalProgressSavers = new List<IProgressSaver>();
        public List<IProgressUpdater> sceneProgressUpdaters = new List<IProgressUpdater>();
        public List<IProgressSaver> sceneProgressSavers = new List<IProgressSaver>();
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private readonly CurrencyTracker currencyTracker;



        public SaveLoadService(PersistentPlayerProgress persistentPlayerProgress, CurrencyTracker currencyTracker)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.currencyTracker = currencyTracker;
        }
        
        
        public void Initialize()
        {
            RegisterGlobalService(currencyTracker);
        }


        private void RegisterGlobalService<T>(T service)
        {
            if (service is IProgressSaver progressSaver)
            {
                globalProgressSavers.Add(progressSaver);
            }

            if (service is IProgressUpdater progressUpdater)
            {
                globalProgressUpdaters.Add(progressUpdater);
            }
        }


        public void Cleanup()
        {
            sceneProgressSavers.Clear();
            sceneProgressUpdaters.Clear();
        }

        
        public void SaveProgress()
        {
            foreach (IProgressSaver progressSaver in globalProgressSavers)
            {
                progressSaver.SaveProgress(persistentPlayerProgress.PlayerProgress);
            }
            
            foreach (IProgressSaver progressSaver in sceneProgressSavers)
            {
                progressSaver.SaveProgress(persistentPlayerProgress.PlayerProgress);
            }

            PlayerPrefs.SetString(RuntimeConstants.PlayerProgressKeys.PLAYER_PROGRESS_KEY,
                persistentPlayerProgress.PlayerProgress.ToJson());
        }



        public void UpdateProgress()
        {
            foreach (IProgressUpdater progressUpdater in globalProgressUpdaters)
            {
                progressUpdater.UpdateProgress(persistentPlayerProgress.PlayerProgress);
            }

            foreach (IProgressUpdater progressUpdater in sceneProgressUpdaters)
            {
                progressUpdater.UpdateProgress(persistentPlayerProgress.PlayerProgress);
            }
        }


        public PlayerProgress LoadProgress()
        {
            return PlayerPrefs.GetString(RuntimeConstants.PlayerProgressKeys.PLAYER_PROGRESS_KEY)
                ?.ToDeserialized<PlayerProgress>();
        }


       
    }
}
