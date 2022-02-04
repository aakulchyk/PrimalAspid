using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeBatUpdate : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.MaxFlaps+=100;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        renderer.enabled = false;
        Destroy(this.gameObject, 1f);

        Game.SharedInstance.SetPopupText("Got upgrade", "You can now flap in the air by pressing \"A\" button / SPACE key");

        
    }
}
