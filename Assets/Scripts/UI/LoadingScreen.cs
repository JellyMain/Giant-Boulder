using System.Collections;
using UnityEngine;


namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration;
        [SerializeField] private CanvasGroup canvasGroup;


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }


        public void Show()
        {
            canvasGroup.alpha = 1;
        }

    
        public void Hide()
        {
            StartCoroutine(FadeOut());
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
        }
    }
}
