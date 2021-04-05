using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorLeverBehavior : LeverBehavior
{
    IEnumerator OpenDoorAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        
        Animator anim = target.GetComponent<Animator>();
        if (anim) 
        {
            anim.SetTrigger("Opened");
            yield return new WaitForSeconds(0.5f);

            AudioSource au = target.GetComponent<AudioSource>();

            if (au) 
                au.enabled = true;
        } else {
            Debug.Log("Door: animation not found");
        }
    }


    public override void switchSpecificTarget()
    {
        StartCoroutine(OpenDoorAfterDelay(0.5f));
    }
}
