using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impactable : MonoBehaviour
{
    public AudioClip clip_hit;
    
    // Start is called before the first frame update

    public void GetImpact(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        GetRealImpact(force, damageType);
    }

    virtual protected void GetRealImpact(Vector2 force, Types.DamageType damageType) {
        GetComponent<AudioSource>().PlayOneShot(clip_hit);
        GetComponent<Rigidbody2D>().velocity = force;//.AddForce(force);
    }

}
