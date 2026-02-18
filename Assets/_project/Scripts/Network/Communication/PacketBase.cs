using System.Collections.Generic;
using _project.Scripts.PluginInterfaces;

namespace Network._project.Scripts.Network.Communication
{
    public abstract class PacketBase<T> where T : PacketBase<T>
    {
        public abstract ushort GetOpcode();
        
        public NetworkMessage BuildNetworkMessage()
        {
            return new NetworkMessage(Serialize(), GetOpcode());
        }
        
        public Packet BuildPacket(PacketFlags flags = PacketFlags.None)
        {
            return BuildNetworkMessage().GetPacket(flags);
        }

        public void SendTo(Peer destination, byte channelId, PacketFlags flags = PacketFlags.None)
        {
            BuildNetworkMessage().SendTo(destination, channelId, flags);
        }

        public T FromNetworkMessage(NetworkMessage message)
        {
            return message.OpCode != GetOpcode() ? throw new InvalidPacketException<T>(GetOpcode(), message.OpCode) : FromNetworkMessage_Impl(message);
        }
        protected abstract T FromNetworkMessage_Impl(NetworkMessage message);
        public abstract List<byte> Serialize();
    }
}