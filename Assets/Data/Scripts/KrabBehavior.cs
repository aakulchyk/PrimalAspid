using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrabBehavior : CrawlerBehavior
{
    public AudioClip clip_jump;

    public float _jumpForce = 1600f;

    private bool isGrounded = true;

    enum KStates {
        Crawling,
        AnticipatingJump,
        Jumping,
        Grounding,
        GettingHit,
        KnockingBack,
        Dying,
        Dead
    }
    private KStates state;

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

        isGrounded = CheckGrounded();

        switch (state) {
            case KStates.Crawling: {
                Crawl();
                if (IsPlayerAbove() && IsJumpCooldownEnded()) {
                    state = KStates.AnticipatingJump;
                }
                break;
            }
            case KStates.AnticipatingJump:
                Debug.Log("Jump");
                lastJumpTime = Time.time;
                jump();
                state = KStates.Jumping;
                break;
            case KStates.Jumping:
                body.velocity = new Vector2(0, body.velocity.y);
                if (Time.time - lastJumpTime > 0.2f && isGrounded) {
                    state = KStates.Grounding;
                }
                break;
            case KStates.Grounding:
                Debug.Log("Ground");
                anim.SetBool("jump", false);
                state = KStates.Crawling;
                break;
            case KStates.GettingHit:
                // TODO: sound, animation, etc...
                state = KStates.KnockingBack;
                break;
            case KStates.KnockingBack:
                if (--_knockback <= 0)
                    state = KStates.Crawling;
                break;

            case KStates.Dead:
                return;
        }
    }

    void onGroundedChanged()
    {
        if (isGrounded) {
            //_isDashing = false;
            anim.SetBool("jump", false);
        }
    }

    bool IsPlayerAbove()
    {
        // jump
        // Cast a ray straight up.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up);
        // If it hits something...
        if (hit.collider != null) {
            if (hit.collider.tag == "Player")
                return true;
        }

        return false;
    }

    bool IsJumpCooldownEnded()
    {
        return Time.time - lastJumpTime > 0.5f;
    }

    private void jump()
    {
        anim.SetBool("jump", true);
        body.AddForce(new Vector2(0f, _jumpForce));
        //isGrounded = false;
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_jump);
    }

    protected override void processCollision(Collision2D hit)
    {
        Collider2D collider = hit.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            //isGrounded = true;
            anim.SetBool("jump", false);
        }

        base.processCollision(hit);
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        base.hurt(force, damageType);
        state = KStates.GettingHit;
    }
   protected override void die()
   {
        base.die();
   }
}
