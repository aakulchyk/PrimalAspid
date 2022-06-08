using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreHealthCollectable : Collectable
{
    public override void GetCollected() {
        if (collected) return;

        base.GetCollected();

        // apply stats
        PlayerStats.FullyRestoreHP();
    }
}
