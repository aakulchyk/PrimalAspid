using UnityEngine;

public class SpikeLeverBehavior : LeverBehavior
{

    public override void switchSpecificTarget()
    {
        foreach (Transform trans in target.transform)
        {
            MovingSpikeBehavior script = trans.gameObject.GetComponent<MovingSpikeBehavior>();
            if (!script) continue;

            script.switchedOff = true;
        }

        
    }
}
