using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Meta
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        protected CancellationToken cancellationToken;

        private void Start()
        {
            closeButton.onClick.AddListener(CloseWindow);
            cancellationToken = this.GetCancellationTokenOnDestroy();
            OnStart();
        }


        protected virtual void OnStart(){}
        
        
        protected virtual void CloseWindow()
        {
            Destroy(gameObject);
        }
    }
}
