using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    public EventSystem eventSystem;


    [SerializeField] private AudioClip sound_select;

    public void Start()
    {
        //eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(gameObject);

        Debug.Log("UI Start");

        Game.SharedInstance.LightenScreenAsync();

        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // Root Menu
        _rootMenu = uiDocument.rootVisualElement.Q<VisualElement>("RootMenu");

        _start = uiDocument.rootVisualElement.Q<Button>("StartGame");
        _load = uiDocument.rootVisualElement.Q<Button>("LoadGame");
        _exit = uiDocument.rootVisualElement.Q<Button>("ExitGame");

        _start.RegisterCallback<ClickEvent>(ev => OnStartPressed());
        _start.RegisterCallback<NavigationSubmitEvent>(ev => OnStartPressed());
        
        _load.RegisterCallback<ClickEvent>(ev => OnLoadPressed());
        _load.RegisterCallback<NavigationSubmitEvent>(ev => OnLoadPressed());

        _exit.RegisterCallback<ClickEvent>(ev => Application.Quit());
        _exit.RegisterCallback<NavigationSubmitEvent>(ev => Application.Quit());

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
        _slot1.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot1, 0));

        _slot2.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot2, 1));
        _slot2.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot2, 0));

        _slot3.RegisterCallback<ClickEvent>(ev => SaveSlotSelected(_slot3, 2));
        _slot3.RegisterCallback<NavigationSubmitEvent>(ev => SaveSlotSelected(_slot3, 0));

        _back.RegisterCallback<ClickEvent>(ev => ShowMainMenu());
        _back.RegisterCallback<NavigationSubmitEvent>(ev => ShowMainMenu());

        // Rewrite Slot dialogue
        _rewriteSlotDlg = uiDocument.rootVisualElement.Q<VisualElement>("RewriteSlotDialog");
        _rewrite = uiDocument.rootVisualElement.Q<Button>("RewriteSlot");
        _backToSlotSelect = uiDocument.rootVisualElement.Q<Button>("BackToSlotSelect");
        
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
        _slot2.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _slot3.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _back.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        
        _rewrite.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());
        _backToSlotSelect.RegisterCallback<PointerEnterEvent>(ev => PlayButtonFocusSound());

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
        _start.UnregisterCallback<ClickEvent>(ev => OnStartPressed());
        _start.UnregisterCallback<PointerDownEvent>(ev => OnStartPressed());
    }

    // callbacks
    public void OnStartPressed()
    {
        PlayButtonFocusSound();
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
    public void OnLoadPressed()
    {
        PlayButtonFocusSound();
        _rootMenu.style.display = DisplayStyle.None;
        _saveSlotSelect.style.display = DisplayStyle.Flex;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
        _slot1.Focus();
    }

    public void SaveSlotSelected(Button slot, int id)
    {
        PlayButtonFocusSound();
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
        yield return Game.SharedInstance.SetScreenAlphaAsync(0f, 1f, 0.6f);
        Game.SharedInstance.LoadGame(id);
    }

    public void PlayButtonFocusSound()
    {
        GetComponent<AudioSource>().PlayOneShot(sound_select);
    }

    public void ShowMainMenu()
    {
        _rootMenu.style.display = DisplayStyle.Flex;
        _saveSlotSelect.style.display = DisplayStyle.None;
        _rewriteSlotDlg.style.display = DisplayStyle.None;
        _start.Focus();
    }

}
