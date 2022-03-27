using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : Collectable
{
    [SerializeField] private int value;

    public override void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.Coins += value;
        
        Debug.Log("Collect coins. new capital =" + PlayerStats.Coins);

        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        
        renderer.enabled = false;
        
        Destroy(this.gameObject, 1f);
    }
}
