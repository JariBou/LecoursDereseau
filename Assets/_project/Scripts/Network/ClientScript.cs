using System;
using System.Collections.Generic;
using _project.Scripts.PluginInterfaces;
using GraphicsLabor.Scripts.Attributes.LaborerAttributes.InspectedAttributes;
using Network._project.Scripts.Network.Communication;
using TMPro;
using UnityEngine;
using Event = _project.Scripts.PluginInterfaces.Event;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace _project.Scripts.Network
{
    public class ClientScript : MonoBehaviour
    {
        [SerializeField] private AddressType _addressType = AddressType.IPv4;
        [SerializeField] private ushort _port;
        private Host _client = new();
        private Peer? _server;

        [SerializeField] private TMP_InputField _portText;
        [SerializeField] private TMP_InputField _ipText;
        
        [SerializeField] private GameObject _clientDisplay;
        [SerializeField] private TMP_InputField _dataField;

        
        private void OnDestroy()
        {
            _client.Dispose();
        }

        [Button]
        public void TryConnect()
        {
            Connect(_ipText.text);
        }
        
        private void Connect(string addressString)
        {
            if (!Library.Initialize())
                throw new Exception("Failed to initialize ENet");
            // Address address = Address.BuildAny(_addressType);
            Address address = new Address();
            if (!address.SetHost(_addressType, addressString))
            {
                Debug.LogError("failed to resolve \"" + addressString + "\"");
                return;
            }
            
            address.Port = Convert.ToUInt16(_portText.text);
            
            _ipText.text = $"{address.GetIP()}";

            _client?.Dispose();
            _client = new Host();
            _client.Create(address.Type, 1, 2, 0, 0);

            _server = _client.Connect(address, 0);
            
            // On laisse la connexion se faire pendant un maximum de 50 * 100ms = 5s
            for (uint i = 0; i < 50; ++i)
            {
                Event evt = new Event();
                if (_client.Service(100, out evt) > 0)
                {
                    Debug.Log(evt.Type);
                    // Nous avons un événement, la connexion a soit pu s'effectuer (ENET_EVENT_TYPE_CONNECT) soit échoué (ENET_EVENT_TYPE_DISCONNECT)
                    break; //< On sort de la boucle
                }
            }
            
            _portText.text = $"{address.Port}";
            _ipText.text = $"{address.GetIP()}";

            if (_server.Value.State != PeerState.Connected)
            {
                Debug.LogError("connection to \"" + addressString + "\" failed");
                return;
            }
            
            _clientDisplay.SetActive(true);
        }
        
        
        // FixedUpdate est appelé à chaque Tick (réglé dans le projet)
        void FixedUpdate()
        {
            if (!_client.IsSet)
            {
                return;
            }
            Event evt = new Event();
            if (_client.Service(0, out evt) > 0)
            {
                do
                {
                    switch (evt.Type)
                    {
                        case EventType.None:
                            Debug.Log("?");
                            break;

                        case EventType.Connect:
                            Debug.Log("Connect");
                            break;

                        case EventType.Disconnect:
                            Debug.Log("Disconnect");
                            _server = null;
                            break;

                        case EventType.Receive:
                            Debug.Log("Receive");
                            break;

                        case EventType.Timeout:
                            Debug.Log("Timeout");
                            break;
                    }
                }
                while (_client.CheckEvents(out evt) > 0);
            }
        }

        public void SendData()
        {
            if (_server == null)
            {
                return;
            }
            // Packet pack = new Packet();
            List<byte> byteList = new List<byte>();
            Serializer.SerializeString(byteList, _dataField.text);
            NetworkMessage message = new(byteList, 0);
            message.SendTo(_server.Value, 0);
            // Packet pack = message.GetPacket();
            // _server.Value.Send(0, ref pack);
        }
    }
}