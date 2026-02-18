using System.Collections.Generic;
using System.Linq;
using _project.Scripts.PluginInterfaces;

namespace Network._project.Scripts.Network.Communication
{
    public class NetworkMessage
    {
        private List<byte> _data;
        private ushort _opCode;
        
        public List<byte> Data => _data;
        public uint DataSize => (uint)_data.Count;
        public ushort OpCode => _opCode;
        
        public NetworkMessage(List<byte> data, ushort opCode)
        {
            _data =  data;
            _opCode = opCode;
        }

        public NetworkMessage(ushort opCode)
        {
            _data = new List<byte>();
            _opCode = opCode;
        }

        public byte[] GetNetworkBytes()
        {
            List<byte> bytes = new List<byte>();
            
            Serializer.SerializeUShort(bytes, _opCode);

            bytes.AddRange(_data);
            
            return bytes.ToArray();
        }

        public Packet GetPacket(PacketFlags flags = PacketFlags.None)
        {
            Packet packet = new();
            packet.Create(GetNetworkBytes(), flags);
            return packet;
        }

        public Packet CreatePacket(List<byte> data, ushort opCode, PacketFlags flags = PacketFlags.None)
        {
            return  new NetworkMessage(data, opCode).GetPacket(flags);
        }

        public static NetworkMessage FromNetworkBytes(byte[] bytes)
        {
            List<byte> byteArray = bytes.ToList();
            uint readerPos = 0;
            ushort opCode = Deserializer.DeserializeUShort(byteArray, ref readerPos);
            return new NetworkMessage(byteArray.GetRange(sizeof(ushort), bytes.Length-sizeof(ushort)), opCode);
        }
        
        public static NetworkMessage FromPacket(Packet packet)
        {
            byte[] rawBytes = new byte[packet.Length];
            packet.CopyTo(rawBytes);
            List<byte> byteArray = rawBytes.ToList();
            uint readerPos = 0;
            ushort opCode = Deserializer.DeserializeUShort(byteArray, ref readerPos);
            return new NetworkMessage(byteArray.GetRange(sizeof(ushort), rawBytes.Length-sizeof(ushort)), opCode);
        }

        public void SendTo(Peer destination, byte channelId, PacketFlags flags = PacketFlags.None)
        {
            Packet packet = GetPacket(flags);
            destination.Send(channelId, ref packet);
        }
    }
}