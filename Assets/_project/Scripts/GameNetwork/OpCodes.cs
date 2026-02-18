namespace _project.Scripts.Network
{
    public static class NetOpCodes
    {
        public enum Server : ushort
        {
            PlayerConnected = 1,
            PlayerDisconnected = 2,
            PlayerPosData = 10,
        }
        
        public enum Client : ushort
        {
            PlayerInfo = 1,
            PlayerPos = 2 // TEMP
        }
    }
}