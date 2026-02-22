using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class ReplicatedPlayerScriptBase : MonoBehaviour
    {
        public float speed = GameConstants.DefaultPlayerSpeed;
        public bool isGrounded;
        protected bool pendingJump = false;
        public float jumpValue = GameConstants.JumpForce;
        protected float currentJump = 0f;
        float CurrentHealth = 0.0f;
        bool isAlive = false;
        bool wasLastMovingRight = true;

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
        
        public void Hurt(Vector3 direction)
        {
            CurrentHealth -= GameConstants.BaseAttackDMG;
            animator.SetTrigger("Hurt");
            float knockbackFullModifier = GameConstants.BaseAttackKnockback;
            if (CurrentHealth > 0)
                knockbackFullModifier += GameConstants.HPKnockBackModifier * CurrentHealth;

            direction *= knockbackFullModifier;
            Vector2 knockbackVector = new Vector2(direction.x, direction.y);

            rb.linearVelocity += knockbackVector;
        }

        private void Update()
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            transform.rotation = Quaternion.Euler(0, wasLastMovingRight ? 0 : 180, 0);
        }

        public void ApplyInput(PlayerInput playerInput)
        {
            if (playerInput.MoveLeft || playerInput.MoveRight)
            {
                wasLastMovingRight = playerInput.MoveRight;
            }
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
            ApplyInput(input);
            if (input.MoveLeft && rb.linearVelocityX > -GameConstants.MaxSpeed)
            {
                rb.linearVelocityX -= speed;
            } else  if (input.MoveRight && rb.linearVelocityX < GameConstants.MaxSpeed)
            {
                rb.linearVelocityX += speed;
            }

            if (input.Jump)
            {
                rb.linearVelocityY = jumpValue;
            }
        }

       
    }
}