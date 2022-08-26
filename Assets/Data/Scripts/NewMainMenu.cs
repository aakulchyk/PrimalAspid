using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using System;
using System.IO;
using System.Collections;

public class NewMainMenu : MonoBehaviour
{
    private VisualElement _mainMenu;

    private VisualElement _titleMenu;
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

    private VisualElement _hud;

    private VisualElement _controlsMenu;


    public EventSystem eventSystem;


    [SerializeField] private AudioClip sound_select;
    [SerializeField] private AudioClip sound_press;

    public void Start()
    {
        //eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(gameObject);

        Debug.Log("UI Start");

        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // Main elements
        _mainMenu = uiDocument.rootVisualElement.Q<VisualElement>("MainMenu");
        _hud = uiDocument.rootVisualElement.Q<VisualElement>("HUD");
        _controlsMenu = uiDocument.rootVisualElement.Q<VisualElement>("ControlsMenu");

        // Root Menu
        _titleMenu = uiDocument.rootVisualElement.Q<VisualElement>("TitleMenu");

        _start = uiDocument.rootVisualElement.Q<Button>("StartGame");
        _load = uiDocument.rootVisualElement.Q<Button>("LoadGame");
        _exit = uiDocument.rootVisualElement.Q<Button>("ExitGame");


        // Save Slot select menu
        _saveSlotSelect = uiDocument.rootVisualElement.Q<VisualElement>("SaveSlotSelect");
        _slot1 = uiDocument.rootVisualElement.Q<Button>("Slot1");
        LoadSaveSlotInfo(_slot1, 0);
        _slot2 = uiDocument.rootVisualElement.Q<Button>("Slot2");
        LoadSaveSlotInfo(_slot2, 1);
        _slot3 = uiDocument.rootVisualElement.Q<Button>("Slot3");
        LoadSaveSlotInfo(_slot3, 2);
        _back = uiDocument.rootVisualElement.Q<Button>("BackToMain");
       
        // Rewrite Slot dialogue
        _rewriteSlotDlg = uiDocument.rootVisualElement.Q<VisualElement>("RewriteSlotDialog");
        _rewrite = uiDocument.rootVisualElement.Q<Button>("RewriteSlot");
        _backToSlotSelect = uiDocument.rootVisualElement.Q<Button>("BackToSlotSelect");
        
        RegisterCallbacks();
        
        ShowMainMenu();
        //DontDestroyOnLoad(gameObject);
    }


