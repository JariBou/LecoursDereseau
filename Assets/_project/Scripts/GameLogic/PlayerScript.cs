using System;
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
        
        private void Update()
        {
            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");
            
            transform.position += new Vector3(xAxis, yAxis, 0).normalized *  Time.deltaTime;
        }

        private void OnEnable()
        {
            TickManager.NetworkTick += TickManagerOnNetworkTick;
        }

        private void TickManagerOnNetworkTick()
        {
            if (!NetworkClient.Connected)
            {
                return;
            }
            List<byte> byteArray = new();
            Serializer.SerializeFloat(byteArray, transform.position.x);
            Serializer.SerializeFloat(byteArray, transform.position.y);
            NetworkClient.SendMessageToServer(new NetworkMessage(byteArray, (ushort)NetOpCodes.Client.PlayerPos));
        }
    }
}