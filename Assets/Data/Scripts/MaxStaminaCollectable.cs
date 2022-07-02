using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxStaminaCollectable : Collectable
{
    public AudioClip clip_accum;

    public override void GetCollected() {
        if (collected) return;

        base.GetCollected();

        // apply stats
        if (PlayerStats.HalfStaminaCollected == true) {
            StartCoroutine(Utils.FreezeFrameEffect(0.05f));
            GetComponent<AudioSource>().PlayOneShot(clip_accum);
            PlayerStats.MaxStamina++;
            PlayerStats.FullyRestoreStamina();
            PlayerStats.HalfStaminaCollected = false;
        } else {
            StartCoroutine(Utils.FreezeFrameEffect(0.01f));
            PlayerStats.HalfStaminaCollected = true;
        }

        Game.SharedInstance.SaveGame();
    }
}
