using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeBloodBody : PrizeBehavior
{
    public AudioClip clip_accum;
    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.BloodBodies++;
        Debug.Log("Collect item. new BB=" + PlayerStats.BloodBodies);

        GetComponent<AudioSource>().volume = 1f;
        if (PlayerStats.BloodBodies>=10 && PlayerStats.HP < PlayerStats.MAX_HP) {
            GetComponent<AudioSource>().PlayOneShot(clip_accum);

            PlayerStats.HP++;
            PlayerStats.BloodBodies -= 10;
        } else {
            GetComponent<AudioSource>().PlayOneShot(clip_collect);
        }
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1f);

        
    }
}
