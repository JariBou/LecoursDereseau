using System;
using System.Collections.Generic;
using _project.Scripts.Network;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace _project.Scripts.GameNetwork
{
    public class GameServer : MonoBehaviour
    {
        private NetworkServer _server = new();

        private void Awake()
        {
            _server.IpAddressType = AddressType.Any;
            _server.Port = 5050;
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
            if (!_server.SendMessageToAllClients(message))
            {
                Debug.LogError("SendMessageToAllClients Error");
                return;
            }
            _server.PollEvents(NetworkEventCallback);
        }

        private void NetworkEventCallback(NetworkEvent obj)
        {
            switch (obj.Type)
            {
                case EventType.None:
                    break;
                case EventType.Connect:
                    break;
                case EventType.Disconnect:
                    break;
                case EventType.Receive:
                    if (obj.Message.OpCode == (ushort)NetOpCodes.Client.PlayerInfo)
                    {
                        uint readerPos = 0;
                        string clientInstanceId = Deserializer.DeserializeString(obj.Message.Data, ref readerPos);
                        Debug.Log("Client connected with instance ID: " + clientInstanceId);
                    }
                    break;
                case EventType.Timeout:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}