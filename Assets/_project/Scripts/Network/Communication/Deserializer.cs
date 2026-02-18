using System;
using System.Collections.Generic;
using System.Text;

namespace Network._project.Scripts.Network.Communication
{
    public static class Deserializer
    {
        public static string DeserializeString(List<byte> byteArray, ref uint readerPos)
        {
            uint stringSize = DeserializeUInt(byteArray, ref readerPos);
            string content = Encoding.UTF8.GetString(byteArray.ToArray(), (int)readerPos, (int)stringSize);
            readerPos += stringSize;
            return content;
        }

        public static byte DeserializeByte(List<byte> byteArray, ref uint readerPos)
        {
            byte b = byteArray[(int)readerPos];
            readerPos += sizeof(byte);
            return b;
        }
        
        public static short DeserializeShort(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(short));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(short);
            return BitConverter.ToInt16(bytes.ToArray());
        }
        
        public static int DeserializeInt(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(int));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(short);
            return BitConverter.ToInt32(bytes.ToArray());
        }
        
        public static long DeserializeLong(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(long));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(long);
            return BitConverter.ToInt64(bytes.ToArray());
        }
        
        public static float DeserializeFloat(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(float));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(float);
            return BitConverter.ToSingle(bytes.ToArray());
        }
        
        public static double DeserializeDouble(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(double));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(double);
            return BitConverter.ToDouble(bytes.ToArray());
        }
        
        public static ushort DeserializeUShort(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(ushort));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(ushort);
            return BitConverter.ToUInt16(bytes.ToArray());
        }

        public static uint DeserializeUInt(List<byte> byteArray, ref uint readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(uint));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(uint);
            return BitConverter.ToUInt32(bytes.ToArray());
        }
        
        public static ulong DeserializeULong(List<byte> byteArray, ref ulong readerPos)
        {
            List<byte> bytes = byteArray.GetRange((int)readerPos, sizeof(ulong));
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            readerPos += sizeof(ulong);
            return BitConverter.ToUInt64(bytes.ToArray());
        }

        public static bool DeserializeBool(List<byte> byteArray, ref uint readerPos)
        {
            ushort s = DeserializeUShort(byteArray, ref readerPos);
            return s > 0; 
        }
    }
}