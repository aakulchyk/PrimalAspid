using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    //public Vector3 camOffset;
    public CameraEffects cameraEffects = null;
    [SerializeField] private Collider2D confiner;

    [SerializeField] private Collider2D mainSceneConfiner;

    public bool isMain = false;
    private bool _confinerSet = false;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player" && !_confinerSet) {
            if (cameraEffects == null) {
                if (Utils.GetPlayer())
                    cameraEffects = Utils.GetPlayer().cameraEffects;
                //return;
            }

            cameraEffects.SetConfiner(confiner);
            _confinerSet = true;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            
            if (!isMain) {
                cameraEffects.SetConfiner(mainSceneConfiner);
            }
            _confinerSet = false;
        }
    }
}
