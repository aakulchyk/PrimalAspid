using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{
    public float px = 0.0f;
    public float py = 0.0f;

    public int hp = 0;

    public int deaths, losses;
    public System.TimeSpan time;
    public int npc_saved, npc_dead;

    
    public List<float> maggotx;
    public List<float> maggoty;

    public List<bool> maggotFound;
    public List<bool> maggotDead;


    public List<bool> platformOpened;


    public void Initialize()
    {
        maggotx = new List<float>();
        maggoty = new List<float>();

        maggotFound = new List<bool>();
        maggotDead = new List<bool>();

        platformOpened = new List<bool>();

    }
    
}
