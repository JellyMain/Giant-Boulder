using System;
using Const;
using DataTrackers;
using DG.Tweening;
using RayFire;
using Sirenix.OdinInspector;
using Sounds;
using StaticData.Data;
using UnityEngine;
using Utils;
using Zenject;


namespace Structures
{
    public abstract class DestructibleObjectBase : MonoBehaviour
    {
        [SerializeField] private ParticleSystem destroyParticlesPrefab;
        [SerializeField] private SoundSettings destroySoundSettings;
        [SerializeField] private GameObject fragmentsRoot;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private ObjectType objectType;
        private MeshRenderer meshRenderer;
        private RayfireBomb rayfireBomb;
        private Collider col;
        private Vector3 objectCenter;
        private SoundPlayer soundPlayer;
        public int ScoreValue => scoreValue;
        public ObjectType ObjectType => objectType;
        public event Action<DestructibleObjectBase> OnDestroyed;
        public event Action<DestructibleObjectBase> OnLightDestroyed;
        public bool IsDestroyed { get; private set; }



        [Inject]
        private void Construct(SoundPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
        }


        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
        }


        private void Start()
        {
            objectCenter = RuntimeMeshUtility.GetMeshCenter(transform, col);
        }



        public void FindFragmentsRoot()
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("FragmentsRoot"))
                {
                    fragmentsRoot = child.gameObject;
                }
            }
        }


        public void DestroyLight()
        {
            Destroy(gameObject);

            OnLightDestroyed?.Invoke(this);
        }


        public void Destroy()
        {
            meshRenderer.enabled = false;
            col.enabled = false;

            fragmentsRoot.SetActive(true);
            rayfireBomb.Explode(0);

            soundPlayer.PlaySound(destroySoundSettings);

            if (destroyParticlesPrefab != null)
            {
                Instantiate(destroyParticlesPrefab, objectCenter, Quaternion.identity);
            }

            IsDestroyed = true;

            OnDestroyed?.Invoke(this);
        }
    }
}
