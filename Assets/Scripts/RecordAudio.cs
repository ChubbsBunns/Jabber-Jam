using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    int lengthSec = 3;

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected!");
            return;
        }
        string device = Microphone.devices[0];
        recordedClip = Microphone.Start(device, false, lengthSec, 44100);
        Debug.Log("Recording started...");
    }

    public void StopRecording()
    {
        Microphone.End(null);
        Debug.Log("Recording stopped");

        if (recordedClip != null)
        {
            // Hand off to the NetworkedAudioManager — it owns all network logic
            NetworkedAudioManager.Instance.SubmitRecording(recordedClip);
        }
    }

    public void PlayRecording()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recording to play!");
            return;
        }
        audioSource.clip = recordedClip;
        audioSource.Play();
    }
}