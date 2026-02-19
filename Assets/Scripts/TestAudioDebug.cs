using UnityEngine;

public class TestAudioDebug : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ShowAllPlayerAudioLogs()
    {
        NetworkedAudioManager.Instance.LogAllRegisteredPlayers();
    }

}
