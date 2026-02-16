using System;
using System.Collections.Generic;
using _project.Scripts.PluginInterfaces;
using Network._project.Scripts.Network.Communication;
using UnityEngine;
using Event = _project.Scripts.PluginInterfaces.Event;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace Network._project.Scripts.Network.Entities
{
    public class NetworkServer
    {
        private Host _server = new();
        private string _address;
        private string _port;
        private AddressType _addressType;
        private bool _started;

        public bool Started => _started;
        public string IpAddress
        {
            get => _address;
            set
            {
                if (Started)
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
                if (Started)
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
                if (Started)
                {
                    throw new Exception("Cannot change Address Type of a running client");
                }
                _addressType = value;
            }
        }
        public Host Server => _server;

        public List<Peer> ConnectedClients { get; } = new();

        public event Action<NetworkEvent> EventReceived;

        public void Stop()
        {
            if (!Started)
            {
                return;
            }
            _server?.Dispose();
            _server = null;
            ConnectedClients.Clear();
            _started = false;
        }
        
        public void Start(bool restartIfStarted = false)
        {
            if (_started && !restartIfStarted)
            {
                return;
            }
            if (!Library.Initialize())
                throw new Exception("Failed to initialize ENet");
            AddressType addressType = _addressType switch
            {
                AddressType.Any => AddressType.IPv6,
                AddressType.IPv4 => AddressType.IPv4,
                AddressType.IPv6 => AddressType.IPv6,
                _ => AddressType.IPv4
            };
            Address address = Address.BuildAny(addressType);
            address.Port = Convert.ToUInt16(Port);
            
            Stop();
            _server = new Host();
            _server.Create(_addressType, address, 32, 2, 0, 0);
            _server.ThrowIfNotCreated();
            
            Debug.Log("Server Created");
            _started = true;
        }

        private void InternalPollEvent(NetworkEvent networkEvent)
        {
            switch (networkEvent.Type)
            {
                case EventType.None:
                    break;
                case EventType.Connect:
                    ConnectedClients.Add(networkEvent.Source);
                    break;
                case EventType.Disconnect:
                    RemoveClient(networkEvent.Source);
                    break;
                case EventType.Receive:
                    break;
                case EventType.Timeout:
                    // For now we just remove the client as well (we could keep session alive for reconnection tho)
                    RemoveClient(networkEvent.Source);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveClient(Peer client)
        {
            for (var i = 0; i < ConnectedClients.Count; i++)
            {
                Peer tClient = ConnectedClients[i];
                if (tClient.ID != client.ID) continue;
                ConnectedClients.RemoveAt(i);
                break;
            }
        }

        // TODO: Check which PollEvents to use
        #region Poll events
        
        public void PollEvents()
        {
            if ((!_server?.IsSet ?? true) || !Started)
            {
                return;
            }

            if (_server.Service(0, out Event evt) > 0)
            {
                do
                {
                    NetworkEvent networkEvent = NetworkEvent.FromENet6Event(evt);
                    EventReceived?.Invoke(networkEvent);
                }
                while (_server.CheckEvents(out evt) > 0);
            }
        }

        public void PollEvents(Action<NetworkEvent> callback)
        {
            if ((!_server?.IsSet ?? true) || !Started)
            {
                return;
            }

            if (_server.Service(0, out Event evt) > 0)
            {
                do
                {
                    NetworkEvent networkEvent = NetworkEvent.FromENet6Event(evt);
                    callback(networkEvent);
                }
                while (_server.CheckEvents(out evt) > 0);
            }
        }
        
        public void PollEvents(Action<Event> callback)
        {
            if ((!_server?.IsSet ?? true) || !Started)
            {
                return;
            }

            if (_server.Service(0, out Event evt) > 0)
            {
                do
                {
                    NetworkEvent networkEvent = NetworkEvent.FromENet6Event(evt);
                    callback(evt);
                }
                while (_server.CheckEvents(out evt) > 0);
            }
        }
        
        #endregion

        public bool SendMessageToAllClients(NetworkMessage message, byte channelID = 0)
        {
            if (!Started ||  _server == null) return false;

            foreach (Peer client in ConnectedClients)
            {
                message.SendTo(client, channelID);
            }

            return true;
        }
    }
}