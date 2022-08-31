using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatUpgrade : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;
        
        PlayerStats.RatExplosivesUnlocked = true;

        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        _renderer.enabled = false;
        Destroy(this.gameObject, 1f);

        Game.SharedInstance.SetPopupText("Unblocked Rat Powers", "Now you can set bombs with long pressing (B) button");
        Game.SharedInstance.OpenPopup();

        Game.SharedInstance.SaveGame();
    }
}
