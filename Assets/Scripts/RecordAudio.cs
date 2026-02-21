using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    int lengthSec = 3;

    public float normalizedPeak = 0.9f;
    public float boostValue = 1.7f;

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
            //AudioClip processedClip = NormalizeAndBoost(recordedClip, normalizedPeak, boostValue);
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

    private AudioClip NormalizeAndBoost(AudioClip clip, float targetPeak = 0.9f, float boost = 1.1f)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 1. Find peak
        float max = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            if (abs > max)
                max = abs;
        }

        // Avoid division by zero (silent clip)
        if (max < 0.0001f)
            return clip;

        // 2. Calculate normalization factor
        float normalizeFactor = targetPeak / max;

        // 3. Apply normalization + boost
        float finalMultiplier = normalizeFactor * boost;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= finalMultiplier;

            // 4. Clamp to prevent clipping
            samples[i] = Mathf.Clamp(samples[i], -1f, 1f);
        }

        // 5. Create new clip
        AudioClip newClip = AudioClip.Create(
            clip.name + "_Normalized",
            clip.samples,
            clip.channels,
            clip.frequency,
            false
        );

        newClip.SetData(samples, 0);

        return newClip;
    }

}