using System;
using UnityEngine;


namespace Coins
{
    public class Coin : MonoBehaviour
    {
         
        [SerializeField] private Rigidbody rb;
        public Rigidbody Rb => rb;


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                rb.isKinematic = true;
            }
        }
    }
}