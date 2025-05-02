using System;
using UnityEngine;


namespace UI
{
    public class UIObjectRotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10;
        
        
        private void Update()
        {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }
}
