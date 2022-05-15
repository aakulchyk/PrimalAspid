using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderBehavior : MonoBehaviour
{
    private AudioSource sounds;

    public AudioClip sound_hit;

    [SerializeField] protected GameObject deathParticles;
    
    // Start is called before the first frame update
    protected GrabbableBehavior grabbable = null;
    void Start()
    {
        sounds = GetComponent<AudioSource>();

        Transform t = transform.parent.Find("Grabbable");
        if (t) {
            grabbable = t.gameObject.GetComponent<GrabbableBehavior>();
        }
    }

    void Crash() {
        sounds.PlayOneShot(sound_hit);
        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }
        Destroy(this.gameObject, 1f);
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        //if (collision.relativeVelocity.magnitude < 1)
        //    return;
        if (collider.tag == "Enemy") {
            StartCoroutine(Utils.FreezeFrameEffect(0.01f));
            Crash();
            var enemy = collider.gameObject.GetComponent<NpcBehavior>();
            if (enemy) {
                enemy.hurt(Vector2.zero);
            }
        }

        if (collider.tag == "DestroyablePlatform") {
            Crash();
            DestroyablePlatform pl = collider.gameObject.GetComponent<DestroyablePlatform>();
            if (pl) {
                pl.OnCollapsed();
            }
        }
     }
}
