using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    public bool isDead = false;

    protected Animator anim;
    protected Rigidbody2D body;
    protected PlayerControl player;
    protected Transform playerTransform;

    public AudioClip clip_death;
    public int _hp = 1;

    protected void BaseInit()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D> ();
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        processCollision(collision);
    }

    protected virtual void processCollision(Collision2D collision) {
        Collider2D collider = collision.collider;
        if (collider.tag == "Throwable") {
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(0);
            }
        }

        if (collider.tag == "Spike")
        {
            hurt(0);
        }
    }

    public virtual void hurt(float force) {
        if (--_hp < 0 && !isDead) {
            die();
        }
    }

    protected virtual void die() {
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_death);
        isDead = true;
        anim.SetTrigger("die");
    }
}
