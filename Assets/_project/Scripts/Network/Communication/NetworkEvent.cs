using _project.Scripts.PluginInterfaces;

namespace Network._project.Scripts.Network.Communication
{
    public class NetworkEvent
    {
        /// <summary>
        /// NetworkMessage, only set when type is Received
        /// </summary>
        public NetworkMessage Message { get; private set; }
        public byte ChannelId { get; private set; }
        public EventType Type { get; private set; }
        public Peer Source { get; private set; }
        
        public Event Native { get; private set; }

        public static NetworkEvent FromENet6Event(Event evt)
        {
            NetworkEvent networkEvent = new()
            {
                ChannelId = evt.ChannelID,
                Source = evt.Peer,
                Type = evt.Type,
                Native =  evt,
            };

            if (evt.Type == EventType.Receive)
            {
                networkEvent.Message =  NetworkMessage.FromPacket(evt.Packet);
            }

            return networkEvent;
        }
    }
}