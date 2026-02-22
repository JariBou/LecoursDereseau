using System;

namespace _project.Scripts.GameLogic
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public uint Score { get; set; } = 0;
        public ushort ID { get; private set; }

        public PlayerData(string name, ushort id)
        {
            Name = name;
            ID = id;
        }


        public override bool Equals(object obj)
        {
            if (obj is PlayerData data)
            {
                return Equals(data);
            }
            return false;
        }

        protected bool Equals(PlayerData other)
        {
            if (other == null)
            {
                return false;
            }
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID);
        }
    }
}