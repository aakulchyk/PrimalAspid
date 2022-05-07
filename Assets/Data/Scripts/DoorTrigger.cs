using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    private bool triggered = false;
    public Door door;

    /*void OnTriggerEnter2D(Collision2D collision)
    {
        Debug.Log("collision");
        if (collision.collider.tag == "Player") {
            if (door) {
                door.Close();
            }
        }
    }*/

    public void GetTriggered()
    {
        if (triggered)
            return;

        triggered = true;
        if (door) {
            door.Close();
        }
    }
}
