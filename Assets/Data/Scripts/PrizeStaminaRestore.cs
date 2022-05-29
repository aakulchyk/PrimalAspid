using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeStaminaRestore : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.FullyRestoreStamina();
        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1f);
    }
}
