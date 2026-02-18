using System.Collections.Generic;
using _project.Scripts.Network;
using Network._project.Scripts.Network.Communication;
using UnityEngine;

namespace _project.Scripts.GameNetwork.Packets
{
    public class PlayerPositionsPacket : PacketBase<PlayerPositionsPacket>
    {
        public Dictionary<ushort, Vector3> PlayerPosDic = new();

        public PlayerPositionsPacket()
        {
        }
        
        public PlayerPositionsPacket(Dictionary<ushort, Transform> playerTransformDic)
        {
            foreach (KeyValuePair<ushort, Transform> pair in playerTransformDic)
            {
                PlayerPosDic[pair.Key] = pair.Value.position;
            }
        }
        
        public override ushort GetOpcode()
        {
            return (ushort)NetOpCodes.Server.PlayerPosData;
        }
        
        protected override PlayerPositionsPacket FromNetworkMessage_Impl(NetworkMessage message)
        {
            uint readerPos = 0;
            int playerCount = Deserializer.DeserializeInt(message.Data, ref readerPos);
            for (int i = 0; i < playerCount; i++)
            {
                ushort playerIndex = Deserializer.DeserializeUShort(message.Data, ref readerPos);
                float x =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                float y =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                PlayerPosDic[playerIndex] = new Vector3(x, y, 0);
            }

            return this;
        }

        public override List<byte> Serialize()
        {
            List<byte> data = new List<byte>();
            // Update player Positions
            Serializer.SerializeInt(data, PlayerPosDic.Count);
            foreach ((ushort pIndex, Vector3 position) in PlayerPosDic)
            {
                Serializer.SerializeUShort(data, pIndex);
                Serializer.SerializeFloat(data, position.x);
                Serializer.SerializeFloat(data, position.y);
            }
            return data;
        }
    }
}