using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthCollectable : Collectable
{
    public AudioClip clip_accum;

    public override void GetCollected() {
        if (collected) return;

        base.GetCollected();

        // apply stats
        if (PlayerStats.HalfLifeCollected == true) {
            StartCoroutine(Utils.FreezeFrameEffect(0.05f));
            GetComponent<AudioSource>().PlayOneShot(clip_accum);
            PlayerStats.MAX_HP++;
            PlayerStats.FullyRestoreHP();
            PlayerStats.HalfLifeCollected = false;
        } else {
            PlayerStats.HalfLifeCollected = true;
        }

        Game.SharedInstance.SaveGame();
    }
}
