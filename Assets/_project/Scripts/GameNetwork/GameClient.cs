using System;
using System.Collections.Generic;
using _project.Scripts.GameNetwork.Packets;
using _project.Scripts.Network;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using Network._project.Scripts.Network.Entities;
using UnityEngine;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace _project.Scripts.GameNetwork
{
    public class GameClient : MonoBehaviour
    {
        private NetworkClient _client = new();
        
        public NetworkClient Client => _client;

        public ushort PlayerIndex => _playerIndex;

        [SerializeField] private GameObject _playerPrefab; // TEMP
        private Dictionary<ushort, Transform> _players = new(); // TEMP type, to change
        [SerializeField] private Transform _player; // TEMP type, to change
        private ushort _playerIndex = NetConstants.InvalidClientIndex;

        private void Start()
        {
            _client.SetOnConnectedCallback(OnConnectedToServer);
            _client.ConnectTo("127.0.0.1", Convert.ToUInt16("6060"), AddressType.IPv4);
        }

        // ReSharper disable once Unity.IncorrectMethodSignature Reason: wtf Rider this is correct stop annoying me
        private void OnConnectedToServer(NetworkEvent obj)
        {
            // Guard close to check if we actually connected or connection failed 
            if (obj.Type != EventType.Connect)
            {
                return;
            }
            NetworkMessage message = new(new List<byte>(), (ushort)NetOpCodes.Client.PlayerInfo);
            Serializer.SerializeString(message.Data, GetInstanceID().ToString());
            _client.SendMessageToServer(message);
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
            _client.PollEvents(Callback);
        }

        private void Callback(NetworkEvent obj)
        {
            if (obj.Type == EventType.Disconnect)
            {
                foreach (KeyValuePair<ushort, Transform> pair in _players)
                {
                    ushort playerIndex = pair.Key;
                    if (playerIndex == _playerIndex)
                    {
                        continue;
                    }
                    Destroy(_players[playerIndex].gameObject);
                }
                _players.Clear();
                return;
            } 
            if (obj.Type != EventType.Receive)
            {
                Debug.Log("Received event of type: " + obj.Type);
                return;
            }

            switch (obj.Message.OpCode)
            {
                case (ushort)NetOpCodes.Server.PlayerConnected:
                {
                    uint readerPos = 0;
                    ushort newPlayerIndex = Deserializer.DeserializeUShort(obj.Message.Data, ref readerPos);
                    // for now this is how we detect if we were the ones to connect
                    // Kind of dirty but works for now
                    if (_playerIndex == NetConstants.InvalidClientIndex)
                    {
                        _playerIndex = newPlayerIndex;
                    }
                    int playerCount = Deserializer.DeserializeInt(obj.Message.Data, ref readerPos);
                    Debug.Log($"Received players data with size of: {playerCount}");
                    for (int i = 0; i < playerCount; i++)
                    {
                        ushort playerIndex = Deserializer.DeserializeUShort(obj.Message.Data, ref readerPos);
                        if (!_players.ContainsKey(playerIndex))
                        {
                            _players.Add(playerIndex,
                                playerIndex == _playerIndex
                                    ? _player
                                    : Instantiate(_playerPrefab, transform).transform);
                        }
                    }

                    break;
                }
                case (ushort)NetOpCodes.Server.PlayerPosData:
                {
                    PlayerPositionsPacket packet = new PlayerPositionsPacket().FromNetworkMessage(obj.Message);
                    foreach ((ushort playerIndex, Vector3 position) in packet.PlayerPosDic)
                    {
                        if (playerIndex == _playerIndex)
                        {
                            float magnitude = (position - _player.position).magnitude;
                            if (magnitude < NetConstants.PlayerPositionReconciliationErrorMargin)
                            {
                                continue;
                            }
                        }
                        _players[playerIndex].position = position;
                    }

                    break;
                }
                case (ushort)NetOpCodes.Server.PlayerDisconnected:
                {
                    uint readerPos = 0;
                    ushort playerIndex = Deserializer.DeserializeUShort(obj.Message.Data, ref readerPos);
                    Destroy(_players[playerIndex].gameObject);
                    _players.Remove(playerIndex);
                    break;
                }
            }
        }
    }
}