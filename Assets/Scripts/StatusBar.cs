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

    GameObject bbCount;
    void Start() {
        bar = transform.Find("Bar");

        //hpBar = new GameObject[] {GameObject.Find("HP1"), GameObject.Find("HP2"), GameObject.Find("HP3")};
        hpBar2 = new GameObject[] {GameObject.Find("HP2.1"), GameObject.Find("HP2.2"), GameObject.Find("HP2.3")};


        popup = GameObject.Find("PopUp");

        bbCount = GameObject.Find("bbCount");
    }


    void Update() {
        bar.localScale = new Vector3(PlayerStats.Stamina*400, 40);


        for (int i=0; i<3; i++) {
            //hpBar[i].SetActive(PlayerStats.HP > i);
            hpBar2[i].SetActive(PlayerStats.HP > i);
        }

        bbCount.GetComponent<Text>().text = PlayerStats.BloodBodies.ToString();
    }

    public void onPopupClose() {
        Debug.Log("close");
        popup.SetActive(false);
    }
}
