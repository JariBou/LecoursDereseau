using System;

namespace _project.Scripts.GameNetwork
{
    public class InvalidPacketException : Exception
    {
        public InvalidPacketException(ushort targetOpCode, ushort messageOpCode) : base($"Tried to reconstruct packet with target opCode {targetOpCode}, but message's opCode was {messageOpCode}")
        {
        }
    }
}