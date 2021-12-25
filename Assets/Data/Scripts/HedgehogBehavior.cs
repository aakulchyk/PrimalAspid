﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HedgehogBehavior : NpcBehavior
{
    public AudioClip clip_crawl;
    public AudioClip clip_bristle;

    public AudioClip clip_reflect;
    public AudioClip clip_unbristle;

    public float _moveSpeed;

    private bool faceRight = false;

    public float _aggravateRadius;
    

    private float lastBristleTime;
    public const float COOLDOWN_TIME = 0.5f;


    private bool _isAnticipating = false;
    private bool _isBristled = false;
    
    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        lastBristleTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        if (isDead)
            return;

        if (!_isBristled && IsPlayerInRange()) {
            _isAnticipating = true;
            anim.SetBool("isBristled", true);
        }

        if (_isBristled && !IsPlayerInRange()) {
            anim.SetBool("isBristled", false);
        }
            
        bool isNotAbleToMove = isDead || _isAnticipating || _isBristled;
        float moveX = isNotAbleToMove ? 0f : faceRight ? _moveSpeed : -_moveSpeed;

        if (_knockback > 0) {
            moveX = body.velocity.x;
            --_knockback;
        } else if (isNotAbleToMove) {
            moveX = 0;
        }

        // move left/right
        body.velocity = new Vector2( moveX, body.velocity.y);

        if (_knockback>0)
            return;

        if (Math.Abs(moveX) > 0.01f && ! GetComponent<AudioSource>().isPlaying) {
                GetComponent<AudioSource>().PlayOneShot(clip_crawl);
        }

        // check ground ahead
        Vector3 v1 = new Vector3((faceRight? 1.5f : -1.5f), 0.05f, 0);
        Vector3 pos = transform.position + v1;
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
        Debug.DrawLine(pos, pos + new Vector3(0, -0.3f, 0), Color.green, 0.02f, false);
        if (!hit.collider) {
            if (IsPlayerInRange())
                Debug.Log("flip 1");    
            ChangeDirection();
        } else {
            RaycastHit2D hit1 = Physics2D.Raycast(pos, v1, 1f, LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall"));
            Debug.DrawLine(pos, pos + v1, Color.yellow, 0.02f, false);
            if (hit1.collider) {
                if (IsPlayerInRange())
                    Debug.Log("flip 2");
                ChangeDirection();
            }
        }
    }

    void ChangeDirection()
    {
        //Debug.Log("Hedgehog: flip");
        faceRight = !faceRight;
        flip();
    }

    bool IsPlayerInRange()
    {
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        return distP < _aggravateRadius;
    }

    void PlayBristleSound()
    {
        GetComponent<AudioSource>().PlayOneShot(clip_bristle);
    }

    void PlayUnbristleSound()
    {
        GetComponent<AudioSource>().PlayOneShot(clip_unbristle);
    }

    void onAnticipationFinished()
    {
        _isAnticipating = false;
        _isBristled = true;
    }

    void onReturnedToIdle()
    {
        _isBristled = false;
    }


    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        if (isDead) return;
        

        if (damageType == Types.DamageType.Spikes) {
            Debug.Log("Yozhiku pizdetz");
            _hp = -1;
        }

        if (damageType == Types.DamageType.PcHit)
        {
            if (_isBristled) {
                sounds.PlayOneShot(clip_reflect);
            } else {
                sounds.PlayOneShot(clip_hurt);
                knockback(force);
            }
            return;
        }

        if (_hp < 0 && !isDead) {
            Debug.Log("Yozhik DIE!!!");
            die();
        } else {
            anim.SetBool("hurt", true);
            sounds.PlayOneShot(clip_hurt);
        }
    }
   protected override void die()
   {
        base.die();
        //GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, 1f);
   }
}
