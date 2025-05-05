using System;
using UnityEngine;
using Zenject;


namespace Progress
{
    public class SessionSaveService : MonoBehaviour
    {
        private SaveLoadService saveLoadService;
        
        
        [Inject]
        private void Construct(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        private void OnApplicationQuit()
        {
            saveLoadService.SaveProgress();
            //TODO: Modify for IOS
        }
    }
}
