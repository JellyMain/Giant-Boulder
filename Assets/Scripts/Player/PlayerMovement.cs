using System;
using PlayerInput.Interfaces;
using UnityEngine;
using Zenject;


namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10;
        [SerializeField] private Transform directionPointer;
        public bool canMove;
        private Rigidbody rb;
        private IInput inputService;


        [Inject]
        private void Construct(IInput inputService)
        {
            this.inputService = inputService;
        }


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        

        private void FixedUpdate()
        {
            Vector2 input = inputService.GetNormalizedMoveInput();

            if (input != Vector2.zero && canMove)
            {
                Vector3 inputDirection = new Vector3(input.x, 0, input.y);

                Vector3 worldDirection = directionPointer.TransformDirection(inputDirection);

                Vector3 force = worldDirection * moveSpeed;

                rb.AddForce(force, ForceMode.Force);
            }
        }
    }
}