using Progress;
using UnityEngine;
using Zenject;


namespace UI
{
    public class SaveButton : MonoBehaviour
    {
        private SaveLoadService saveLoadService;
        
        
        [Inject]
        private void Construct(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }
        
        
        public void Save()
        {
            Debug.Log("Saved");
            saveLoadService.SaveProgress();
        }
    }
}
