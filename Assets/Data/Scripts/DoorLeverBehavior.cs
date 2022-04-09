using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorLeverBehavior : LeverBehavior
{
    public bool closing = false;

    [SerializeField] private Collider2D tempCameraViewPort;
    IEnumerator OpenDoorAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        
        var water = target.GetComponent<Water>();
        if (water) {
            water.SetCameraFocusAndReflux(tempCameraViewPort);
        } else {
            // OUTDATED!
            Animator anim = target.GetComponent<Animator>();
            if (anim) {
                anim.SetTrigger(closing ? "Closed" : "Opened");
                yield return new WaitForSeconds(5f);
            } else {
                Debug.Log("Door: animation not found");
            }
        }

    }


    public override void switchSpecificTarget()
    {
        StartCoroutine(OpenDoorAfterDelay(0.5f));
    }
}
