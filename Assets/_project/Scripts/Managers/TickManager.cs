using System;
using UnityEngine;

// Should be in a common scene and loaded once, its alr if client and server use same tick manager
public class TickManager : MonoBehaviour
{
    public static event Action PreUpdate;
    public static event Action FrameUpdate;
    public static event Action PostUpdate;
    public static event Action NetworkTick;
    
    [SerializeField] private int _framerate = 60;
    
    [SerializeField] private int _networkTickrate = 60;
    private float _networkAccumulator = 0;


    private void Awake()
    {
        Application.targetFrameRate = _framerate;
        Physics2D.simulationMode = SimulationMode2D.Update;
        DontDestroyOnLoad(gameObject);
    }
    
    private static void OnPreUpdate()
    {
        // Debug.Log("Calling All PreUpdates");
        PreUpdate?.Invoke();
    }

    private static void OnFrameUpdate()
    {
        // Debug.Log("Calling All FrameUpdates");
        FrameUpdate?.Invoke();
    }

    private static void OnPostUpdate()
    {
        // Debug.Log("Calling All PostUpdates");
        PostUpdate?.Invoke();
    }
    
    private static void OnNetworkTick()
    {
        // Debug.Log("Calling All PostUpdates");
        NetworkTick?.Invoke();
    }

    private void Update()
    {
        _networkAccumulator += Time.deltaTime;
        if (_networkAccumulator >= 1f / _networkTickrate)
        {
            OnNetworkTick();
            _networkAccumulator -= 1f / _networkTickrate;
        }
        // Apparently this works, all pre updates are called BEFORE frame update
        OnPreUpdate();
        OnFrameUpdate();
    }

    private void LateUpdate()
    {
        OnPostUpdate();
    }
}
