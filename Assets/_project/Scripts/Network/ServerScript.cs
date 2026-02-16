using System;
using _project.Scripts.PluginInterfaces;
using UnityEngine;
using JetBrains.Annotations;
using Network._project.Scripts.Network.Communication;
using TMPro;
using Event = _project.Scripts.PluginInterfaces.Event;
using EventType = _project.Scripts.PluginInterfaces.EventType;

namespace _project.Scripts.Network
{
    public class ServerScript : MonoBehaviour
    {
        [SerializeField] private AddressType _addressType = AddressType.IPv4;
        [SerializeField] private ushort _port;
        [CanBeNull] private Host _host;
        
        [SerializeField] private TMP_InputField _portText;
        [SerializeField] private TMP_InputField _ipText;
        [SerializeField] private TMP_Text _lastDataReceived;
        

        public void Host()
        {
            _lastDataReceived.gameObject.SetActive(true);
            
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
            // Address address = new  Address();
            // address.SetIP("localhost");
            address.Port = Convert.ToUInt16(_portText.text);
            // _portText.text = $"{address.Port}";
            _ipText.text = $"{address.GetIP()}";
            
            _host?.Dispose();
            _host = new Host();
            _host.Create(_addressType, address, 32, 2, 0, 0);
            _host.ThrowIfNotCreated();
            
            _portText.text = $"{address.Port}";
            _ipText.text = $"{address.GetIP()}";
            
            Debug.Log("Host Created");
        }

        private void FixedUpdate()
        {
            if (!_host?.IsSet ?? true)
            {
                return;
            }
            Event evt = new Event();
            if (_host.Service(0, out evt) > 0)
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
                            break;

                        case EventType.Receive:
                            Debug.Log("Receive");

                            uint readerPos = 0;
                            NetworkMessage message = NetworkMessage.FromPacket(evt.Packet);
                            string s = Deserializer.DeserializeString(message.Data, ref readerPos);
                            _lastDataReceived.text = $"LastDataReceived:\n {s}";
                            break;

                        case EventType.Timeout:
                            Debug.Log("Timeout");
                            break;
                    }
                }
                while (_host.CheckEvents(out evt) > 0);
            }
        }

        private void OnDestroy()
        {
            _host.Dispose();
        }
    }
}