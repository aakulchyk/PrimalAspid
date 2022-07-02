
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

    [SerializeField] private Image blackScreen;
    public bool isPopupOpen = false;

    public bool isGameInProgress = false;
    [SerializeField] private string defaultScene = "Level_1_0";
    public static string currentScene;

    public Camera mainCamera;

    private string[] texts;
    private Queue<string> textQueue = new Queue<string>();

    public const int INITIAL_HP = 2;
    public const int INITIAL_STAMINA = 2;

    public Vector2 LastCheckPointPosition;
    public string LastCheckPointScene;


    private void Awake()
    {
        if (SharedInstance == null) {
            SharedInstance = this;
            DontDestroyOnLoad(gameObject);
            blackScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            blackScreen.enabled = true;
        }
        else Destroy(gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Game - OnLevelWasLoaded " + level + " " + currentScene);
        LightenScreenAsync();
    }

    public void MemorizeCheckPoint(Vector2 pos)
    {
        LastCheckPointPosition = new Vector2(pos.x, pos.y);
        LastCheckPointScene = currentScene;
    }

    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        save.Initialize();

        // gather player data
        save.currentScene = LastCheckPointScene;
        
        save.px = LastCheckPointPosition.x;
        save.py = LastCheckPointPosition.y;
        save.max_hp = PlayerStats.MAX_HP;
        save.max_stamina = PlayerStats.MaxStamina;

        // Abilities
        save.bat_unlocked = PlayerStats.BatWingsUnlocked;
        save.halflife_collected = PlayerStats.HalfLifeCollected;

        save.deaths = PlayerStats.Deaths;
        save.losses = PlayerStats.Losses;
        save.time = PlayerStats.Time;
        //save.npc_dead = PlayerStats.NpcsLostDead;

        save.coins = PlayerStats.Coins;
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
        PlayerPrefs.DeleteAll();
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

    public void StartNewGame()
    {
        ClearGame();
        SceneManager.LoadScene("LD_" + defaultScene);
        SceneManager.LoadScene("LA_" + defaultScene, LoadSceneMode.Additive);
        isGameInProgress = true;
        PlayerStats.MAX_HP = INITIAL_HP;
        PlayerStats.MaxStamina = INITIAL_STAMINA;
        PlayerStats.BatWingsUnlocked = false;
        PlayerStats.HalfLifeCollected = false;
        PlayerStats.FullyRestoreHP();
        PlayerStats.FullyRestoreStamina();
    }

    public void LoadGame()
    {
        //Time.timeScale = 1f;
        if (!File.Exists(Application.persistentDataPath + "/gamesave.save")) {
             Debug.Log("Cannot find a saved game");
             StartNewGame();
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
        PlayerStats.Deaths = save.deaths;
        PlayerStats.Losses = save.losses;
        PlayerStats.Time = save.time;

        PlayerStats.BloodBodies = save.bloodBodies;
        PlayerStats.Coins = save.coins;

        PlayerStats.BatWingsUnlocked = save.bat_unlocked;
        PlayerStats.HalfLifeCollected = save.halflife_collected;

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
        PlayerStats.MAX_HP = save.max_hp;
        PlayerStats.MaxStamina = save.max_stamina;
        PlayerStats.FullyRestoreHP();
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

    public void DarkenScreenAsync() {
        StartCoroutine(SetScreenAlphaAsync(0f, 1f, 0.5f));
    }

    public void LightenScreenAsync() {
        StartCoroutine(SetScreenAlphaAsync(1f, 0f, 0.5f));
    }

    public IEnumerator SetScreenAlphaAsync(float alphaBegin, float alphaEnd, float time) {
        float alpha = alphaBegin;

        if (alphaBegin < alphaEnd) {
            while (alpha < alphaEnd) {
                yield return null;
                alpha += 0.025f;
                blackScreen.color = new Color(0f, 0f, 0f, alpha);
            }
        } else {
            yield return new WaitForSeconds(0.4f);
            while (alpha > alphaEnd) {
                yield return null;
                alpha -= 0.025f;
                blackScreen.color = new Color(0f, 0f, 0f, alpha);
            }
        }
    }


    public void DarkenScreen() {
        SetScreenAlpha(0f, 1f, 0.5f);
    }

    public void LightenScreen() {
        SetScreenAlpha(1f, 0f, 0.1f);
    }

    IEnumerator waiter() {
        yield return null;
    }
    private void SetScreenAlpha(float alphaBegin, float alphaEnd, float time) {
        float alpha = alphaBegin;

        if (alphaBegin < alphaEnd) {
            while (alpha < alphaEnd) {
                StartCoroutine(waiter());
                alpha += 0.01f;
                blackScreen.color = new Color(0f, 0f, 0f, alpha);
            }
        } else {
            while (alpha > alphaEnd) {
                alpha -= 0.01f;
                blackScreen.color = new Color(0f, 0f, 0f, alpha);
            }
        }
    }
}
