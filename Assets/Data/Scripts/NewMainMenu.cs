using UnityEngine;
using UnityEngine.UIElements;

using System;
using System.IO;
using System.Collections;

public class NewMainMenu : MonoBehaviour
{
    private VisualElement _rootMenu;
    private Button _start;
    private Button _load;
    private Button _exit;

    private VisualElement _saveSlotSelect;
    private Button _slot1;
    private Button _slot2;
    private Button _slot3;
    private Button _back;

    private bool _newGame = false;


    private VisualElement _rewriteSlotDlg;
    private Button _rewrite;
    private Button _backToSlotSelect;

    private int selectedSlot;

    public void Start()
    {
        Debug.Log("UI Start");

        Game.SharedInstance.LightenScreenAsync();

        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // Root Menu
        _rootMenu = uiDocument.rootVisualElement.Q<VisualElement>("RootMenu");

        _start = uiDocument.rootVisualElement.Q<Button>("StartGame");
        _load = uiDocument.rootVisualElement.Q<Button>("LoadGame");
        _exit = uiDocument.rootVisualElement.Q<Button>("ExitGame");

        _start.RegisterCallback<ClickEvent>(OnStartPressed);
        _load.RegisterCallback<ClickEvent>(OnLoadPressed);
        _exit.RegisterCallback<ClickEvent>(ev => Application.Quit());


        // Save Slot select menu
        _saveSlotSelect = uiDocument.rootVisualElement.Q<VisualElement>("SaveSlotSelect");
        _slot1 = uiDocument.rootVisualElement.Q<Button>("Slot1");
        LoadSaveSlotInfo(_slot1, 0);
        _slot2 = uiDocument.rootVisualElement.Q<Button>("Slot2");
        LoadSaveSlotInfo(_slot2, 1);
        _slot3 = uiDocument.rootVisualElement.Q<Button>("Slot3");
        LoadSaveSlotInfo(_slot3, 2);
        _back = uiDocument.rootVisualElement.Q<Button>("BackToMain");

        _slot1.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot1, 0));
        _slot2.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot2, 1));
        _slot3.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot3, 2));
        _back.RegisterCallback<ClickEvent>(ev => ShowMainMenu());


        // Rewrite Slot dialogue
        _rewriteSlotDlg = uiDocument.rootVisualElement.Q<VisualElement>("RewriteSlotDialog");
        _rewrite = uiDocument.rootVisualElement.Q<Button>("RewriteSlot");
        _backToSlotSelect = uiDocument.rootVisualElement.Q<Button>("BackToSlotSelect");

        _rewrite.RegisterCallback<ClickEvent>(ev => StartCoroutine(DarkenScreenAndStartNewGame(selectedSlot)));
        _backToSlotSelect.RegisterCallback<ClickEvent>(OnLoadPressed);

        ShowMainMenu();
        
    }

    private void LoadSaveSlotInfo(Button slot, int id)
    {
        if (Game.SharedInstance.CheckIfSaveSlotBusy(id)) {
            DateTime time = File.GetCreationTime(Game.SharedInstance.SaveFileFullPath(id));
            var fileInfo = "Save file created on " + time.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Debug.Log(fileInfo);
            slot.text = fileInfo;

            byte[] fileData = File.ReadAllBytes(Game.SharedInstance.SaveFileFullPath(id) + ".screen");
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            slot.style.backgroundImage = new StyleBackground(tex);
        } else
            slot.text = "Empty";
    }

    private void OnDestroy()
    {
        _start.UnregisterCallback<ClickEvent>(OnStartPressed);
    }

    // callbacks
    public void OnStartPressed(ClickEvent evt)
    {
        _newGame = true;
        _rootMenu.style.display = DisplayStyle.None;
        _saveSlotSelect.style.display = DisplayStyle.Flex;
        //StartCoroutine(DarkenScreenAndStartNewGame());
    }

    public IEnumerator DarkenScreenAndStartNewGame(int id)
    {
        // darken
        yield return Game.SharedInstance.SetScreenAlphaAsync(0f, 1f, 0.6f);
        Game.SharedInstance.StartNewGame(id);
    }

        // callbacks
    public void OnLoadPressed(ClickEvent evt)
    {
        _rootMenu.style.display = DisplayStyle.None;
        _saveSlotSelect.style.display = DisplayStyle.Flex;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
    }

    public void SaveSlotSelected(Button slot, int id)
    {
        if (_newGame) {
            if (slot.text != "Empty") {
                selectedSlot = id;
                _saveSlotSelect.style.display = DisplayStyle.None;
                _rewriteSlotDlg.style.display = DisplayStyle.Flex;
                return;
            }
            
            StartCoroutine(DarkenScreenAndStartNewGame(id));
            return;
        }

        if (slot.text != "Empty") {
            StartCoroutine(DarkenScreenAndStartLoadGame(id));
        }
    }

    public IEnumerator DarkenScreenAndStartLoadGame(int id)
    {
        yield return Game.SharedInstance.SetScreenAlphaAsync(0f, 1f, 0.6f);
        Game.SharedInstance.LoadGame(id);
    }

    public void ShowMainMenu()
    {
        _rootMenu.style.display = DisplayStyle.Flex;
        _saveSlotSelect.style.display = DisplayStyle.None;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
    }

}
