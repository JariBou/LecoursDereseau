using _project.Scripts.PluginInterfaces;

namespace Network._project.Scripts.Network.Communication
{
    public class NetworkEvent
    {
        public NetworkMessage Message { get; private set; }
        public byte ChannelId { get; private set; }
        public EventType Type { get; private set; }
        public Peer Source { get; private set; }

        public static NetworkEvent FromENet6Event(Event evt)
        {
            NetworkEvent networkEvent = new()
            {
                Message = NetworkMessage.FromPacket(evt.Packet),
                ChannelId = evt.ChannelID,
                Source = evt.Peer,
                Type = evt.Type
            };

            return networkEvent;
        }
    }
}