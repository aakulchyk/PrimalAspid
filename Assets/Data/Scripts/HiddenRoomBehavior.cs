using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenRoomBehavior : MonoBehaviour
{
    private GameObject sprite;
    public bool isPlayerInside = false;
    void Start()
    {
        sprite = transform.Find("Sprite").gameObject;
    }
    public void MakeVisible(bool value)
    {
        if (sprite)
            sprite.GetComponent<Renderer>().enabled = value;
        else
            Debug.LogError("No Sprite!!!");
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.tag == "Player") {
            isPlayerInside = true;
            MakeVisible(false);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            isPlayerInside = false;
            MakeVisible(true);
        }
    }
}
