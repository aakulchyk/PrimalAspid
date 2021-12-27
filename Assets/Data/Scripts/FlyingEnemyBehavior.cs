using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBehavior : NpcBehavior
{
    public float _followRadius = 14f;
    public float moveSpeed = 3f;

    private bool _chasingPlayer = false;
 
    private bool faceRight = false;

    public AudioClip clip_captured;

    private bool prev_captured = false;

    private float moveX;
    
    void Start()
    {
        BaseInit();
    }

    protected override bool checkAccessibility(Transform wp)
    {
        
        CircleCollider2D col = GetComponent<CircleCollider2D> ();
        Vector2 pos = new Vector2(transform.position.x + col.offset.x, transform.position.y + col.offset.y); 

        float r = col.radius*1.6f;

        Vector3[] points = new Vector3[] {
            new Vector3(pos.x - r, pos.y, 0),
            new Vector3(pos.x + r, pos.y, 0),
            new Vector3(pos.x, pos.y - r, 0),
            new Vector3(pos.x, pos.y + r, 0)
        };

        bool accessible = true;
        Vector3 ppos = new Vector3(wp.position.x, wp.position.y+1.5f, wp.position.z);
        foreach (var p in points) {
            float distM = Vector2.Distance(p, ppos);
            Vector3 dir = ppos - p;
            RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);

            if (!hit.collider) {
                Debug.DrawLine(p, ppos, Color.green, 0.02f, false);
                continue;
            }

            if (hit.collider.tag == "Player" || hit.collider == col) {
                Debug.DrawLine(p, ppos, Color.green, 0.02f, false);
                continue;
                
            } else {
                Debug.DrawLine(p, ppos, Color.red, 0.02f, false);
                accessible = false;
            }
        }
        
        
        return accessible;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }


        body.velocity = _chasingPlayer ? new Vector2( moveX, GetVectorToPlayer().y*moveSpeed ) : new Vector2(0, 0.86f);
    }
    
    void Update()
    {
        if (isDead || Time.timeScale == 0) {
            return;
        }

        Transform pt = PlayerTransform();

        //
        float distP = Vector2.Distance(pt.position, transform.position);
        _chasingPlayer = _followRadius > distP  &&  checkAccessibility(pt);

        moveX = (isDead || !_chasingPlayer) ? 0f : GetVectorToPlayer().x * moveSpeed;

        if (_knockback > 0) {
            Debug.Log("Fly frame of knockback");
            moveX = body.velocity.x;
            --_knockback;
        } else {
            anim.SetBool("chase", true);
        }

        // interact with player

        if (_chasingPlayer) {
            if (pt.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (pt.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }

            if (!grabbable) {
                Debug.Log("Capture Error: No Grabbable found");
                return;
            }

            if (!grabbable.captured) {
                if (prev_captured) {
                    sounds.Stop();
                    sounds.Play();
                }

            } else {

                if (!prev_captured) {
                    sounds.Stop();   
                }

                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_captured);
            }

            prev_captured = grabbable.captured;

        } else {
            anim.SetBool("chase", false);
        }
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        base.hurt(force, damageType);
        GetComponent<ParticleSystem>().Play();
    }
   protected override void die()
   {
       //anim.SetTrigger("die");
       base.die();
       Destroy(this.gameObject, 0.6f);
   }    
   
}
