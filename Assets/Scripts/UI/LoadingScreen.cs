using System.Collections;
using TMPro;
using UnityEngine;


namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text loadingStageText;


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }


        public void Show()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }

    
        public void Hide()
        {
            StartCoroutine(FadeOut());
        }



        public void SetLoadingStageText(string text)
        {
            loadingStageText.text = text;
        }
    
    
        private IEnumerator FadeOut()
        {
            float elapsedTime = 0;

            while (elapsedTime < fadeDuration)
            {
                float t = elapsedTime / fadeDuration;

                canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                elapsedTime += Time.deltaTime;
            
                yield return null;
            }
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
