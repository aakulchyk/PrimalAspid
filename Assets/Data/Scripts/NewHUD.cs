using UnityEngine;
using UnityEngine.UIElements;


public class NewHUD : MonoBehaviour
{
    private VisualElement _hud;

    [SerializeField]
    private StyleSheet uss;

    private Label[] hpBar;
    private Label[] staminaBar;

    private Label coinsAmount;

    private Label hpShard;
    private Label staminaShard;

    private VisualElement _energyPict;

    private VisualElement _coins;


    private VisualElement _blackScreen;

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();

        //_start = uiDocument.rootVisualElement.Q<VisualElement>("HpBar");
        //_hp = uiDocument.rootVisualElement.Q<Label>("HpListView");


        hpBar = new Label[] {
            uiDocument.rootVisualElement.Q<Label>("hp0"),
            uiDocument.rootVisualElement.Q<Label>("hp1"),
            uiDocument.rootVisualElement.Q<Label>("hp2"),
            uiDocument.rootVisualElement.Q<Label>("hp3"),
            uiDocument.rootVisualElement.Q<Label>("hp4"),
            uiDocument.rootVisualElement.Q<Label>("hp5"),
            uiDocument.rootVisualElement.Q<Label>("hp6"),
            uiDocument.rootVisualElement.Q<Label>("hp7")
        };

        foreach (var hp in hpBar) {
            hp.style.visibility = Visibility.Hidden;
        }

        // TODO: stamina
        staminaBar = new Label[] {
            uiDocument.rootVisualElement.Q<Label>("st0"),
            uiDocument.rootVisualElement.Q<Label>("st1"),
            uiDocument.rootVisualElement.Q<Label>("st2"),
            uiDocument.rootVisualElement.Q<Label>("st3"),
            uiDocument.rootVisualElement.Q<Label>("st4"),
            uiDocument.rootVisualElement.Q<Label>("st5"),
            uiDocument.rootVisualElement.Q<Label>("st6"),
            uiDocument.rootVisualElement.Q<Label>("st7"),
            uiDocument.rootVisualElement.Q<Label>("st8"),
            uiDocument.rootVisualElement.Q<Label>("st9")
        };

        foreach (var st in staminaBar) {
            st.style.visibility = Visibility.Hidden;
        }

        // Coins
        //coins = uiDocument.rootVisualElement.Q<VisualElement>("Coins");
        coinsAmount = uiDocument.rootVisualElement.Q<Label>("CoinsAmount");
        
        // Shards
        hpShard = uiDocument.rootVisualElement.Q<Label>("HpShard");
        staminaShard = uiDocument.rootVisualElement.Q<Label>("StaminaShard");

        // Animal Energy Pictorgam

        _energyPict = uiDocument.rootVisualElement.Q<VisualElement>("EnergyPictorgam");

        _blackScreen = uiDocument.rootVisualElement.Q<VisualElement>("BlackScreen");

    }


    void Update()
    {
        // Black screen
        if (Game.SharedInstance.showBlackScreen) {
            _blackScreen.style.display = DisplayStyle.Flex;
        } else
            _blackScreen.style.display = DisplayStyle.None;

        // HP
        for (int i=0; i<PlayerStats.MAX_HP; i++) {
            hpBar[i].style.visibility = Visibility.Visible;
            if (PlayerStats.HP<=i)
                hpBar[i].AddToClassList("life-point-dark");
            else
                hpBar[i].RemoveFromClassList("life-point-dark");   
        }

        // STAMINA
        for (int i=0; i<PlayerStats.MaxStamina; i++) {
            staminaBar[i].style.visibility = Visibility.Visible;
            if (PlayerStats.Stamina<=i)
                staminaBar[i].AddToClassList("stamina-point-dark");
            else
                staminaBar[i].RemoveFromClassList("stamina-point-dark");
        }

        // MONEY
        coinsAmount.text = PlayerStats.Coins.ToString();

        // SHARDS
        hpShard.style.visibility = PlayerStats.HalfLifeCollected ? Visibility.Visible : Visibility.Hidden;
        staminaShard.style.visibility = PlayerStats.HalfStaminaCollected ? Visibility.Visible : Visibility.Hidden;

        // Animal pict
        _energyPict.style.visibility = PlayerStats.BatWingsUnlocked ? Visibility.Visible : Visibility.Hidden;

        
    }
}
