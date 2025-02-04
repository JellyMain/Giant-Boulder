using System.Collections;
using UnityEngine;

namespace GeNa.Core
{
    [ExecuteAlways] // Have this work in the Editor as well as Runtime
    public class GeNaGrowthDecorator : GeNaDecorator
    {
        [SerializeField] protected bool m_scaleToGameObject = true;
        [SerializeField] protected float m_speed = 5f;
        [SerializeField] protected Vector3 m_startScale = Vector3.zero;
        [SerializeField] protected Vector3 m_endScale = Vector3.one;
        private Vector3 m_gameObjectScale = Vector3.one;

        public bool ScaleToGameObject
        {
            get => m_scaleToGameObject;
            set => m_scaleToGameObject = value;
        }

        public float Speed
        {
            get => m_speed;
            set => m_speed = value;
        }

        public Vector3 StartScale
        {
            get => m_startScale;
            set => m_startScale = value;
        }

        public Vector3 EndScale
        {
            get => m_endScale;
            set => m_endScale = value;
        }

// Called when Decorator is Ingested into GeNa
        public override void OnIngest(Resource resource)
        {
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        // Scales the transform using lerp
        public IEnumerator Scale(float time)
        {
            try
            {
                if (transform == null)
                    yield break;
            }
            catch (MissingReferenceException)
            {
                yield break;
            }

            // Change the Scale of the Transform
            if (ScaleToGameObject)
            {
                transform.localScale = Vector3.Lerp(m_startScale, m_gameObjectScale, time);
            }
            else
            {
                transform.localScale = Vector3.Lerp(m_startScale, m_endScale, time);
            }

            // Wait till the frame finishes rendering to see the changes
            yield return new WaitForEndOfFrame();
        }

        // IEnumerator Method for Growth of the Transform's Scale.
        public IEnumerator Grow()
        {
            // Start scale at time 0
            yield return Scale(0.0f);
            // Start time at 0
            float time = 0.0f;
            while (time < 1.0f)
            {
                // Increment time by Delta.
                time += Speed * Time.deltaTime;
                // Test Delta to see if it passes 1
                if (time >= 1.0f)
                    time = 1.0f; // Force time to scale to 1
                // Pass time to Scale method
                yield return Scale(time);
            }
        }

        // Runs once this Decorator is Spawned
        public override IEnumerator OnSelfSpawned(Resource resource)
        {
            //Sets the gameobject scale
            m_gameObjectScale = gameObject.transform.localScale;
            // Special method that performs a Coroutine at Edit mode and Runtime
            GeNaEvents.StartCoroutine(Grow(), this);
            // Alternatively, you can yield the Grow process directly (note: this halts spawn process in order to perform spawns).
            // yield return Grow();

            // Skips a frame
            // yield return null;
            // Continues to next process without skipping a frame
            yield break;
        }

        // Runs directly after Spawning Children Decorators
        public override void OnChildrenSpawned(Resource resource)
        {
        }

        // Runs when Spawner has requested to load references from a Palette
        public override void LoadReferences(Palette palette)
        {
        }
    }
}