using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenRoomBehavior : MonoBehaviour
{
    public GameObject sprite;
    public bool isPlayerInside = false;

    private Color color;
    //private float currentAlpha;
    void Start()
    {
        //sprite = transform.Find("Sprite").gameObject;
        if (sprite) {
            //color = sprite.GetComponent<SpriteShapeRenderer>().color;
            //currentAlpha = color.a;
        }
        else
            Debug.LogError("No Sprite!!!");
    }
    public void MakeVisible(bool value)
    {
        if (sprite) {
            //sprite.GetComponent<Renderer>().enabled = value;
            sprite.GetComponent<Animator>().SetBool("reveal", !value);
        }
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

