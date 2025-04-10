using System;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Meta
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Button closeButton;


        private void Start()
        {
            closeButton.onClick.AddListener(CloseWindow);
        }
        
        
        protected virtual void CloseWindow()
        {
            Destroy(gameObject);
        }
    }
}
