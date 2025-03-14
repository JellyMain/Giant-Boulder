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
        [SerializeField] private GameObject fragmentsRoot;
        private RayfireBomb rayfireBomb;
        private Rigidbody rb;
        private MeshRenderer meshRenderer;
        private Collider col;
        private bool isDestroyed;
        private ScoreTracker scoreTracker;
        public event Action<int, Vector3> OnScoreCollected;



        [Inject]
        private void Construct(ScoreTracker scoreTracker)
        {
            this.scoreTracker = scoreTracker;
        }


        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
            col = GetComponent<Collider>();
        }


        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     Destroy();
            // }
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
            if (!isDestroyed)
            {
                isDestroyed = true;

                rb.isKinematic = true;
                meshRenderer.enabled = false;
                col.enabled = false;

                fragmentsRoot.SetActive(true);
                rayfireBomb.Explode(0);
            }
        }
    }
}
