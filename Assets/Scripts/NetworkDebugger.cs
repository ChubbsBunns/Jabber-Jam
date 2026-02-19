using Unity.Netcode;
using UnityEngine;

public class NetworkDebugger : MonoBehaviour
{
    void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnServerStarted += OnServerStarted;
            nm.OnClientConnectedCallback += OnClientConnected;
            nm.OnClientDisconnectCallback += OnClientDisconnected;
            
            Debug.Log($"[NetworkDebugger] NetworkManager found. Player Prefab assigned: {nm.NetworkConfig.PlayerPrefab != null}");
            if (nm.NetworkConfig.PlayerPrefab != null)
            {
                Debug.Log($"[NetworkDebugger] Player Prefab: {nm.NetworkConfig.PlayerPrefab.name}");
            }
        }
        else
        {
            Debug.LogError("[NetworkDebugger] NetworkManager.Singleton is NULL!");
        }
    }

    void OnServerStarted()
    {
        Debug.Log("[NetworkDebugger] Server started!");
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkDebugger] Client {clientId} connected. Total connected: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        // Check if player object was spawned
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                Debug.Log($"[NetworkDebugger] Client {clientId} has PlayerObject: {client.PlayerObject.name}");
            }
            else
            {
                Debug.LogWarning($"[NetworkDebugger] Client {clientId} has NO PlayerObject!");
            }
        }
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkDebugger] Client {clientId} disconnected");
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}