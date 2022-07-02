using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    //public Vector3 camOffset;
    public CameraEffects cameraEffects = null;
    [SerializeField] private Collider confiner;
    [SerializeField] private Collider mainSceneConfiner;

    [SerializeField] private Collider2D confiner2D;
    [SerializeField] private Collider2D mainSceneConfiner2D;

    public bool isMain = false;
    private bool _confinerSet = false;

    void OnTriggerStay2D(Collider2D other) {
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
