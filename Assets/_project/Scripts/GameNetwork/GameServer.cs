using System;
using System.Collections.Generic;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;

namespace _project.Scripts.GameNetwork
{
    public class GameServer : MonoBehaviour
    {
        private NetworkServer _server = new();

        private void Awake()
        {
            _server.IpAddressType = AddressType.Any;
            _server.Port = "5050";
            _server.Start();
        }

        private void OnEnable()
        {
            TickManager.NetworkTick += TickManagerOnNetworkTick;
        }
        
        private void OnDisable()
        {
            TickManager.NetworkTick -= TickManagerOnNetworkTick;
        }

        private void TickManagerOnNetworkTick()
        {
            List<byte> bytes = new List<byte>();
            Serializer.SerializeString(bytes, "Hi client!");
            NetworkMessage message = new NetworkMessage(bytes, 0);
            _server.SendMessageToAllClients(message);
            _server.PollEvents(NetworkEventCallback);
        }

        private void NetworkEventCallback(NetworkEvent obj)
        {
            Debug.Log("Received event on channel " + obj.ChannelId);
        }
    }
}