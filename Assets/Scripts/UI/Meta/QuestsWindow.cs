using System;
using Quests;
using UI.Meta.Quests;
using UnityEngine;
using Zenject;


namespace UI.Meta
{
    public class QuestsWindow: WindowBase
    {
        [SerializeField] private GameObject windowOverlay;
        [SerializeField] private Transform questsContainer;
        public Transform QuestsContainer => questsContainer;
        public GameObject WindowOverlay => windowOverlay;
        
        
        protected override void CloseWindow()
        {
            windowOverlay.SetActive(false);
        }

        
    }
}
