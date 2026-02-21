using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSelector : MonoBehaviour
{
    public enum Map
    {
        Kitchen
    }

    public Map? chosenMap;

    public GameObject currentMap;

    public GameObject[] maps;

    public static Dictionary<Map, int> mapIndexes;

    void Start()
    {
        chosenMap = null;
        mapIndexes = new Dictionary<Map, int>();
        mapIndexes.Add(Map.Kitchen, 0);
    }

    public bool LoadChosenMap()
    {
        if (!chosenMap.HasValue)
        {
            Debug.LogError("No map has been chosen!");
            return false;
        }

        Map selected = chosenMap.Value;

        if (!mapIndexes.TryGetValue(selected, out int index))
        {
            Debug.LogError("Map index not found!");
            return false;
        }

        currentMap = Instantiate(maps[index]);
        if (currentMap == null)
        {
            return false;
        }
        return true;
    }

    public void UnloadChosenMap()
    {
        Destroy(currentMap);        
    }

    public void ChooseMap(string mapChosen)
    {
        switch (mapChosen)
        {
            case "Kitchen":
                chosenMap = Map.Kitchen;
                break;
            default:
                Debug.LogError("No Map chosen, please figure out sth bruv");
                break;

        }
    }
}
