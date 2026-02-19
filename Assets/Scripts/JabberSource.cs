using UnityEngine;

public class JabberSource : MonoBehaviour
{
    public AudioClip audioClip;

    public float upperLimit;
    public float lowerLimit;

    //This Flag is only for debugging purposes
    public bool activated = false;

    void Start()
    {
        activated = false;
        audioClip = null;
    }

    public void Set(AudioClip inputAudioClip)
    {
        audioClip = inputAudioClip;
        activated = true;
    }

    public void Activate()
    {
        if (audioClip == null)
        {
            return;
        }
        
        //TODO: Actually start the sound generation for the actions
    }

    public void Deactivate()
    {
        activated = false;
        audioClip = null;
        //TODO: Stop the sound generation
    }
}
