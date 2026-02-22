using UnityEngine;

namespace _project.Scripts.GameLogic
{
    // Yes this is not good at all but I'm depressed so it'll do for now
    public class ConnectionParamsData : MonoBehaviour
    {
        public static ConnectionParamsData Instance { get; private set; }
        public string IpAddress {get; set;}
        public ushort Port {get; set;}

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}