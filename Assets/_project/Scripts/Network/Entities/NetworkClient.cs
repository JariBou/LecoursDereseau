using System;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using UnityEngine;
using Event = _project.Scripts.PluginInterfaces.Event;

namespace Network._project.Scripts.Network.Entities
{
    public class NetworkClient
    {
        private Host _client = new();
        private Peer? _server;
        private string _address;
        private string _port;
        private AddressType _addressType;
        private bool _connected;
       
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
        public string Port
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
        
        public event Action<NetworkEvent> EventReceived;

        public void Disconnect()
        {
            if (!Connected) return;
            _client.Dispose();
            _server = null;
            _connected = false;
        }

        public void Connect()
        {
            ConnectTo(_address, _port, _addressType);
        }
        
        public void ConnectTo(string addressString, string port, AddressType addressType)
        {
            if (!Library.Initialize())
                throw new Exception("Failed to initialize ENet");
            IpAddressType = addressType;
            Port = port;
            IpAddress = addressString;
            // Address address = Address.BuildAny(_addressType);
            Address address = new Address();
            if (!address.SetHost(addressType, addressString))
            {
                Debug.LogError("failed to resolve \"" + addressString + "\"");
                return;
            }
            
            address.Port = Convert.ToUInt16(port);
            
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
            
            if (_server.Value.State != PeerState.Connected)
            {
                Debug.LogError("connection to \"" + addressString + "\" failed");
                return;
            }

            _connected = true;
        }

        public bool SendMessageToServer(NetworkMessage message, byte channelId = 0)
        {
            // !_server.HasValue really is to shut up rider for when we use it because it shouldn't be null as long as we are connected  
            if (!Connected || !_server.HasValue) return false;
            
            message.SendTo(_server.Value, channelId);
            return true;
        }
        
        // TODO: Check which PollEvents to use
        #region Poll events
        
        public void PollEvents()
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
                    EventReceived?.Invoke(networkEvent);
                }
                while (_client.CheckEvents(out evt) > 0);
            }
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
        
        public void PollEvents(Action<Event> callback)
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
                    callback(evt);
                }
                while (_client.CheckEvents(out evt) > 0);
            }
        }
        
        #endregion
    }
}