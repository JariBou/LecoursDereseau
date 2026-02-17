namespace _project.Scripts.Network
{
    public static class NetOpCodes
    {
        public enum Server : ushort
        {
            PlayerConnected,
            PlayerPosData,
        }
        
        public enum Client : ushort
        {
            PlayerInfo,
        }
    }
}