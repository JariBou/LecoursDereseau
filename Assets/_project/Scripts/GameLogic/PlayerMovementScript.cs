using System;
using System.Collections.Generic;
using _project.Scripts.GameNetwork;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _project.Scripts.GameLogic
{

    public class PlayerHitPacket
    {
        public bool gotHurt = false;
        public bool tryAttack = false;

        public void Rreset()
        {
            gotHurt = false;
            tryAttack = false;
        }

        public void Serialize(List<byte> bytes)
        {
            byte data = 0;
            data |= (byte)((gotHurt ? 1 : 0) << 0);
            data |= (byte)((tryAttack? 1 : 0) << 1);

            Serializer.SerializeByte(bytes, data);
        }

        public static PlayerHitPacket DeSerialize(List<byte> bytes, ref uint readerPos)
        {
            byte data = Deserializer.DeserializeByte(bytes, ref readerPos);
            PlayerHitPacket newPacket = new PlayerHitPacket();
            newPacket.gotHurt = ((1 << 0) & data) > 0;
            newPacket.tryAttack = ((1 << 1) & data) > 0;

            return newPacket;
        }

    }

    public class PlayerInput
    {
        public bool MoveLeft = false;
        public bool MoveRight = false;
        //public bool MoveCancelled = false;
        public bool Attack = false;
        public bool Jump = false;
        public void Rreset()
        {
            // MoveLeft = false;
            // MoveRight = false;
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

    public class PlayerMovementScript : ReplicatedPlayerScriptBase
    {

        [SerializeField] private GameClient _gameClient;
        private NetworkClient NetworkClient => _gameClient.Client;

        PlayerInputs inputs;
        Vector2 MvmtValue;

        private PlayerInput LastRecordedInput = new ();
        private PlayerHitPacket packetHit= new();

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            inputs = new PlayerInputs();

            // Deplacement
            inputs.FightPlayer.Move.performed += RecordMove;
            inputs.FightPlayer.Move.canceled += RecordMove;
            // inputs.FightPlayer.Move.performed += ctx => RecordMove(ctx);
            //inputs.FightPlayer.Move.performed += ctx => MvmtValue = ctx.ReadValue<Vector2>();
            //inputs.FightPlayer.Move.canceled += ctx => RecordInput(ctx);
            //inputs.FightPlayer.Move.canceled += ctx => MvmtValue = Vector2.zero;

            // Jump
            inputs.FightPlayer.Jump.started += ctx => RecordJump(ctx);
            //inputs.FightPlayer.Jump.started += ctx => TryJump();

            // Attack
            inputs.FightPlayer.Attack.started += RecordAttack;
            //inputs.FightPlayer.Attack.started += ctx => TryAttack();

        }

        private void TryAttack()
        {
            Debug.Log("Attack");
            packetHit.tryAttack = true;
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
            TickManager.NetworkTick -= TickManagerOnNetworkTick;
            inputs.FightPlayer.Disable();
        }

        private void RecordMove(InputAction.CallbackContext ctx)
        {
            Vector2 temp  = ctx.ReadValue<Vector2>();
            LastRecordedInput.MoveLeft = false;
            LastRecordedInput.MoveRight = false;
            if (temp.x > 0)
            {
                LastRecordedInput.MoveRight = true;
            }else if (temp.x < 0)
            {
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


        public void RecordDamage()
        {
            packetHit.gotHurt = true;
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

            // Parse Inputs
            Serializer.SerializeUShort(byteArray, _gameClient.PlayerIndex);
            LastRecordedInput.Serialize(byteArray);
            NetworkClient.SendMessageToServer(new NetworkMessage(byteArray, (ushort)NetOpCodes.Client.PlayerInput));
            LastRecordedInput.Rreset();
            // Parse Hit packet
            // Serializer.SerializeUShort(byteArray, _gameClient.PlayerIndex);
            // packetHit.Serialize(byteArray);
            // NetworkClient.SendMessageToServer(new NetworkMessage(byteArray, (ushort)NetOpCodes.Client.PlayerHit));
            // packetHit.Rreset();

        }

        void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                NetworkClient.Disconnect();
            }
        }

    }
}