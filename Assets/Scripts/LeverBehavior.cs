using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverBehavior : MonoBehaviour
{
    public GameObject target;
    public bool toggled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void switchTarget() {
        foreach (Transform trans in target.transform)
        {
            MovingSpikeBehavior script = trans.gameObject.GetComponent<MovingSpikeBehavior>();
            if (!script) continue;

            script.switchedOff = true;
        }

        GetComponent<AudioSource>().Play();
        toggled = true;
        transform.Rotate(0,0, -32f);
    }
}
