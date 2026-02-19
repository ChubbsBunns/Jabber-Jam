using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;

public class GameManager : MonoBehaviour
{
    //Game manager should manage all the random logic.
    //It then parses the instances using Network AudioManager 
    //This ensures that the random indexes are constant throughout all characters
    public static GameManager Instance { get; private set; }
    public Dictionary<ulong, AudioClip> playerAudioClips = new Dictionary<ulong, AudioClip>();

    public String[] playerIDs;
    public String imposterPlayerID;
    public float roundDurationInSeconds;
    public int maxNumRounds = 2;
    public int currNumRound = 0;
    public float countdownMaxTime = 3.0f;
    public float currRoundStartCountDownTime;
    public float currRoundCountdown = 0;


    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //TODO: Port the randomization algo here,
    // Host and list all the corresponding map potential audio sources.
    // Once loaded, pick the imposter, and update the map with the corersponding audios
    void UpdateAudioFiles()
    {
        
    }

    public void SetupRound()
    {
        currRoundStartCountDownTime = countdownMaxTime;
        currRoundCountdown = roundDurationInSeconds;
        currNumRound += 1;
        if (currNumRound > maxNumRounds)
        {
            
            EndGame();
        }
        imposterPlayerID = GetRandomPlayerID();


        //This section here is to assign the audio clips dynamically. Im too lazy to refactor this shit
        List<AudioClip> audioClipsForRound = GetAudioClipLists();
        JabberSource[] jabberSources = LevelAudioSourceManager.Instance.GetJabberSources();
        int[] JabberSourcesIndexToAssign = Get_Random_Indexes((playerIDs.Length + 1), jabberSources.Length);
        if (JabberSourcesIndexToAssign.Length != audioClipsForRound.Count)
        {
            Debug.LogError("Number of audio clips in audioClips For Round is not the same asJabber sources index to assign");
        }
        else
        {
            Debug.Log("Number of Audio Clips in audioclipsforround are the same as number of jabber sources to assign");
        }

        for (int i = 0; i < JabberSourcesIndexToAssign.Length; i++)
        {
            jabberSources[JabberSourcesIndexToAssign[i]].audioClip = audioClipsForRound[i];
        }

        StartCoroutine(CountdownTillRoundStart());
    }

    public IEnumerator CountdownTillRoundStart()
    {
        yield return new WaitForSeconds(currNumRound);   
        StartRound();
    }

    public void StartRound()
    {
        Debug.Log("ROUND STARTED");
        LevelAudioSourceManager.Instance.Activate_JabberSources();
    }

    public IEnumerator CountdownTilleRoundEnds()
    {
        yield return new WaitForSeconds(roundDurationInSeconds);
        //TODO: Start some UI Timer for the timer
        EndRound();
    }

    public void EndRound()
    {
        //Check if its the last round, if it is, then go back to lobby, otherwise start anth round
        LevelAudioSourceManager.Instance.Deactivate_JabberSources();
    }

    public List<AudioClip> GetAudioClipLists()
    {
        List<AudioClip> allClips = new List<AudioClip>(playerAudioClips.Values);

        // Create a new list based on allClips
        List<AudioClip> withImposterAppended = new List<AudioClip>(allClips);

        if (ulong.TryParse(imposterPlayerID, out ulong imposterKey))
        {
            if (playerAudioClips.TryGetValue(imposterKey, out AudioClip imposterClip))
            {
                withImposterAppended.Add(imposterClip);
            }
        }

        return withImposterAppended;
    }

    public string GetRandomPlayerID()
    {
        if (playerIDs == null || playerIDs.Length == 0)
            return null;

        System.Random random = new System.Random();
        int index = random.Next(playerIDs.Length);
        return playerIDs[index];
    }

    private int[] Get_Random_Indexes(int numAssigned, int numMax)
    {
        System.Random rand = new System.Random();
        if (numAssigned > numMax)
        {
            Debug.LogError("Num assigned more than num max");
        }
        int[] numbers = Enumerable.Range(0, numMax).ToArray();

        // Fisher–Yates shuffle
        for (int i = numbers.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
        }

        for (int i = 0; i < numAssigned; i++)
        {
            Console.WriteLine(numbers[i]);
        }  
        return numbers.Take(numAssigned).ToArray();
    }

    public void EndGame()
    {
        //TODO: End the game, show leaderboard
    }

}
