using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeBloodBody : PrizeBehavior
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.BloodBodies++;
        Debug.Log("Collect item. new BB=" + PlayerStats.BloodBodies);

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1f);
    }
}
