using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    private Transform bar;
    void Start() {
        bar = transform.Find("Bar");
    }

    void Update() {
        bar.localScale = new Vector3(PlayerStats.Stamina, 1f);
    }
}
