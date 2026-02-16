using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network._project.Scripts.Network.Communication
{
    public static class Serializer
    {
        public static void SerializeString(List<byte> byteArray, string str)
        {
            byte[] collection = Encoding.UTF8.GetBytes(str);
            SerializeUInt(byteArray, (uint)collection.Length);
            byteArray.AddRange(collection);
        } 
        
        public static void SerializeByte(List<byte> byteArray, byte val)
        {
            byteArray.Add(val);
        }
        
        public static void SerializeShort(List<byte> byteArray, short val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeInt(List<byte> byteArray, int val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeLong(List<byte> byteArray, long val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeFloat(List<byte> byteArray, float val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeDouble(List<byte> byteArray, double val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeUShort(List<byte> byteArray, ushort val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeUInt(List<byte> byteArray, uint val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeULong(List<byte> byteArray, ulong val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            byteArray.AddRange(bytes);
        }
        
        public static void SerializeBool(List<byte> byteArray, bool val)
        {
            SerializeUShort(byteArray, (ushort)(val ? 1 : 0));
        }
    }
}