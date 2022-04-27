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
            water.SetCameraFocusAndReflux();
        }

        var door = target.GetComponent<Door>();
        if (door) {
            door.Open();
        }
    }

    public override void switchSpecificTarget()
    {
        StartCoroutine(OpenDoorAfterDelay(0.2f));
        StartCoroutine(SetTemporaryCameraFocus());
        StartCoroutine(TemporaryDisablePlayerControl());
    }

    IEnumerator SetTemporaryCameraFocus()
    {
        yield return new WaitForSeconds(0.5f);
        // save old camera bounds
        CameraEffects cameraEffects = Utils.GetPlayer().cameraEffects;
        var oldConfiner = cameraEffects.CurrentConfiner();
        // temporary put camera focus on target
        cameraEffects.SetConfiner(tempCameraViewPort);
        yield return new WaitForSeconds(5f);
        cameraEffects.SetConfiner(oldConfiner);
    }

    IEnumerator TemporaryDisablePlayerControl()
    {
        Utils.GetPlayer().EnableControl(false);
        yield return new WaitForSeconds(5f);
        Utils.GetPlayer().EnableControl(true);
    }
}
