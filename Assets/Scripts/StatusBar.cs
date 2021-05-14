using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    private Transform bar;
    GameObject[] hpBar;
    void Start() {
        bar = transform.Find("Bar");

        hpBar = new GameObject[] {GameObject.Find("HP1"), GameObject.Find("HP2"), GameObject.Find("HP3")};
    }

    void Update() {
        bar.localScale = new Vector3(PlayerStats.Stamina, 1f);


        for (int i=0; i<3; i++) {
            hpBar[i].SetActive(PlayerStats.HP > i);
        }

        /*if (GameObject.Find("HP3")) {
            GameObject.Find("HP3").SetActive( PlayerStats.HP > 2);
        }

        if (GameObject.Find("HP2")) {
            GameObject.Find("HP2").SetActive( PlayerStats.HP > 1);
        }

        if (GameObject.Find("HP2")) {
            GameObject.Find("HP2").SetActive( PlayerStats.HP > 0);
        }*/

    }
}
