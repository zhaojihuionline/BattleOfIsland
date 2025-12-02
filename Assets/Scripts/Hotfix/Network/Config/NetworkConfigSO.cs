using UnityEngine;

/// <summary>
/// Centralized network endpoints editable via ScriptableObject asset.
/// Create an instance via the create menu and place it under Resources so it can be loaded at runtime.
/// </summary>
[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Configs/Network Config")]
public class NetworkConfigSO : ScriptableObject
{
    [Header("WebSocket Server")]
    public string serverEndpoint = "192.168.9.109:3250";
    public string serverPath = "/ws";

    [Header("HTTP Service")]
    public string httpBaseUrl = "http://192.168.9.109:8888/api/v1";
}

