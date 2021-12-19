using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeStaminaRestore : PrizeBehavior
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        //PlayerStats.BloodBodies++;
        PlayerStats.Stamina = 1f;
        //GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1f);
    }
}
