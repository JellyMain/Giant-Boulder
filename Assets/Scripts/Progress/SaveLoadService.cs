using System.Collections.Generic;
using Const;
using UnityEngine;
using Utils;


namespace Progress
{
    public class SaveLoadService
    {
        private readonly List<IProgressUpdater> globalProgressUpdaters = new List<IProgressUpdater>();
        private readonly List<IProgressSaver> globalProgressSavers = new List<IProgressSaver>();
        private readonly List<IProgressUpdater> sceneProgressUpdaters = new List<IProgressUpdater>();
        private readonly List<IProgressSaver> sceneProgressSavers = new List<IProgressSaver>();
        private readonly PersistentPlayerProgress persistentPlayerProgress;



        public SaveLoadService(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }
        
        

        public void RegisterGlobalObject<T>(T service)
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


        public void RegisterSceneObject<T>(T service)
        {
            if (service is IProgressSaver progressSaver)
            {
                sceneProgressSavers.Add(progressSaver);
                Debug.Log($"Savers: {sceneProgressSavers.Count}" );
            }

            if (service is IProgressUpdater progressUpdater)
            {
                sceneProgressUpdaters.Add(progressUpdater);
                Debug.Log($"Updaters: {sceneProgressUpdaters.Count}" );
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
