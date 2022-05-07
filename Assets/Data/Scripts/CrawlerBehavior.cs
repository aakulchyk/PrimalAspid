using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlerBehavior : NpcBehavior
{
    public AudioClip clip_crawl;

    public float _moveSpeed;

    private bool faceRight = false;

    public float _aggravateRadius;

    private float _visibilityRadius = 50f;

    enum States {
        Crawling,
        GettingHit,
        KnockingBack,
        Dying,
        Dead
    }

    private States state;


    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        state = States.Crawling;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        switch (state) {
            case States.Crawling: {
                Crawl();
                break;
            } 
            case States.GettingHit:
                // TODO: sound, animation, etc...
                state = States.KnockingBack;
                break;
            case States.KnockingBack:
                if (--_knockback <= 0)
                    state = States.Crawling;
                break;

            case States.Dead:
                return;
        }
    }

    protected void Crawl()
    {
        float moveX = faceRight ? _moveSpeed : -_moveSpeed;
        body.velocity = new Vector2( moveX, body.velocity.y);
        if (Math.Abs(moveX) > 0.01f && !sounds.isPlaying)
            sounds.PlayOneShot(clip_crawl);
                
        if (isObstacleOrPlatformEdgeOnWay())
            ChangeDirection();
    }

    protected bool isObstacleOrPlatformEdgeOnWay()
    {
        // check ground ahead
        Vector3 v1 = new Vector3((faceRight? 1.5f : -1.5f), 0.05f, 0);
        Vector3 pos = transform.position + v1;
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
        Debug.DrawLine(pos, pos + new Vector3(0, -0.3f, 0), Color.green, 0.02f, false);

        if (!hit.collider) {
            return true;
        } else {
            RaycastHit2D hit1 = Physics2D.Raycast(pos, v1, 1f, LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall"));
            Debug.DrawLine(pos, pos + v1, Color.yellow, 0.02f, false);
            if (hit1.collider) {
                return true;
            }
        }

        return false;
    }

    void ChangeDirection()
    {
        //Debug.Log("Hedgehog: flip");
        faceRight = !faceRight;
        flip();
    }

    bool IsPlayerInRange()
    {
        Transform pt = Utils.GetPlayerTransform();
        if (pt == null)
            return false;

        float distP = Vector2.Distance(pt.position, transform.position);
        return distP < _aggravateRadius;
    }

    bool IsVisibleToPlayer()
    {
        Transform pt = Utils.GetPlayerTransform();
        if (pt == null)
            return false;
        float distP = Vector2.Distance(pt.position, transform.position);
        return distP < _visibilityRadius;
    }


    void onReturnedToIdle()
    {
        state = States.Crawling;
    }


    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        if (isDead) return;

        if (damageType == Types.DamageType.Spikes) {
            _hp = -1;
        }

        base.hurt(force, damageType);

        state = States.GettingHit;
    }
   protected override void die()
   {
        sounds.pitch = 1;
        sounds.volume = 0.8f;
        base.die();
        Destroy(this.gameObject, 1f);
   }
}
