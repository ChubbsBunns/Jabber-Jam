using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class LobbySceneManager : NetworkBehaviour
{
    [SerializeField] GameObject[] LobbyUI;
    [SerializeField] Transform[] LobbyUI_Transform;
    [SerializeField] GameObject[] Maps;
    Dictionary<String, int> LobbyNameToIndexInMapsDictionary;
    [SerializeField] GameObject AudioRecorder;
    public NetworkedAudioManager networkedAudioManager;
    public GameManager gameManager;

    void Start()
    {
        // TODO: Hard code this shit: LobbyNameToIndexInMapsDictionary = 
        for (int i = 0; i < LobbyUI.Length; i++ )
        {
            LobbyUI_Transform.Append(LobbyUI[i].transform);
        }
        networkedAudioManager = FindAnyObjectByType<NetworkedAudioManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        if (networkedAudioManager == null)
        {
            Debug.LogError("No Network Audio Manager Found by Lobby Scene Manager");
        }
        if (gameManager == null)
        {
            Debug.LogError("No Network Audio Manager Found by Lobby Scene Manager");
        }
    }

    // Call this from your "Start Game" button
    public void OnStartGameButtonClicked()
    {
        // Tell the server someone clicked start
        RequestStartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[Server] Client {clientId} clicked Start Game. Broadcasting to all clients...");
        //Have a game manager, :
        // that has the number of rounds, 
        // holds all the sounds and corresponding palyer ids
        GameManager.Instance.playerAudioClips = NetworkedAudioManager.Instance.playerAudioClips;
        GameManager.Instance.roundDurationInSeconds = 60f;
        //Sets all the corresponding stuff, only on start game then start the round. Reset round logic in endRound for game manager

        
        //TODO: Pick the map, and send the number of sounds and type of sound to the map for mapping

        //Start the game by removing UI Elements
        StartGameClientRpc();
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        Debug.Log("[Client] Received Start Game command. Updating UI...");
        DisableLobbyUI();
        DisableAudioRecorderUI();
    }

    public void DisableLobbyUI()
    {
        for (int i = 0; i < LobbyUI.Length; i++)
        {
            LobbyUI[i].transform.position = new Vector3( LobbyUI[i].transform.position.x + 2000,  LobbyUI[i].transform.position.y,  LobbyUI[i].transform.position.z);
        }
        Debug.Log("Lobby UI disabled");
    }
    public void EnableLobbyUI()
    {
        for (int i = 0; i < LobbyUI.Length; i++)
        {
            LobbyUI[i].transform.position = new Vector3( LobbyUI[i].transform.position.x,  LobbyUI[i].transform.position.y,  LobbyUI[i].transform.position.z);
        }
    }

    public void DisableAudioRecorderUI()
    {
        Debug.Log("Disabled Audio Recorder UI");
        AudioRecorder.SetActive(false);
    }
    public void EnableAudioRecorderUI()
    {
        Debug.Log("Joined Lobby");
        AudioRecorder.SetActive(true);
    }
}
