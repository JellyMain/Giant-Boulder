using System;
using Structures;
using UnityEngine;


namespace Player
{
    public class ObjectsDestroyer : MonoBehaviour
    {
        public delegate void ObjectScoreCollectedHandler(int scoreValue, Vector3 objectPosition);

        public event ObjectScoreCollectedHandler OnObjectScoreCollected;
        public event Action<ObjectType> OnObjectDestroyed;


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out DestructibleObjectBase destructibleObject))
            {
                OnObjectScoreCollected?.Invoke(destructibleObject.ScoreValue, destructibleObject.transform.position);
                OnObjectDestroyed?.Invoke(destructibleObject.ObjectType);
                destructibleObject.Destroy();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out DestructibleObjectBase destructibleObject))
            {
                OnObjectScoreCollected?.Invoke(destructibleObject.ScoreValue, destructibleObject.transform.position);
                OnObjectDestroyed?.Invoke(destructibleObject.ObjectType);
                destructibleObject.Destroy();
            }
        }
    }
}
