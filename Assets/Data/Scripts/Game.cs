
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Game : MonoBehaviour
{
    public static Game SharedInstance { get; private set; }

    public GameObject popupWindow;
    public bool isPopupOpen = false;

    public bool isGameInProgress = false;


    public static string defaultScene = "LD_Level_1_0";
    public static string currentScene;

    private string[] texts;
    private Queue<string> textQueue = new Queue<string>();


    private void Awake()
    {
        if (SharedInstance == null) {
            SharedInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Game - OnLevelWasLoaded " + level + " " + currentScene);
    }

    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        save.Initialize();

        // gather player data

        save.currentScene = currentScene;
        
        save.px = Utils.GetPlayer().transform.position.x;
        save.py = Utils.GetPlayer().transform.position.y;
        //save.hp =  PlayerStats.HP;

        save.deaths = PlayerStats.Deaths;
        save.losses = PlayerStats.Losses;
        save.time = PlayerStats.Time;
        save.npc_saved = PlayerStats.NpcsSavedAlive;
        save.npc_dead = PlayerStats.NpcsLostDead;

        save.bloodBodies = PlayerStats.BloodBodies;

        // npc
        var npcs = FindObjectsOfType<RatFatherBehavior>();  
        foreach (var npc in npcs) {
            save.maggotFound.Add(npc.waitSuccess);
        }

        // maggots
        var maggots = FindObjectsOfType<BabyRatBehavior>();  
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

        Utils.GetPlayer().onSaveGame();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Game Saved");
    }

    public void StartNewGame()
    {
        ClearGame();
        SceneManager.LoadScene(defaultScene);
        isGameInProgress = true;
    }

    public void LoadGame()
    {
        //Time.timeScale = 1f;
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

        currentScene = save.currentScene;

        SceneManager.LoadScene(currentScene);

        // assign loaded values
        //PlayerStats.HP = save.hp;
        PlayerStats.Deaths = save.deaths;
        PlayerStats.Losses = save.losses;
        PlayerStats.Time = save.time;
        PlayerStats.NpcsSavedAlive = save.npc_saved;
        PlayerStats.NpcsLostDead = save.npc_dead;

        PlayerStats.BloodBodies = save.bloodBodies;

        // player
        var pos = Utils.GetPlayer().transform.position;
        pos.x = save.px;
        pos.y = save.py;
        Utils.GetPlayer().transform.position = pos;

        // npcs waiting
        // maggots
        {
            int i=0;
            var npcs = FindObjectsOfType<RatFatherBehavior>();  
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
            var maggots = FindObjectsOfType<BabyRatBehavior>();  
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
        isGameInProgress = true;
    }

    public void SetPopupText(string title, string text) {
        Text titleWindow = popupWindow.transform.Find("Title").gameObject.GetComponent<Text>();
        titleWindow.text = title;

        textQueue.Enqueue(text);
        PopTextAndSetToPopup();
    }


    public void SetPopupTexts(string title, string[] texts) {
        Text titleWindow = popupWindow.transform.Find("Title").gameObject.GetComponent<Text>();
        titleWindow.text = title;

        foreach (var t in texts) {
            textQueue.Enqueue(t);
        }

        PopTextAndSetToPopup();
    }

    public void PopTextAndSetToPopup()
    {
        if (textQueue.Count > 0) {
            var t = textQueue.Dequeue();
            Text textWindow = popupWindow.transform.Find("Text").gameObject.GetComponent<Text>();
            textWindow.text = t;
        }
    }

    public void OpenPopup() {
        isPopupOpen = true;
        Time.timeScale = 0;
        popupWindow.SetActive(true);
    }

    public void ClosePopup() {
        if (textQueue.Count > 0) {
            PopTextAndSetToPopup();
        } else {
            popupWindow.SetActive(false);
            Time.timeScale = 1;
            isPopupOpen = false;
        }
    }
}
