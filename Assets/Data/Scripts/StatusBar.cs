using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
public class StatusBar : MonoBehaviour
{
    private Transform bar;
    //GameObject[] hpBar;
    GameObject[] hpBar2;
    GameObject popup;

    [SerializeField] private GameObject moneyCount;
    [SerializeField] private GameObject bbCount;

    [SerializeField] private Slider staminaBar;
    void Start() {
        //hpBar = new GameObject[] {GameObject.Find("HP1"), GameObject.Find("HP2"), GameObject.Find("HP3")};
        hpBar2 = new GameObject[] {
            GameObject.Find("HP2.1"),
            GameObject.Find("HP2.2"),
            GameObject.Find("HP2.3"),
            GameObject.Find("HP2.4"),
            GameObject.Find("HP2.5")};


        foreach (var hp in hpBar2) {
            hp.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        }
        popup = GameObject.Find("PopUp");

        staminaBar.maxValue = PlayerStats.MaxStamina;
    }


    void Update() {
        float staminaScale = (float)PlayerStats.Stamina / (float)PlayerStats.MaxStamina;
        //bar.localScale = new Vector3(staminaScale * 400, 40);
        staminaBar.value = PlayerStats.Stamina;

        for (int i=0; i<PlayerStats.MAX_HP; i++) {
            hpBar2[i].GetComponent<Image>().color = PlayerStats.HP > i
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(0.9f, 0.1f, 0.2f, 0.2f);
        }

        bbCount.GetComponent<Text>().text = PlayerStats.BloodBodies.ToString();
        moneyCount.GetComponent<Text>().text = PlayerStats.Coins.ToString();
    }

    public void onPopupClose() {
        Debug.Log("close");
        popup.SetActive(false);
    }
}
