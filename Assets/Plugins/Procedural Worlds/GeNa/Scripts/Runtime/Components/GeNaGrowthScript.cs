using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Base growth class - can be assigned to a game object to cause it to grow over a given range.
    /// To use, attach it to a prefab and set it up to run in the way that works best for you. The script can 
    /// disable itself after the game object has finished growing in order to conserve cpu etc.
    /// Implemeneted as virtuals so that you can derive more sophisticated behavior from it.
    /// </summary>
    public sealed class GeNaGrowthScript : MonoBehaviour
    {
        #region Variables
        [Range(0.1f, 2f), Tooltip("The start scale in the game.")]
        public float m_startScale = 0.15f;
        [Range(0.1f, 2f), Tooltip("The end scale in the game.")]
        public float m_endScale = 1.0f;
        [Range(0f, 2f), Tooltip("Scale variance. Final scale is equal to end scale plus a a random value between 0 and this.")]
        public float m_scaleVariance = 0.25f;
        [Tooltip("The time it takes to grow in seconds.")]
        public float m_growthTime = 5.0f;
        [Tooltip("The time the object will live for after it has finished growing in seconds.")]
        public float m_lifeTime = 30f;
        [Tooltip("Disable the script at the end.")]
        public bool m_disableScriptAtEndOfLife = true;
        [Tooltip("Destroy the object at the end of its living time.")]
        public bool m_destroyObjectAtEndOfLife = false;
        /// <summary>
        /// The actual end scale being used
        /// </summary>
        private float m_actualEndScale = 0f;
        #endregion
        #region Methods
        /// <summary>
        /// Initialise this agent.
        /// </summary>
        public void Initialise()
        {
            //Randomly choose an end scale
            m_actualEndScale = m_endScale + Random.Range(0f, m_scaleVariance);
            //Update the scale of the agent
            StartCoroutine(Grow());
        }
        /// <summary>
        /// Grow this agent in a co-routine. Its a one shot thing.
        /// </summary>
        public IEnumerator Grow()
        {
            //Set growth params
            float scale;
            float startTime = Time.realtimeSinceStartup;
            float currentTime = startTime;
            float deltaScale = m_actualEndScale - m_startScale;
            float finishTime = startTime + m_growthTime;
            while (currentTime < finishTime)
            {
                //Update scale
                scale = 1f - ((finishTime - currentTime) / m_growthTime);
                scale = m_startScale + (scale * deltaScale);
                //Apply it to the game object
                gameObject.transform.localScale = Vector3.one * scale;
                yield return null;
                //yield return new WaitForSeconds(0.1f); // Can use this to lessen impact on fps
                //Update time
                currentTime = Time.realtimeSinceStartup;
            }
            if (m_lifeTime > 0f)
                yield return new WaitForSeconds(m_lifeTime);
            if (m_destroyObjectAtEndOfLife)
                Die();
            else if (m_disableScriptAtEndOfLife)
                enabled = false;
        }
        /// <summary>
        /// Kill this instance.
        /// </summary>
        public void Die() => Destroy(gameObject, 0.25f);
        #endregion
    }
}