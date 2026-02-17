using System;
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

        private void Start()
        {
            _client.ConnectTo("127.0.0.1", "5050", AddressType.IPv4);
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
            if (obj.Type == EventType.Receive)
            {
                uint readerPos = 0;
                string deserializeString = Deserializer.DeserializeString(obj.Message.Data, ref readerPos);
                Debug.Log("Received message: " + deserializeString);
            }
        }
    }
}