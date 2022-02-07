using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public Vector3 camOffset;
    public CameraEffects cameraEffects = null;


    void OnTriggerStay2D(Collider2D other) {
        if (other.tag == "Player") {
            cameraEffects.SetHardOffset(camOffset);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            cameraEffects.SetHardOffset(Vector3.zero);
        }
    }
}