    void RegisterCallbacks()
    {
        // Root menu
        _start.RegisterCallback<ClickEvent>(ev => OnStartPressed());
        _start.RegisterCallback<NavigationSubmitEvent>(ev => OnStartPressed());
        
        _load.RegisterCallback<ClickEvent>(ev => OnLoadPressed());
        _load.RegisterCallback<NavigationSubmitEvent>(ev => OnLoadPressed());

        _exit.RegisterCallback<ClickEvent>(ev => Application.Quit());
        _exit.RegisterCallback<NavigationSubmitEvent>(ev => Application.Quit());

        // Save slots
        _slot1.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot1, 0));
        _slot1.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot1, 0));

        _slot2.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot2, 1));
        _slot2.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot2, 0));

        _slot3.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot3, 2));
        _slot3.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot3, 0));

        _back.RegisterCallback<ClickEvent>(ev => ShowMainMenu());
        _back.RegisterCallback<NavigationSubmitEvent>(ev => ShowMainMenu());
        _back.RegisterCallback<NavigationSubmitEvent>(ev => PlayButtonPressSound());

         // Rewrite Slot dialogue
        _rewrite.RegisterCallback<ClickEvent>(ev => StartCoroutine(DarkenScreenAndStartNewGame(selectedSlot)));
        _rewrite.RegisterCallback<NavigationSubmitEvent>(ev => StartCoroutine(DarkenScreenAndStartNewGame(selectedSlot)));

        _backToSlotSelect.RegisterCallback<ClickEvent>(ev => OnLoadPressed());
        _backToSlotSelect.RegisterCallback<NavigationSubmitEvent>(ev => OnLoadPressed());


        // Register button sound callbacks
        _start.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _start.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _load.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _load.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _exit.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _exit.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());
        
        _slot1.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot1.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _slot2.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot2.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _slot3.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot3.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _back.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _back.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());
        
        _rewrite.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _rewrite.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _backToSlotSelect.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _backToSlotSelect.RegisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

    }

    void UnregisterCallbacks()
    {
        // Root menu
        _start.UnregisterCallback<ClickEvent>(ev => OnStartPressed());
        _start.UnregisterCallback<NavigationSubmitEvent>(ev => OnStartPressed());
        
        _load.UnregisterCallback<ClickEvent>(ev => OnLoadPressed());
        _load.UnregisterCallback<NavigationSubmitEvent>(ev => OnLoadPressed());

        _exit.UnregisterCallback<ClickEvent>(ev => Application.Quit());
        _exit.UnregisterCallback<NavigationSubmitEvent>(ev => Application.Quit());

        // Save slots
        _slot1.UnregisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot1, 0));
        _slot1.UnregisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot1, 0));

        _slot2.UnregisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot2, 1));
        _slot2.UnregisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot2, 0));

        _slot3.UnregisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot3, 2));
        _slot3.UnregisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot3, 0));

        _back.UnregisterCallback<ClickEvent>(ev => ShowMainMenu());
        _back.UnregisterCallback<NavigationSubmitEvent>(ev => ShowMainMenu());
        _back.UnregisterCallback<NavigationSubmitEvent>(ev => PlayButtonPressSound());

         // Rewrite Slot dialogue
        _rewrite.UnregisterCallback<ClickEvent>(ev => StartCoroutine(DarkenScreenAndStartNewGame(selectedSlot)));
        _rewrite.UnregisterCallback<NavigationSubmitEvent>(ev => StartCoroutine(DarkenScreenAndStartNewGame(selectedSlot)));

        _backToSlotSelect.UnregisterCallback<ClickEvent>(ev => OnLoadPressed());
        _backToSlotSelect.UnregisterCallback<NavigationSubmitEvent>(ev => OnLoadPressed());


        // Register button sound callbacks
        _start.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _start.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _load.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _load.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _exit.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _exit.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());
        
        _slot1.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot1.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _slot2.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot2.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _slot3.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot3.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _back.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _back.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());
        
        _rewrite.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _rewrite.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

        _backToSlotSelect.UnregisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _backToSlotSelect.UnregisterCallback<NavigationMoveEvent>(ev => PlayButtonFocusSound());

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
        UnregisterCallbacks();
    }

    // callbacks
    public void OnStartPressed()
    {
        PlayButtonFocusSound();
        _newGame = true;
        _titleMenu.style.display = DisplayStyle.None;
        _saveSlotSelect.style.display = DisplayStyle.Flex;
        //StartCoroutine(DarkenScreenAndStartNewGame());
    }

    public IEnumerator DarkenScreenAndStartNewGame(int id)
    {
        Game.SharedInstance.DarkenScreen();
        yield return new WaitForSeconds(0.5f);
        // hide menu
        _mainMenu.style.display = DisplayStyle.None;
        _hud.style.display = DisplayStyle.Flex;
        UnregisterCallbacks();

        //yield return new WaitForSeconds(0.01f);


        // darken
        Game.SharedInstance.StartNewGame(id);
    }

        // callbacks
    public void OnLoadPressed()
    {
        PlayButtonPressSound();
        _titleMenu.style.display = DisplayStyle.None;
        _saveSlotSelect.style.display = DisplayStyle.Flex;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
        _slot1.Focus();
    }

    public void SaveSlotSelected(Button slot, int id)
    {
        PlayButtonPressSound();
        if (_newGame) {
            if (slot.text != "Empty") {
                selectedSlot = id;
                _saveSlotSelect.style.display = DisplayStyle.None;
                _rewriteSlotDlg.style.display = DisplayStyle.Flex;
                _backToSlotSelect.Focus();
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
        Game.SharedInstance.DarkenScreen();
        yield return new WaitForSeconds(0.5f);
        // hide menu
        _mainMenu.style.display = DisplayStyle.None;
        _hud.style.display = DisplayStyle.Flex;
        UnregisterCallbacks();

        //yield return new WaitForSeconds(0.1f);

        Game.SharedInstance.LoadGame(id);
    }

    public void PlayButtonFocusSound()
    {
        GetComponent<AudioSource>().PlayOneShot(sound_select);
    }

    public void PlayButtonPressSound()
    {
        GetComponent<AudioSource>().PlayOneShot(sound_press);
    }

    public void ShowMainMenu()
    {
        _mainMenu.style.display = DisplayStyle.Flex;
        _hud.style.display = DisplayStyle.None;
        _controlsMenu.style.display = DisplayStyle.None;


        _titleMenu.style.display = DisplayStyle.Flex;
        _saveSlotSelect.style.display = DisplayStyle.None;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
        _start.Focus();
    }

    public void ShowInGameMenu()
    {
        _hud.style.display = DisplayStyle.None;
        _controlsMenu.style.display = DisplayStyle.Flex;
    }

    public void HideInGameMenu()
    {
        _hud.style.display = DisplayStyle.Flex;
        _controlsMenu.style.display = DisplayStyle.None;
    }

}
