using System;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using UnityEngine;
using Event = _project.Scripts.PluginInterfaces.Event;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace Network._project.Scripts.Network.Entities
{
    public class NetworkClient
    {
        private Host _client = new();
        private Peer? _server;
        private string _address;
        private ushort _port;
        private AddressType _addressType;
        private bool _connected;
        private Action<NetworkEvent> _onConnectedCallback;

        public bool Connected => _connected;
        public string IpAddress
        {
            get => _address;
            set
            {
                if (Connected)
                {
                    throw new Exception("Cannot change Ip address of a running client");
                }
                _address = value;
            }
        }
        public ushort Port
        {
            get => _port;
            set
            {
                if (Connected)
                {
                    throw new Exception("Cannot change Port of a running client");
                }
                _port = value;
            }
        }
        public AddressType IpAddressType
        {
            get => _addressType;
            set
            {
                if (Connected)
                {
                    throw new Exception("Cannot change Address Type of a running client");
                }
                _addressType = value;
            }
        }
        public Host Client => _client;
        public Peer? Server => _server;

        ~NetworkClient()
        {
            Client?.Dispose();
        }

        public void Disconnect()
        {
            if (!Connected) return;
            _client.Dispose();
            _server = null;
            _connected = false;
        }

        public void SetOnConnectedCallback(Action<NetworkEvent> callback)
        {
            _onConnectedCallback = callback;
        }

        public void Connect()
        {
            ConnectTo(_address, _port, _addressType);
        }
        
        public void ConnectTo(string addressString, ushort port, AddressType addressType)
        {
            if (!Library.Initialize())
                throw new Exception("Failed to initialize ENet");
            IpAddressType = addressType;
            Port = port;
            IpAddress = addressString;
            Address address = new();
            if (!address.SetHost(addressType, addressString))
            {
                Debug.LogError("failed to resolve \"" + addressString + "\"");
                return;
            }
            
            address.Port = Port;
            
            _client?.Dispose();
            _client = new Host();
            _client.Create(address.Type, 1, 2, 0, 0);

            _server = _client.Connect(address, 0);
            
            // On laisse la connexion se faire pendant un maximum de 50 * 100ms = 5s
            for (uint i = 0; i < 50; ++i)
            {
                if (_client.Service(100, out Event evt) > 0)
                {
                    // We need to set Connected so that the send message to server method is available
                    if (evt.Type == EventType.Connect)
                    {
                        _connected = true;
                    }
                    
                    if (_onConnectedCallback != null)
                    {
                        _onConnectedCallback(NetworkEvent.FromENet6Event(evt));
                    }
                    
                    break;
                }
            }
            
            if (_server.Value.State != PeerState.Connected)
            {
                Debug.LogError("connection to \"" + addressString + "\" failed");
            }
        }

        public bool SendMessageToServer(NetworkMessage message, byte channelId = 0, PacketFlags flags = PacketFlags.None)
        {
            // !_server.HasValue really is to shut up rider for when we use it because it shouldn't be null as long as we are connected  
            if (!Connected || !_server.HasValue) return false;
            
            message.SendTo(_server.Value, channelId, flags);
            return true;
        }

        public void PollEvents(Action<NetworkEvent> callback)
        {
            if ((!_client?.IsSet ?? true) || !Connected)
            {
                return;
            }

            if (_client.Service(0, out Event evt) > 0)
            {
                do
                {
                    NetworkEvent networkEvent = NetworkEvent.FromENet6Event(evt);
                    callback(networkEvent);
                }
                while (_client.CheckEvents(out evt) > 0);
            }
        }
    }
}