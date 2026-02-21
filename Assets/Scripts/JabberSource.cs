using System.Collections;
using UnityEngine;

public class JabberSource : MonoBehaviour
{
    public AudioClip audioClip;
    public Animator anim;
    public AudioSource audioSource;

    public float upperLimit;
    public float lowerLimit;

    //This Flag is only for debugging purposes
    public bool activated = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (anim == null)
        
            Debug.LogError("No Animator attached to this jabber source" + gameObject.name );
        if (audioSource == null)
            Debug.LogError("No AudioSource attached to this jabber source: " + gameObject.name);
    }

    public void Set(AudioClip inputAudioClip)
    {
        audioClip = inputAudioClip;
        activated = true;
        print("Setting " + gameObject.name + " jabber source");
    }

    public void Activate()
    {
        if (audioClip == null)
        {
            return;
        }
        StartCoroutine(JabberRoutine());
        //TODO: Actually start the sound generation for the actions
    }

    public void Deactivate()
    {
        if (audioClip == null)
        {
            return;
        }
        activated = false;
        audioClip = null;
        StopCoroutine(JabberRoutine());
        audioSource.Stop();
        anim.Play("idle");
        //TODO: Stop the sound generation
    }

    private IEnumerator JabberRoutine()
    {
        while (activated)
        {
            float interval = Random.Range(lowerLimit, upperLimit);
            yield return new WaitForSeconds(interval);

            if (!activated) break;

            audioSource.PlayOneShot(audioClip);
            anim.Play("active");
        }
    }

}
