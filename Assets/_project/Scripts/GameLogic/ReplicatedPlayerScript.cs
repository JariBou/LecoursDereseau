using System;
using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class ReplicatedPlayerScriptBase : MonoBehaviour
    {
        public float speed = 2.5f;
        public bool isGrounded;
        protected bool pendingJump = false;
        public float jumpValue = 10f;
        protected float currentJump = 0f;
        
        protected Rigidbody2D rb;
        [SerializeField] protected Animator animator;
        
        public Vector3 GetPos()
        {
            return transform.position;
        }

        public Vector3 GetSpeed()
        {
            return rb.linearVelocity;
        }

        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetSpeed(Vector3 speed)
        {
            rb.linearVelocity = speed;
        }

        public void Attack()
        {
            animator.SetTrigger("Attack");
        }
        
        public void Hurt()
        {
            animator.SetTrigger("Hurt");
        }

        private void Update()
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            transform.rotation = Quaternion.Euler(0, rb.linearVelocityX < 0 ? 180 : 0, 0);
        }

        public void ApplyInput(PlayerInput playerInput)
        {
            if (playerInput.Attack)
            {
                Attack();
            }
        }
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class ReplicatedPlayerScript : ReplicatedPlayerScriptBase
    {
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyInputs(PlayerInput input)
        {
            if (input.MoveLeft)
            {
                rb.linearVelocityX = -speed;
            } else  if (input.MoveRight)
            {
                rb.linearVelocityX = speed;
            }

            if (input.Jump)
            {
                rb.linearVelocityY = jumpValue;
            }

            if (input.Attack)
            {
                Attack();
            }
        }

       
    }
}