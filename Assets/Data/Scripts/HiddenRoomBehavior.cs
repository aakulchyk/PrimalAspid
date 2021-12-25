using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenRoomBehavior : MonoBehaviour
{
    private GameObject sprite;
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
}
