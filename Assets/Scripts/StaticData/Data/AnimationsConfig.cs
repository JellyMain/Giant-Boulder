using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/AnimationsConfig", fileName = "AnimationsConfig")]
    public class AnimationsConfig : ScriptableObject
    {
        public MainMenuAnimations mainMenuAnimations;
        public RageScaleAnimations rageScaleAnimations;
        public ScoreAnimations scoreAnimations;
        public QuestsPopupAnimations questsPopupAnimations;
    }
}