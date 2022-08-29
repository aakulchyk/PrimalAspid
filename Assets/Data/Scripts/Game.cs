
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Game : MonoBehaviour
{
    public static Game SharedInstance { get; private set; }

    public GameObject eventSystem; 

    public GameObject popupWindow;

    public SpeciesSelectMenu speciesSelectMenu;

    public bool isPopupOpen = false;

    public bool isMenuOpen = false;

    public bool isGameInProgress = false;
    [SerializeField] private string defaultScene = "Level_1_0";
    public static string currentScene;

    public Camera mainCamera;

    private string[] texts;
    private Queue<string> textQueue = new Queue<string>();

    public const int INITIAL_HP = 2;
    public const int INITIAL_STAMINA = 0;

    public Vector2 LastCheckPointPosition;
    public string LastCheckPointScene;

    private int popupCloseStatus = 0; // 0=ok, 1=yes. -1=no

    public GameObject closePopupButton;

    public int selectedSaveSlot;

    public NewMainMenu _mainMenu;

    public bool showBlackScreen = false;
    [SerializeField] private GameObject blackScreen;


    private void Awake()
    {
        Debug.Log("Game Awake");
        if (SharedInstance == null) {
            SharedInstance = this;
            DontDestroyOnLoad(gameObject);
            blackScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

            DontDestroyOnLoad(eventSystem);
        }
        else Destroy(gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        currentScene = SceneManager.GetActiveScene().name.Substring(3);
        Debug.Log("Game - OnLevelWasLoaded " + level + " " + currentScene);
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

    public string SaveFileFullPath(int slot)
    {
        return Application.persistentDataPath  + "/gamesave-" + slot +".save";
    }

    public void ClearGame()
    {
        File.Delete(SaveFileFullPath(selectedSaveSlot));
        File.Delete(SaveFileFullPath(selectedSaveSlot) + ".screen");
        PlayerPrefs.DeleteAll();
        Debug.Log("Game Deleted");
    }
    
    public void MakeScreenshot()
    {
        ScreenCapture.CaptureScreenshot(SaveFileFullPath(selectedSaveSlot) + ".screen");
    }

    public void SaveGame()
    {
        Debug.Log("Save Game");
        Save save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(SaveFileFullPath(selectedSaveSlot));
        bf.Serialize(file, save);
        file.Close();


        Debug.Log("Game Saved");
    }

    public bool CheckIfSaveSlotBusy(int slot)
    {
        return File.Exists(SaveFileFullPath(slot));
    }

    public void StartNewGame(int slot)
    {
        Debug.Log("Start New Game");
        selectedSaveSlot = slot;

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
        PlayerStats.Energy = 0;
        MakeScreenshot();
    }

    public void LoadGame(int slot)
    {
        selectedSaveSlot = slot;

        if (!File.Exists(SaveFileFullPath(selectedSaveSlot))) {
             Debug.Log("Cannot find a saved game");
             StartNewGame(slot);
             return;
        }
            
        Debug.Log("Loading Game " + Application.persistentDataPath);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(SaveFileFullPath(selectedSaveSlot), FileMode.Open);
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
        SceneManager.LoadScene("LD_" + currentScene);
        SceneManager.LoadScene("LA_" + currentScene, LoadSceneMode.Additive);

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
        PlayerStats.Energy = 0;
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

    public void OpenInGameMenu()
    {
        Time.timeScale = 0;
        isMenuOpen = true;
        _mainMenu.ShowInGameMenu();
        Utils.GetPlayer().enabled = false;
    }

    public void CloseInGameMenu()
    {
        Time.timeScale = 1;
        _mainMenu.HideInGameMenu();
        Utils.GetPlayer().enabled = true;
        isMenuOpen = false;
    }

    public void ClosePopup() {
        if (textQueue.Count > 0) {
            PopTextAndSetToPopup();
        } else {
            popupCloseStatus = 0;
            popupWindow.SetActive(false);
            Time.timeScale = 1;
            isPopupOpen = false;
        }
    }

    public void onPopupClose() {
        Debug.Log("close");
        popupCloseStatus = 0;
        popupWindow.SetActive(false);
    }

    public void onPopupAccepted() {
        popupCloseStatus = 1;
        popupWindow.SetActive(false);
    }

    public void onPopupRejected() {
        popupCloseStatus = -1;
        popupWindow.SetActive(false);
    }

    public void DarkenScreen() {
        blackScreen.GetComponent<Animator>().SetBool("loading", true);
    }

    public void LightenScreen() {
        blackScreen.GetComponent<Animator>().SetBool("loading", false);
    }
  
}
