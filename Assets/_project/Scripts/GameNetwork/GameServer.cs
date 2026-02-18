using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Transform _player; // TEMP
        [SerializeField] private GameObject _playerPrefab; // TEMP
        private Dictionary<ushort, Transform> _players = new(); // TEMP type, to change
        private Dictionary<ushort, Peer> _playerClient = new(); // TEMP type, to change

        private void Awake()
        {
            _server.IpAddressType = AddressType.IPv4;
            _server.Port = Convert.ToUInt16("6060");
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
            NetworkMessage message = new NetworkMessage(bytes, (ushort)NetOpCodes.Server.PlayerPosData);
            
            // Update player Positions
            Serializer.SerializeInt(message.Data, _players.Count);
            foreach (KeyValuePair<ushort, Transform> pair in _players)
            {
                Serializer.SerializeUShort(message.Data, pair.Key);
                Debug.Log("Serializing pos data for player index: " + pair.Key);
                Serializer.SerializeFloat(message.Data, pair.Value.position.x);
                Serializer.SerializeFloat(message.Data, pair.Value.position.y);
            }
            
            if (!_server.SendMessageToAllClients(message))
            {
                Debug.LogError("SendMessageToAllClients Error");
            }
            _server.PollEvents(NetworkEventCallback);
        }

        private void NetworkEventCallback(NetworkEvent evt)
        {
            switch (evt.Type)
            {
                case EventType.None:
                    break;
                case EventType.Connect:
                    break;
                case EventType.Disconnect:
                    break;
                case EventType.Receive:
                    OnMessageReceived(evt);
                    break;
                case EventType.Timeout:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMessageReceived(NetworkEvent evt)
        {
            if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerInfo)
            {
                uint readerPos = 0;
                string clientInstanceId = Deserializer.DeserializeString(evt.Message.Data, ref readerPos);

                GameObject player = Instantiate(_playerPrefab, transform);
                ushort playerIndex = (ushort)(_players.Keys.Count+1); // TEMP
                _players.Add(playerIndex, player.transform);

                NetworkMessage msg = new(new List<byte>(), (ushort)NetOpCodes.Server.PlayerConnected);
                Serializer.SerializeUShort(msg.Data, playerIndex);
                Serializer.SerializeInt(msg.Data, _players.Count);
                foreach (KeyValuePair<ushort, Transform> pair in _players)
                {
                    Serializer.SerializeUShort(msg.Data, pair.Key);
                }
                
                _server.SendMessageToAllClients(msg);
                
                Debug.Log("Client connected with instance ID: " + clientInstanceId);
            } else if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerPos)
            {
                uint readerPos = 0;
                ushort pIndex = Deserializer.DeserializeUShort(evt.Message.Data, ref readerPos);
                // Debug.Log($"Received Info for player with index {pIndex}");
                float pX = Deserializer.DeserializeFloat(evt.Message.Data, ref readerPos);
                float pY = Deserializer.DeserializeFloat(evt.Message.Data, ref readerPos);
                if (pIndex == 0)
                {
                    return;
                }
                _players[pIndex].transform.position = new Vector3(pX, pY, 0);
            }
        }
    }
}