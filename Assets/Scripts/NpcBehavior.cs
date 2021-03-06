﻿using System.Collections;
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

    public GameObject collectiblePrefab;     // the prefab of our collectible

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
            player.throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*7), true);
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

        if (collectiblePrefab) {
            StartCoroutine(dropCollectible());
        }

    }


    IEnumerator dropCollectible() {
        yield return new WaitForSeconds(0.4F);
        GameObject go = Instantiate(collectiblePrefab);

        go.transform.position = gameObject.transform.position + new Vector3(0,1,0);
        go.transform.rotation = Quaternion.identity;
    }

        // Update is called once per frame
    protected Vector3 GetVectorToPlayer()
    {
        // get direction to player
        Vector3 direction = playerTransform.position - transform.position;
        return direction.normalized;
    }

    protected virtual bool checkAccessibility(Transform wp)
    {
        Vector3 p = new Vector3(transform.position.x, transform.position.y, 0);

        float distM = Vector2.Distance(p, wp.position);
        Vector3 dir = wp.position - p;
        RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);
        if (hit.collider != null && hit.collider.tag != "Player") {
            Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
            return false;
        } else {
            Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
            return true;
        }
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
