using System;

namespace Network._project.Scripts.Network.Communication
{
    public class InvalidPacketException<T> : Exception where T : PacketBase<T>
    {
        public InvalidPacketException(ushort targetOpCode, ushort messageOpCode) : base($"{typeof(T).AssemblyQualifiedName}::Tried to reconstruct packet with target opCode {targetOpCode}, but message's opCode was {messageOpCode}")
        {
        }
    }
}