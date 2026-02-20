using System.Collections.Generic;
using System.Linq;
using _project.Scripts.GameLogic;
using _project.Scripts.Network;
using Network._project.Scripts.Network.Communication;
using UnityEngine;

namespace _project.Scripts.GameNetwork.Packets
{
    public class PlayerDataPacket : PacketBase<PlayerDataPacket>
    {
        public Dictionary<ushort, Vector3> PlayerPosDic = new();
        public Dictionary<ushort, Vector3> PlayerSpeedDic = new();
        public Dictionary<ushort, PlayerInput> PlayerInputDic = new();
        public Dictionary<ushort, PlayerHitPacket> PlayerHitPacketDic = new();



        public PlayerDataPacket()
        {
        }

        public PlayerDataPacket(Dictionary<ushort, PlayerInput> playerDic, Dictionary<ushort, ReplicatedPlayerScript> playerTransformDic, Dictionary<ushort, PlayerHitPacket> _playerHitDictionnary)
        {
            foreach (KeyValuePair<ushort, PlayerHitPacket> pair in _playerHitDictionnary)
            {
                PlayerHitPacketDic[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<ushort, PlayerInput> pair in playerDic)
            {
                PlayerInputDic[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<ushort, ReplicatedPlayerScript> pair in playerTransformDic)
            {
                PlayerPosDic[pair.Key] = pair.Value.GetPos();
                PlayerSpeedDic[pair.Key] = pair.Value.GetSpeed();
            }
        }

        public override ushort GetOpcode()
        {
            return (ushort)NetOpCodes.Server.PlayerData;
        }
        
        protected override PlayerDataPacket FromNetworkMessage_Impl(NetworkMessage message)
        {
            uint readerPos = 0;
            int playerCount = Deserializer.DeserializeInt(message.Data, ref readerPos);
            for (int i = 0; i < playerCount; i++)
            {
                ushort playerIndex = Deserializer.DeserializeUShort(message.Data, ref readerPos);
                float tX =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                float tY =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                float sX =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                float sY =  Deserializer.DeserializeFloat(message.Data, ref readerPos);
                PlayerPosDic[playerIndex] = new Vector3(tX, tY, 0);
                PlayerSpeedDic[playerIndex] = new Vector3(sX, sY, 0);
                PlayerInputDic[playerIndex] = PlayerInput.DeSerialize(message.Data, ref readerPos);
                PlayerHitPacketDic[playerIndex] = PlayerHitPacket.DeSerialize(message.Data, ref readerPos);
            }

            return this;
        }

        public override List<byte> Serialize()
        {
            List<byte> data = new List<byte>();
            // Update player Positions
            ushort[] pIndexes = PlayerPosDic.Keys.ToArray();
            Serializer.SerializeInt(data, pIndexes.Length);
            foreach (ushort playerIndex in pIndexes)
            {
                Vector3 position = PlayerPosDic[playerIndex];
                Vector3 speed = PlayerSpeedDic[playerIndex];
                PlayerInput input = PlayerInputDic[playerIndex];
                PlayerHitPacket hitPacket = PlayerHitPacketDic.TryGetValue(playerIndex, out PlayerHitPacket hit) ? hit : new PlayerHitPacket();
                Serializer.SerializeUShort(data, playerIndex);
                Serializer.SerializeFloat(data, position.x);
                Serializer.SerializeFloat(data, position.y);
                Serializer.SerializeFloat(data, speed.x);
                Serializer.SerializeFloat(data, speed.y);
                input.Serialize(data);
                hitPacket.Serialize(data);
            }
            return data;
        }
    }
}