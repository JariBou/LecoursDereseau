namespace _project.Scripts.GameNetwork
{
    public static class NetOpCodes
    {
        public enum Server : ushort
        {
            PlayerConnected = 1,
            PlayerDisconnected = 2,
            PlayerData = 10,
            PlayerInputData = 67,
        }
        
        public enum Client : ushort
        {
            PlayerInfo = 1,
            PlayerPos = 2, // TEMP
            PlayerInput = 69,

        }
    }
}