﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Game : MonoBehaviour
{
    /*public Transform platform0;
    public Transform platform1;
    public Transform platform2;*/

    public GameObject popupWindow;
    public bool isPopupOpen = false;

    public bool isGameInProgress = false;
    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        save.Initialize();

        // gather player data
        PlayerControl player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        save.px = player.transform.position.x;
        save.py = player.transform.position.y;
        //save.hp =  PlayerStats.HP;

        save.deaths = PlayerStats.Deaths;
        save.losses = PlayerStats.Losses;
        save.time = PlayerStats.Time;
        save.npc_saved = PlayerStats.NpcsSavedAlive;
        save.npc_dead = PlayerStats.NpcsLostDead;

        save.bloodBodies = PlayerStats.BloodBodies;

        // npc
        var npcs = FindObjectsOfType<NpcWaitingBehavior>();  
        foreach (var npc in npcs) {
            save.maggotFound.Add(npc.waitSuccess);
        }

        // maggots
        var maggots = FindObjectsOfType<MaggotRescuedBehavior>();  
        foreach (var maggot in maggots) {

            Debug.Log("maggot: " + maggot.gameObject.name);
            save.maggotx.Add(maggot.transform.position.x);
            save.maggoty.Add(maggot.transform.position.y);
            save.maggotDead.Add(maggot.isDead);
        }

        // platform

        // popups


        return save;
    }

    public void ClearGame()
    {
        File.Delete(Application.persistentDataPath + "/gamesave.save");
        Debug.Log("Game Deleted");
    }

    public void SaveGame()
    {
        Debug.Log("Save Game");
        Save save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        if (!File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
             Debug.Log("Cannot find a saved game");
             return;
        }
            
        Debug.Log("Loading Game " + Application.persistentDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
        Save save = (Save)bf.Deserialize(file);
        file.Close();

        // assign loaded values
        //PlayerStats.HP = save.hp;

        PlayerStats.Deaths = save.deaths;
        PlayerStats.Losses = save.losses;
        PlayerStats.Time = save.time;
        PlayerStats.NpcsSavedAlive = save.npc_saved;
        PlayerStats.NpcsLostDead = save.npc_dead;

        PlayerStats.BloodBodies = save.bloodBodies;

        // player
        PlayerControl player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        var pos = player.transform.position;
        pos.x = save.px;
        pos.y = save.py;
        player.transform.position = pos;

        // npcs waiting
        // maggots
        {
            int i=0;
            var npcs = FindObjectsOfType<NpcWaitingBehavior>();  
            foreach (var npc in npcs)
            {
                if (save.maggotFound==null || i >= save.maggotFound.Capacity)
                    break;
                npc.waitSuccess = save.maggotFound[i];
                npc.LoadInActualState();
                i++;
            }
        }


        // load maggots
        {
            int i=0;
            var maggots = FindObjectsOfType<MaggotRescuedBehavior>();  
            foreach (var maggot in maggots)
            {
                if (save.maggotDead==null || i >= save.maggotDead.Capacity)
                    break;
                maggot.isDead = save.maggotDead[i];
                var mpos = maggot.transform.position;
                mpos.x = save.maggotx[i];
                mpos.y = save.maggoty[i];
                maggot.transform.position = mpos;

                maggot.LoadInActualState();
                i++;
            }
        }
        

        Debug.Log("Game Loaded");
    }


    public void SetPopupText(string text) {
        Text textWindow = popupWindow.transform.Find("Text").gameObject.GetComponent<Text>();
        textWindow.text = text;
    }
    public void OpenPopup() {
        isPopupOpen = true;
        Time.timeScale = 0;
        popupWindow.SetActive(true);
    }

    public void ClosePopup() {
        popupWindow.SetActive(false);
        Time.timeScale = 1;
        isPopupOpen = false;
    }
}
