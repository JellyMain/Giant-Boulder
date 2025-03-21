using System;
using DataTrackers;
using RayFire;
using Structures;
using UnityEngine;
using Zenject;


namespace Player
{
    public class ObjectsDestroyer : MonoBehaviour
    {
        private ScoreTracker scoreTracker;
        public event Action<int, Vector3> OnScoreCollected;



        [Inject]
        private void Construct(ScoreTracker scoreTracker)
        {
            this.scoreTracker = scoreTracker;
        }
        

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out DestructibleObjectBase structure))
            {
                OnScoreCollected?.Invoke(structure.ScoreValue, structure.transform.position);
                structure.Destroy();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out DestructibleObjectBase structure))
            {
                scoreTracker.AddScore(structure.ScoreValue);
                OnScoreCollected?.Invoke(structure.ScoreValue, structure.transform.position);
                structure.Destroy();
            }
        }


        private void Destroy()
        {
            
        }
    }
}
