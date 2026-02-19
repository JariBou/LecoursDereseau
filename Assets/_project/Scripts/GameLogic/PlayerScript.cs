using System;
using System.Collections.Generic;
using _project.Scripts.GameNetwork;
using _project.Scripts.Network;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _project.Scripts.GameLogic
{

    public class PlayerInput
    {
        public bool MoveLeft = false;
        public bool MoveRight = false;
        //public bool MoveCancelled = false;
        public bool Attack = false;
        public bool Jump = false;

        public void Rreset()
        {
            MoveLeft = false;
            MoveRight = false;
            //MoveCancelled = false;
            Attack = false;
            Jump = false;
        }

        public void Serialize(List<byte> bytes)
        {
            byte data = 0;
            data |= (byte)((MoveLeft ? 1 : 0) << 0);
            data |= (byte)((MoveRight? 1 : 0) << 2);
            data |= (byte)((Attack ? 1 : 0) << 3);
            data |= (byte)((Jump ? 1 : 0) << 4);

            Serializer.SerializeByte(bytes, data);
        }

        public static PlayerInput DeSerialize(List<byte> bytes, ref uint readerPos)
        {
            byte data = Deserializer.DeserializeByte(bytes, ref readerPos);
            PlayerInput newInput = new PlayerInput();
            newInput.MoveLeft = ((1 << 0) & data) > 0;
            newInput.MoveRight = ((1 << 2) & data) > 0;
            newInput.Attack = ((1 << 3) & data) > 0;
            newInput.Jump = ((1 << 4) & data) > 0;

            return newInput;
        }

    }   

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

        private PlayerInput LastRecordedInput;

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
            inputs.FightPlayer.Move.performed += ctx => RecordMove(ctx);
            //inputs.FightPlayer.Move.performed += ctx => MvmtValue = ctx.ReadValue<Vector2>();
            //inputs.FightPlayer.Move.canceled += ctx => RecordInput(ctx);
            //inputs.FightPlayer.Move.canceled += ctx => MvmtValue = Vector2.zero;

            // Jump
            inputs.FightPlayer.Jump.started += ctx => RecordJump(ctx);
            //inputs.FightPlayer.Jump.started += ctx => TryJump();

            // Attack
            inputs.FightPlayer.Attack.started += ctx => RecordAttack(ctx);
            //inputs.FightPlayer.Attack.started += ctx => TryAttack();

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

        private void RecordMove(InputAction.CallbackContext ctx)
        {
            Vector2 temp  = ctx.ReadValue<Vector2>();
            if (temp.x > 0)
            {
                LastRecordedInput.MoveLeft = false;
                LastRecordedInput.MoveRight = true;
            }else if (temp.x < 0)
            {
                LastRecordedInput.MoveRight = false;
                LastRecordedInput.MoveLeft = true;
            }
        }

        private void RecordJump(InputAction.CallbackContext ctx)
        {
            LastRecordedInput.Jump = true;
        }

        private void RecordAttack(InputAction.CallbackContext ctx)
        {
            LastRecordedInput.Attack = true;
        }
        private void TickManagerOnNetworkTick()
        {
            if (!NetworkClient.Connected || _gameClient.PlayerIndex == NetConstants.InvalidClientIndex)
            {
                return;
            }
            List<byte> byteArray = new();
            /*          Serializer.SerializeFloat(byteArray, transform.position.x);
                        Serializer.SerializeFloat(byteArray, transform.position.y);*/

            // Parse 
            Serializer.SerializeUShort(byteArray, _gameClient.PlayerIndex);
            LastRecordedInput.Serialize(byteArray);
            NetworkClient.SendMessageToServer(new NetworkMessage(byteArray, (ushort)NetOpCodes.Client.PlayerInput));
            LastRecordedInput.Rreset();
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