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

    public string currentScene;
    public int coins;
    public int bloodBodies;
    public System.TimeSpan time;
    public int npc_saved, npc_dead;

    
    public List<float> npcx;
    public List<float> npcy;

    public List<int> npcState;


    public List<bool> platformOpened;


    public void Initialize()
    {
        npcx = new List<float>();
        npcy = new List<float>();

        npcState = new List<int>();

        platformOpened = new List<bool>();
    }
    
}
