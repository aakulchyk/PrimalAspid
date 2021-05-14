using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    public bool isDead = false;

    public bool captured = false;  
    
    protected Animator anim;
    protected Rigidbody2D body;
    protected PlayerControl player;
    protected Transform playerTransform;

    protected AudioSource sounds;

    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public int _hp = 1;

    public bool invulnerable = false;

    protected void BaseInit()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D> ();
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;

        sounds =  GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        
        processCollision(collision);
    }

    protected virtual void processCollision(Collision2D collision) {
       // Debug.Log("NPC collision");
        Collider2D collider = collision.collider;
        if (collider.tag == "Throwable") {
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(0);
            }
        }

        if (collider.tag == "Spike")
        {
            //Debug.Log("NPC Collide Spike");
            hurt(0);
        }
    }

    public virtual void hurt(float force) {

        if (isDead) return;

        if (player.isPulling && player.GetComponent<FixedJoint2D>().connectedBody == GetComponent<Rigidbody2D>()) {
            player.releaseBody();
            player.throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*6));
            invulnerable = true;
        }

        Debug.Log("NPC Hurt");
        if (--_hp < 0 && !isDead) {
            die();
        } else {
            sounds.PlayOneShot(clip_hurt);
        }
    }

    protected virtual void die() {
        sounds.Stop();
        sounds.PlayOneShot(clip_death);
        isDead = true;
        anim.SetTrigger("die");
    }

        // Update is called once per frame
    protected Vector3 GetVectorToPlayer()
    {
        // get direction to player
        Vector3 direction = playerTransform.position - transform.position;
        return direction.normalized;
    }

    public virtual void getCaptured()
    {
        captured = true;
    }

    public virtual void getReleased()
    {
        captured = false;
    }

    public virtual void LoadInActualState()
    {
        Debug.Log("Dummy LoadInActualState");
    }
}
