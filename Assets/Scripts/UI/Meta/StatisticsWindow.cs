using System.Collections.Generic;
using System.Linq;
using Progress;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;


namespace UI.Meta
{
    public class StatisticsWindow : WindowBase
    {
        [SerializeField] private List<StatisticsUIObject> statisticsUIObjects;
        [SerializeField] private TMP_Text totalStructuresDestroyedAmount;
        private PersistentPlayerProgress persistentPlayerProgress;


        [Inject]
        private void Construct(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


#if UNITY_EDITOR


        [Button]
        private void FindStatisticsUIObjects()
        {
            statisticsUIObjects = new List<StatisticsUIObject>();

            StatisticsUIObject[] statisticsUIObjectsArray = GetComponentsInChildren<StatisticsUIObject>();

            statisticsUIObjects = statisticsUIObjectsArray.ToList();
        }


#endif


        protected override void OnStart()
        {
            InitStatistics();
        }


        private void InitStatistics()
        {
            int totalAmount = 0;

            foreach (StatisticsUIObject statisticsObject in statisticsUIObjects)
            {
                int amount =
                    persistentPlayerProgress.PlayerProgress.statsData.destroyedObjectsCount.GetValueOrDefault(
                        statisticsObject.ObjectType, 0);
                
                statisticsObject.AmountText.text = amount.ToString();

                totalAmount += amount;
            }

            totalStructuresDestroyedAmount.text = totalAmount.ToString();
        }

 
        protected override void CloseWindow()
        {
            Destroy(gameObject);
        }
    }
}
