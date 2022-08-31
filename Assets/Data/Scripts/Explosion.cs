using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private AudioClip clip_explode;

    void OnTriggerEnter2D(Collider2D other) {
        Vector3 dir = (other.transform.position - transform.position).normalized;

        if (other.tag == "Player") {
            Utils.GetPlayer().hurt(dir * 10f, Types.DamageType.Explosion);
        } else if (other.tag == "Enemy") {
            NpcBehavior behavior = other.gameObject.GetComponent<NpcBehavior>();

            if (behavior == null) {
                behavior = other.GetComponentInParent<NpcBehavior>();
            }
            
            behavior.hurt(dir * 10f, Types.DamageType.Explosion, 2);
        } else if (other.tag == "Impactable") {
            other.gameObject.GetComponent<Impactable>().GetImpact(dir * 10f, Types.DamageType.Explosion);
        }  else if (other.tag == "Breakable") {
            other.gameObject.GetComponent<Breakable>().hurt(2);
        }
    }

    public void Explode()
    {
        Utils.GetPlayer().cameraEffects.Shake(0.3f, 1000, 0.4f);
        GetComponent<AudioSource>().PlayOneShot(clip_explode);
        GetComponent<Animator>().SetTrigger("explode");
    }

    public void OnExplosionFinished()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.2f);
    }
}
