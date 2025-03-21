using System;
using UI;


namespace GameLoop
{
    public class LevelCreationWatcher
    {
        private readonly LoadingScreen loadingScreen;
        public event Action OnLevelCreated;


        public LevelCreationWatcher(LoadingScreen loadingScreen)
        {
            this.loadingScreen = loadingScreen;
        }


        public void MapGenerationStarted()
        {
            loadingScreen.SetLoadingStageText("Generating Map");
        }


        public void UICreationStarted()
        {
            loadingScreen.SetLoadingStageText("Creating UI");
        }


        public void CameraSetStarted()
        {
            loadingScreen.SetLoadingStageText("Setting Cameras");
        }


        public void StructuresAssignmentStarted()
        {
            loadingScreen.SetLoadingStageText("Assigning Structures");
        }

        
        public void LevelCreated()
        {
            OnLevelCreated?.Invoke();
        }
    }
}
