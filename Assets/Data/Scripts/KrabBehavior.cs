using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrabBehavior : NpcBehavior
{
    public AudioClip clip_crawl;
    public AudioClip clip_jump;

    public  float _followRadius = 7f;
    public float _moveSpeed = 1.5f;
    public float _jumpForce = 1600f;


    private bool isGrounded = true;

    private float lastJumpTime;
    
    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        lastJumpTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        bool isNotAbleToMove = isDead || !IsPlayerInRange() || !isGrounded;

        float moveX = isNotAbleToMove ? 0f : GetVectorToPlayer().x > 0 ? _moveSpeed : -_moveSpeed;

        if (_knockback > 0) {
            moveX = body.velocity.x;
            --_knockback;
        }
            
        // move left/right
        body.velocity = new Vector2( moveX, body.velocity.y );

        if (isNotAbleToMove)
            return;

        if (Math.Abs(moveX) > 0.01f && ! GetComponent<AudioSource>().isPlaying) {
                GetComponent<AudioSource>().PlayOneShot(clip_crawl);
        }

        // jump
        // Cast a ray straight up.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up);
        // If it hits something...
        if (hit.collider != null) {
            if (hit.collider.tag == "Player") {
                if (Time.time - lastJumpTime > 0.5f) {
                    jump();
                    lastJumpTime = Time.time;
                }
            }
        }
    }

    bool IsPlayerInRange()
    {
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        return distP < _followRadius;
    }

    private void jump()
    {
        anim.SetBool("jump", true);
        body.AddForce(new Vector2(0f, _jumpForce));
        isGrounded = false;
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_jump);
    }

    protected override void processCollision(Collision2D hit)
    {
        if (hit.gameObject.CompareTag ("Obstacle") || hit.gameObject.CompareTag ("Ground")) {
            isGrounded = true;
            anim.SetBool("jump", false);
        }

        base.processCollision(hit);
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        if (isDead) return;
        knockback(force);

        if (damageType == Types.DamageType.Spikes) {
            _hp = -1;
        }

        if (_hp < 0 && !isDead) {
            die();
        } else {
            anim.SetBool("hurt", true);
            sounds.PlayOneShot(clip_hurt);
        }
    }
   protected override void die()
   {
        base.die();
        GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, 5f);
   }
}
