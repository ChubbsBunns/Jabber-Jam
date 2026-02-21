using UnityEngine;


public class LevelAudioSourceManager : MonoBehaviour
{
    public static LevelAudioSourceManager Instance { get; private set;}
    public  JabberSource[] jabberSources;

    private void Awake()
    {
        Instance = this;
    }
    public void Activate_JabberSources()
    {
        for (int i = 0; i < jabberSources.Length; i++)
        {
            if (jabberSources[i].activated)
            {
                print( i + " activated");
                jabberSources[i].Activate();
            }
            else
            {
                print("Jabber source " + i + " not activated");
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
