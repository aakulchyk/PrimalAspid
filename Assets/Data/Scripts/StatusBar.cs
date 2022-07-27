using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
public class StatusBar : MonoBehaviour
{
    private Transform bar;
    [SerializeField] private GameObject[] hpBar;
    [SerializeField] private GameObject[] staminaBar;

    [SerializeField] private GameObject hpShard;
    [SerializeField] private GameObject staminaShard;

    [SerializeField] private GameObject energyPanel;
    [SerializeField] private GameObject energyLevel;
    private Vector2 initialEnergyPanelPos;

    GameObject popup;

    [SerializeField] private GameObject moneyCount;
    //[SerializeField] private GameObject bbCount;

    //[SerializeField] private Slider staminaBar;


    public int popupCloseStatus = 0; // 0=ok, 1=yes. -1=no
    void Start() {
        /*hpBar = new GameObject[] {
            GameObject.Find("HP2.1"),
            GameObject.Find("HP2.2"),
            GameObject.Find("HP2.3"),
            GameObject.Find("HP2.4"),
            GameObject.Find("HP2.5"),
            GameObject.Find("HP2.6"),
            GameObject.Find("HP2.7"),
            };
        */

        foreach (var hp in hpBar) {
            hp.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        }
        foreach (var st in staminaBar) {
            st.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        }
        popup = GameObject.Find("PopUp");

        energyLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -76);
        initialEnergyPanelPos = energyPanel.GetComponent<RectTransform>().anchoredPosition;

    }


    void Update() {
        //staminaBar.maxValue = PlayerStats.MaxStamina;
        //float staminaScale = (float)PlayerStats.Stamina / (float)PlayerStats.MaxStamina;
        //bar.localScale = new Vector3(staminaScale * 400, 40);
        //staminaBar.value = PlayerStats.Stamina;

        for (int i=0; i<PlayerStats.MAX_HP; i++) {
            hpBar[i].GetComponent<Image>().color = PlayerStats.HP > i
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(0.9f, 0.1f, 0.2f, 0.2f);
        }

        for (int i=0; i<PlayerStats.MaxStamina; i++) {
            staminaBar[i].GetComponent<Image>().color = PlayerStats.Stamina > i
                ? new Color(0f, 0.5f, 1f, 1f)
                : new Color(0f, 0.2f, 0.6f, 0.2f);
        }

        hpShard.SetActive(PlayerStats.HalfLifeCollected);

        staminaShard.SetActive(PlayerStats.HalfStaminaCollected);

        //bbCount.GetComponent<Text>().text = PlayerStats.BloodBodies.ToString();
        moneyCount.GetComponent<Text>().text = PlayerStats.Coins.ToString();

        // -23 .. -77
        // 0  ... 100
        float coeff = (90-9)/100f;
        float y = ((float)PlayerStats.Energy * coeff) - 90;

        energyLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);

        if (PlayerStats.Energy > 99) {
            Vector2 randomVector = Random.insideUnitCircle.normalized;
            energyPanel.GetComponent<RectTransform>().anchoredPosition = initialEnergyPanelPos + randomVector;
        }
    }

    public void onPopupClose() {
        Debug.Log("close");
        popupCloseStatus = 0;
        popup.SetActive(false);
    }

    public void onPopupAccepted() {
        popupCloseStatus = 1;
        popup.SetActive(false);
    }

    public void onPopupRejected() {
        popupCloseStatus = -1;
        popup.SetActive(false);
    }
}
