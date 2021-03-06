﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorLeverBehavior : LeverBehavior
{
    public bool closing = false;
    IEnumerator OpenDoorAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        
        Animator anim = target.GetComponent<Animator>();
        if (anim) 
        {
            anim.SetTrigger(closing ? "Closed" : "Opened");
            yield return new WaitForSeconds(0.5f);

            AudioSource au = target.GetComponent<AudioSource>();

            if (au) 
                au.enabled = true;

            yield return new WaitForSeconds(5f);
            au.enabled = false;
        } else {
            Debug.Log("Door: animation not found");
        }
    }


    public override void switchSpecificTarget()
    {
        StartCoroutine(OpenDoorAfterDelay(0.5f));
    }
}
