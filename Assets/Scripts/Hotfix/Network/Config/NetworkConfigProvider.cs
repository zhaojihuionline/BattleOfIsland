using UnityEngine;

/// <summary>
/// Lazy loads the network configuration ScriptableObject from Resources.
/// Keeps a cached instance for repeated access at runtime.
/// </summary>
public static class NetworkConfigProvider
{
    private const string ResourcePath = "NetworkConfig";
    private static NetworkConfigSO _cachedConfig;

    public static NetworkConfigSO Config
    {
        get
        {
            if (_cachedConfig == null)
            {
                _cachedConfig = Resources.Load<NetworkConfigSO>(ResourcePath);
                if (_cachedConfig == null)
                {
                    Debug.LogError($"NetworkConfig asset not found at Resources/{ResourcePath}.asset");
                }
            }

            return _cachedConfig;
        }
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearCache() => _cachedConfig = null;
#endif
}

