using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderHolder : Impactable
{
    bool open = false;
    public AudioClip clip_open;
    
    [SerializeField] private int health = 3;
    // Start is called before the first frame update
    
    protected override void GetRealImpact(Vector2 force, Types.DamageType damageType) {
        if (open) return;

        Debug.Log("GetRealImpact. HP: " + health);

        if (--health > 0) {
            base.GetRealImpact(force, damageType);
        } else {
            open = true;
            GetComponent<Animator>().SetBool("open", true);
            GetComponent<AudioSource>().PlayOneShot(clip_open);
            GetComponent<FixedJoint2D>().enabled = false;

        }
    }
}
