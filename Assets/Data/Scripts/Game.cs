
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


    public static string defaultScene = "LD_Level_1_1";
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

        save.deaths = PlayerStats.Deaths;
        save.losses = PlayerStats.Losses;
        save.time = PlayerStats.Time;
        save.npc_saved = PlayerStats.NpcsSavedAlive;
        save.npc_dead = PlayerStats.NpcsLostDead;

        save.bloodBodies = PlayerStats.BloodBodies;

        // npc
        var npcObjs = GameObject.FindGameObjectsWithTag("FriendlyNPC");
        foreach (var obj in npcObjs) {
            Debug.Log("npc: " + obj.name);
            
            save.npcx.Add(obj.transform.position.x);
            save.npcy.Add(obj.transform.position.y);
            var npc = obj.GetComponent<NpcBehavior>();
            save.npcState.Add(npc.isDead ? 0 : 1); // TODO: extend
        }


        // platform

        // dynamic objects


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

        // load player data
        // assign loaded values
        //PlayerStats.playerSpawnCoord = new Vector2(save.px, save.py);
        PlayerStats.Deaths = save.deaths;
        PlayerStats.Losses = save.losses;
        PlayerStats.Time = save.time;
        PlayerStats.NpcsSavedAlive = save.npc_saved;
        PlayerStats.NpcsLostDead = save.npc_dead;

        PlayerStats.BloodBodies = save.bloodBodies;

        // load scene
        SpawnManager.SharedInstance.SetSpawn(new Vector2(save.px, save.py));
        SceneManager.LoadScene(currentScene);


        // load npcs
        {
            int i=0;
            var npcObjs = GameObject.FindGameObjectsWithTag("FriendlyNPC");
            foreach (var obj in npcObjs)
            {
                if (save.npcState==null || i >= save.npcState.Capacity)
                    break;

                var npc = obj.GetComponent<NpcBehavior>();
                npc.isDead = save.npcState[i]==0;
                var mpos = npc.transform.position;
                mpos.x = save.npcx[i];
                mpos.y = save.npcy[i];
                npc.transform.position = mpos;

                npc.LoadInActualState();
                i++;
            }
        }

        // TODO: dynamic objects

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
