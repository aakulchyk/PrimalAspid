using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthCollectable : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        base.GetCollected();

        // apply stats
        PlayerStats.MAX_HP++;
        PlayerStats.FullyRestoreHP();
    }
}
