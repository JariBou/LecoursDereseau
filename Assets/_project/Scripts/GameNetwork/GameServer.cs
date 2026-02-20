using System;
using System.Collections.Generic;
using System.Linq;
using _project.Scripts.GameLogic;
using _project.Scripts.GameNetwork.Packets;
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
        private Dictionary<ushort, ReplicatedPlayerScript> _players = new(); // TEMP type, to change
        private Dictionary<ushort, PlayerInput> _playersWithInputs = new(); // TEMP type, to change
        private Dictionary<ushort, PlayerHitPacket> _playerHitDictionnary = new(); // TEMP type, to change

        private Dictionary<ushort, Peer> _playerClientDic = new(); // TEMP type, to change

        private void Awake()
        {
            _server.IpAddressType = AddressType.IPv4;
            _server.Port = Convert.ToUInt16("6060");
            _server.Start();
        }

        private void OnEnable()
        {
            TickManager.PreUpdate += TickManagerOnPreUpdate;
            TickManager.NetworkTick += TickManagerOnNetworkTick;
        }

        private void TickManagerOnPreUpdate()
        {
            foreach ((ushort pIndex, ReplicatedPlayerScript playerScript) in _players)
            {
                playerScript.ApplyInputs(_playersWithInputs.GetValueOrDefault(pIndex, new PlayerInput()));
                //playerScript.Hurt();
            }
        }

        private void OnDisable()
        {
            TickManager.PreUpdate -= TickManagerOnPreUpdate;
            TickManager.NetworkTick -= TickManagerOnNetworkTick;
        }

        private void OnDestroy()
        {
            _server.Stop();
        }

        private void TickManagerOnNetworkTick()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _server.Stop();
            }

            // Send data to all players
            PlayerDataPacket dataPacket = new(_playersWithInputs, _players, _playerHitDictionnary);
            
            if (!_server.SendMessageToAllClients(dataPacket.BuildNetworkMessage()))
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
                    OnPlayerConnected(evt);
                    break;
                case EventType.Disconnect:
                    Debug.Log("Player Disconnect : " + FindPlayerIndexFromPeer(evt.Source));
                    OnPlayerDisconnected(evt);
                    break;
                case EventType.Receive:
                    OnMessageReceived(evt);
                    break;
                case EventType.Timeout:
                    Debug.Log("Player Timeout : " + FindPlayerIndexFromPeer(evt.Source));
                    OnPlayerTimeout(evt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnPlayerTimeout(NetworkEvent evt)
        {
            OnPlayerDisconnected(evt);
        }

        private void OnPlayerDisconnected(NetworkEvent evt)
        {
            ushort playerIndex = FindPlayerIndexFromPeer(evt.Source);
            if (playerIndex == NetConstants.InvalidClientIndex)
            {
                return;
            }
            Destroy(_players[playerIndex].gameObject);
            _players.Remove(playerIndex);
            _playerClientDic.Remove(playerIndex);
            
            NetworkMessage msg = new NetworkMessage((ushort)NetOpCodes.Server.PlayerDisconnected);
            Serializer.SerializeUShort(msg.Data, playerIndex);
            _server.SendMessageToAllClients(msg);
        }

        private void OnPlayerConnected(NetworkEvent evt)
        {
            ushort newPlayerIndex;
            if (_playerClientDic.Count > 0)
            {
                List<ushort> existingPlayerIndices = _playerClientDic.Keys.ToList();
                existingPlayerIndices.Sort();
                ushort last = existingPlayerIndices.Last();
                newPlayerIndex = (ushort)(last + 1);
            }
            else
            {
                newPlayerIndex = 1;
            }
            
            _playerClientDic[newPlayerIndex] = evt.Source;
        }

        private void OnMessageReceived(NetworkEvent evt)
        {
            if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerInfo)
            {
                uint readerPos = 0;
                string clientInstanceId = Deserializer.DeserializeString(evt.Message.Data, ref readerPos);

                // Create new player
                GameObject player = Instantiate(_playerPrefab, transform);
                ushort playerIndex = FindPlayerIndexFromPeer(evt.Source);
                if (playerIndex == NetConstants.InvalidClientIndex)
                {
                    Debug.LogError($"Found no player associated with Client with ID {evt.Source.ID}, IP: {evt.Source.IP}");
                    return;
                }
                _players.Add(playerIndex, player.GetComponent<ReplicatedPlayerScript>());
                _playersWithInputs.Add(playerIndex, new PlayerInput());

                NetworkMessage msg = new(new List<byte>(), (ushort)NetOpCodes.Server.PlayerConnected);
                // We set the first data as the new player's index (the one that just send the data
                Serializer.SerializeUShort(msg.Data, playerIndex);
                Serializer.SerializeInt(msg.Data, _players.Count);
                foreach (KeyValuePair<ushort, ReplicatedPlayerScript> pair in _players)
                {
                    Serializer.SerializeUShort(msg.Data, pair.Key);
                }

                _server.SendMessageToAllClients(msg);

                Debug.Log("Client connected with instance ID: " + clientInstanceId);
            }
            else if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerPos)
            {
                uint readerPos = 0;
                ushort pIndex = Deserializer.DeserializeUShort(evt.Message.Data, ref readerPos);
                float pX = Deserializer.DeserializeFloat(evt.Message.Data, ref readerPos);
                float pY = Deserializer.DeserializeFloat(evt.Message.Data, ref readerPos);
                if (pIndex == 0)
                {
                    return;
                }
                _players[pIndex].transform.position = new Vector3(pX, pY, 0);
            }
            else if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerInput)
            {
                uint readerPos = 0;
                ushort pIndex = Deserializer.DeserializeUShort(evt.Message.Data, ref readerPos);
                PlayerInput savedInputData = PlayerInput.DeSerialize(evt.Message.Data, ref readerPos);

                if (pIndex == NetConstants.InvalidClientIndex)
                {
                    return;
                }
                // Save input here
                _playersWithInputs[pIndex] = savedInputData;
            } else if (evt.Message.OpCode == (ushort)NetOpCodes.Client.PlayerHit)
            {
                uint readerPos = 0;
                ushort pIndex = Deserializer.DeserializeUShort(evt.Message.Data, ref readerPos);
                PlayerHitPacket savedHitData = PlayerHitPacket.DeSerialize(evt.Message.Data, ref readerPos);

                if (pIndex == NetConstants.InvalidClientIndex)
                {
                    return;
                }
                _playerHitDictionnary[pIndex] = savedHitData;
            }
        }

        private ushort FindPlayerIndexFromPeer(Peer peer)
        {
            try
            {
                return _playerClientDic.First(pair => pair.Value.ID == peer.ID).Key;
            }
            catch (InvalidOperationException _)
            {
                return NetConstants.InvalidClientIndex;
            }
        }
    }
}