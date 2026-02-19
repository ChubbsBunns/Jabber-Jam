using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class NetworkedAudioManager : NetworkBehaviour
{
    public static NetworkedAudioManager Instance { get; private set; }

    private const int CHUNK_SIZE = 512;

    // Stored audio per player (lives on all clients + server)
    private Dictionary<ulong, AudioClip> playerAudioClips = new Dictionary<ulong, AudioClip>();

    // Server-side chunk assembly tracking
    private Dictionary<ulong, List<byte[]>> serverPendingChunks = new Dictionary<ulong, List<byte[]>>();
    private Dictionary<ulong, int> serverExpectedChunks = new Dictionary<ulong, int>();
    private Dictionary<ulong, int[]> serverPendingMeta = new Dictionary<ulong, int[]>();

    // Client-side chunk assembly tracking
    private Dictionary<ulong, List<byte[]>> clientPendingChunks = new Dictionary<ulong, List<byte[]>>();
    private Dictionary<ulong, int> clientExpectedChunks = new Dictionary<ulong, int>();
    private Dictionary<ulong, int[]> clientPendingMeta = new Dictionary<ulong, int[]>();

    void Start()
    {
        Debug.Log("[AudioManager] Awake called");
        
        // Check if NetworkManager exists
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[AudioManager] NetworkManager.Singleton is NULL in Awake!");
        }
        else
        {
            Debug.Log($"[AudioManager] NetworkManager found. IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called by RecordAudio when the player stops recording
    public void SubmitRecording(AudioClip clip)
    {
        byte[] audioData = AudioClipToByteArray(clip);
        int totalChunks = Mathf.CeilToInt((float)audioData.Length / CHUNK_SIZE);

        Debug.Log($"Sending audio in {totalChunks} chunks ({audioData.Length} bytes total)");

        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * CHUNK_SIZE;
            int size = Mathf.Min(CHUNK_SIZE, audioData.Length - offset);

            byte[] chunk = new byte[size];
            System.Array.Copy(audioData, offset, chunk, 0, size);

            SubmitChunkServerRpc(chunk, i, totalChunks, clip.frequency, clip.channels);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitChunkServerRpc(byte[] chunk, int chunkIndex, int totalChunks, int frequency, int channels, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (chunkIndex == 0)
        {
            serverPendingChunks[clientId] = new List<byte[]>();
            serverExpectedChunks[clientId] = totalChunks;
            serverPendingMeta[clientId] = new int[] { frequency, channels };
            Debug.Log($"[Server] Started receiving from client {clientId}. Expecting {totalChunks} chunks.");
        }

        serverPendingChunks[clientId].Add(chunk);

        if (serverPendingChunks[clientId].Count == serverExpectedChunks[clientId])
        {
            Debug.Log($"[Server] All chunks received from {clientId}. Reassembling...");
            ServerReassembleAndBroadcast(clientId);
        }
    }

    void ServerReassembleAndBroadcast(ulong clientId)
    {
        int[] meta = serverPendingMeta[clientId];
        byte[] fullAudio = MergeChunks(serverPendingChunks[clientId]);

        // Register on server
        AudioClip clip = ByteArrayToAudioClip(fullAudio, meta[0], meta[1], $"Player_{clientId}_Audio");
        playerAudioClips[clientId] = clip;
        Debug.Log($"[Server] Registered audio for client {clientId}. Total players: {playerAudioClips.Count}");

        // Clean up
        serverPendingChunks.Remove(clientId);
        serverExpectedChunks.Remove(clientId);
        serverPendingMeta.Remove(clientId);

        // Broadcast chunks to all other clients
        int totalChunks = Mathf.CeilToInt((float)fullAudio.Length / CHUNK_SIZE);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * CHUNK_SIZE;
            int size = Mathf.Min(CHUNK_SIZE, fullAudio.Length - offset);

            byte[] chunk = new byte[size];
            System.Array.Copy(fullAudio, offset, chunk, 0, size);

            SyncChunkClientRpc(clientId, chunk, i, totalChunks, meta[0], meta[1]);
        }
    }

    [ClientRpc]
    void SyncChunkClientRpc(ulong playerId, byte[] chunk, int chunkIndex, int totalChunks, int frequency, int channels)
    {
        if (IsServer) return; // Server already registered it above

        if (chunkIndex == 0)
        {
            clientPendingChunks[playerId] = new List<byte[]>();
            clientExpectedChunks[playerId] = totalChunks;
            clientPendingMeta[playerId] = new int[] { frequency, channels };
            Debug.Log($"[Client] Receiving audio for player {playerId}. Expecting {totalChunks} chunks.");
        }

        clientPendingChunks[playerId].Add(chunk);

        if (clientPendingChunks[playerId].Count == clientExpectedChunks[playerId])
        {
            int[] meta = clientPendingMeta[playerId];
            byte[] fullAudio = MergeChunks(clientPendingChunks[playerId]);

            AudioClip clip = ByteArrayToAudioClip(fullAudio, meta[0], meta[1], $"Player_{playerId}_Audio");
            playerAudioClips[playerId] = clip;

            Debug.Log($"[Client] Registered audio for player {playerId}. Total players: {playerAudioClips.Count}");

            clientPendingChunks.Remove(playerId);
            clientExpectedChunks.Remove(playerId);
            clientPendingMeta.Remove(playerId);
        }
    }

    // ---- Helpers ----

    byte[] MergeChunks(List<byte[]> chunks)
    {
        int totalSize = 0;
        foreach (var c in chunks) totalSize += c.Length;

        byte[] merged = new byte[totalSize];
        int offset = 0;
        foreach (var c in chunks)
        {
            System.Array.Copy(c, 0, merged, offset, c.Length);
            offset += c.Length;
        }
        return merged;
    }

    byte[] AudioClipToByteArray(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(samples[i] * short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xFF);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }
        return bytes;
    }

    AudioClip ByteArrayToAudioClip(byte[] bytes, int frequency, int channels, string name)
    {
        float[] samples = new float[bytes.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
            samples[i] = value / (float)short.MaxValue;
        }

        AudioClip clip = AudioClip.Create(name, samples.Length / channels, channels, frequency, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ---- Debug ----

    public void LogAllRegisteredPlayers()
    {
        Debug.Log($"=== Registered Audio Clips ({playerAudioClips.Count}) ===");
        foreach (var kvp in playerAudioClips)
        {
            Debug.Log($"Player {kvp.Key}: {kvp.Value.name} | Length: {kvp.Value.length:F2}s");
        }
    }

    public AudioClip GetPlayerAudio(ulong clientId)
    {
        return playerAudioClips.ContainsKey(clientId) ? playerAudioClips[clientId] : null;
    }
}