using System.Collections.Generic;
using _project.Scripts.GameNetwork;
using _project.Scripts.Network;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class PlayerScript : MonoBehaviour
    {

        [SerializeField] private GameClient _gameClient;
        private NetworkClient NetworkClient => _gameClient.Client;

        PlayerInputs inputs;
        Vector2 MvmtValue;
        public float speed = 10;
        public bool isGrounded;
        private bool pendingJump = false;
        public float jumpValue = 3.0f;
        private float currentJump = 0.0f;


        private Rigidbody2D rigidbody;
        private void Update()
        {
            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.F1))
            {
                NetworkClient.Disconnect();
            }
            
            transform.position += new Vector3(xAxis, yAxis, 0).normalized *  Time.deltaTime;
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            inputs = new PlayerInputs();

            // Deplacements
            inputs.FightPlayer.Move.performed += ctx => RecordInput();
            inputs.FightPlayer.Move.performed += ctx => MvmtValue = ctx.ReadValue<Vector2>();
            inputs.FightPlayer.Move.canceled += ctx => MvmtValue = Vector2.zero;
            // Jump
            inputs.FightPlayer.Jump.started += ctx => RecordInput();
            inputs.FightPlayer.Jump.started += ctx => TryJump();

            // Attack
            inputs.FightPlayer.Attack.started += ctx => RecordInput();
            inputs.FightPlayer.Attack.started += ctx => TryAttack();

        }

        private void TryAttack()
        {
            Debug.Log("Attack");
        }

        private void TryJump()
        {
            if (isGrounded)
            {
                pendingJump = true;
            }
            Debug.Log("Jump");
        }

        private void OnEnable()
        {
            TickManager.NetworkTick += TickManagerOnNetworkTick;
            inputs.FightPlayer.Enable();
        }

        private void OnDisable()
        {
            inputs.FightPlayer.Disable();
        }

        void RecordInput(/*Input package*/)
        {
        }

        private void TickManagerOnNetworkTick()
        {
            if (!NetworkClient.Connected || _gameClient.PlayerIndex == NetConstants.InvalidClientIndex)
            {
                return;
            }
            List<byte> byteArray = new();
            Serializer.SerializeUShort(byteArray, _gameClient.PlayerIndex);
            Serializer.SerializeFloat(byteArray, transform.position.x);
            Serializer.SerializeFloat(byteArray, transform.position.y);
            NetworkClient.SendMessageToServer(new NetworkMessage(byteArray, (ushort)NetOpCodes.Client.PlayerPos));
        }

        void FixedUpdate()
        {
            rigidbody.AddForce(new Vector2(MvmtValue.x * speed, 0f));

            if (pendingJump)
            {
                rigidbody.AddForce(new Vector2(0f, jumpValue), ForceMode2D.Impulse);
                pendingJump = false;
            }

            // CheckGround
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);

            Debug.DrawRay(transform.position, -Vector2.up);

            isGrounded = false;
            if(hit)
            {
                //Debug.Log(hit.transform.gameObject.name);
                //Debug.Log("Distance to ground: " + Vector3.Distance(hit.point, transform.position));
             
                if((Vector3.Distance(hit.point, transform.position) <= 2.0f) 
                    && (hit.transform.gameObject.tag == "ground"))
                {
                    isGrounded = true;
                } else
                {
                    isGrounded = false;
                }
            } 
        }
    }
}