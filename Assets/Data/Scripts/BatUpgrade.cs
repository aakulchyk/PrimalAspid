using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatUpgrade : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        collected = true;
        
        PlayerStats.BatWingsUnlocked = true;
        PlayerStats.MaxStamina = 2;

        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        _renderer.enabled = false;
        Destroy(this.gameObject, 1f);

        Game.SharedInstance.SetPopupText("Got upgrade", "You can now flap in the air by pressing \"A\" button / SPACE key");
        Game.SharedInstance.OpenPopup();

        Game.SharedInstance.SaveGame();
    }
}