using UnityEngine;


public class LevelAudioSourceManager : MonoBehaviour
{
    public static LevelAudioSourceManager Instance { get; private set;}
    [SerializeField] JabberSource[] jabberSources;

    public void Assign_Audio_To_JabberSources(AudioClip[] audioClips)
    {
        
    }

    public void Activate_JabberSources()
    {
        for (int i = 0; i < jabberSources.Length; i++)
        {
            if (jabberSources[i].activated)
            {
                jabberSources[i].Activate();
            }
        }
    }

    public void Deactivate_JabberSources()
    {
        for (int i = 0; i < jabberSources.Length; i++)
        {
            if (jabberSources[i].activated)
            {
                jabberSources[i].Deactivate();
            }
        }        
    }

    public void Clear_JabberSources()
    {
        
    }

    public JabberSource[] GetJabberSources()
    {
        return jabberSources;
    }


}
