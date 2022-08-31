using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBehavior : NpcBehavior
{
    public float _followRadius;
    public float moveSpeed;

    private bool _chasingPlayer = false;
 
    private bool faceRight = false;

    public AudioClip clip_captured;

    private bool prev_captured = false;

    private Vector2 _moveVector;
    
    public int MaxChaseTimeout = 125;
    private int _chaseTimeout;

    public Transform[] waypoints;

    public Transform currentTarget;
    
    void Start()
    {
        BaseInit();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Hanger"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Background"), true);

        _chaseTimeout = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        if (_chaseTimeout>0)
            --_chaseTimeout;


        if (_knockback > 0) {
            _moveVector.x = body.velocity.x;
            --_knockback;
        } else {
            anim.SetBool("chase", true);
        }

        // do not fall
        body.velocity = _moveVector + new Vector2( 0, 0.86f );
    }
    
    void Update()
    {
        if (Time.timeScale == 0) {
            return;
        }
        
        if (isDead) {
            _moveVector = Vector2.zero;
            return;
        }

        Transform pt = PlayerTransform();
        if (pt == null)
            return;

        float distP = Vector2.Distance(pt.position, transform.position);
        bool noticed = _followRadius>distP  &&  checkAccessibility(pt);

        if (noticed) {
            _chaseTimeout = MaxChaseTimeout;
            _chasingPlayer = true;
        }

        if (_chaseTimeout > 0) {
            _chasingPlayer = true;
            _moveVector = TryToReachTarget(pt);
        } else {
            _chasingPlayer = false;
            if (waypoints.Length>0 && Vector2.Distance(waypoints[0].position, transform.position) > 0.2f)
                _moveVector = TryToReachTarget(waypoints[0], true);
            else
                _moveVector =  Vector2.zero;
        }

        EnsureRightDirection(pt);
        
        // interact with player
        if (_chasingPlayer) {
            if (!grabbable) {
                Debug.Log("Capture Error: No Grabbable found");
                return;
            }

            CheckIfCaptured();   

        } else {
            anim.SetBool("chase", false);
        }
    }


    void EnsureRightDirection(Transform pt)
    {
        if (pt.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (pt.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }
    }

    void CheckIfCaptured()
    {
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
    }

    Vector2 TryToReachTarget(Transform pt, bool waypoint = false)
    {
        Vector2 mv;
        if (checkAccessibility(pt)) {
            Debug.DrawLine(transform.position, pt.position, Color.cyan, 0.5f, false);    
            currentTarget = pt;
            Vector3 direction = pt.position - transform.position;
            mv = new Vector2 (direction.normalized.x * moveSpeed, direction.normalized.y * moveSpeed);
        } else {
            if (waypoints == null || waypoints.Length==0)
                return Vector2.zero;

            // find a waypoint closest to the player
            float minDist =  Vector2.Distance(pt.position, waypoints[0].position);
            Transform minDistWp =  waypoints[0];
            foreach (var wp in waypoints) {
                if (!checkAccessibility(wp))
                    continue;

                /*if (!IsWaypointAccessibleToPlayer(wp, pt))
                    continue;*/

                float distPl = Vector2.Distance(pt.position, wp.position);
                if (distPl < minDist) {
                    minDist = distPl;
                    minDistWp = wp;
                }
            }

            currentTarget = minDistWp;

            Vector3 direction = minDistWp.position - transform.position;
            Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0.1f, false);    
            mv = new Vector2 (direction.normalized.x * moveSpeed, direction.normalized.y * moveSpeed);
        }

        return mv;
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

            //col.enabled = false;
            var layers = LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall") | LayerMask.GetMask("DeadlyDanger") | LayerMask.GetMask("PC");
            RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM, layers);
            //col.enabled = true;
            if (!hit.collider) {
                Debug.DrawLine(p, ppos, Color.yellow, 0.02f, false);
                continue;
            }

            if (hit.collider.tag == "Player" || hit.collider.tag == "Enemy") {
                Debug.DrawLine(p, ppos, Color.green, 0.02f, false);
                continue;
            } else {
                Debug.DrawLine(p, ppos, Color.red, 0.02f, false);
                accessible = false;
            }
        }
        
        return accessible;
    }

    private bool IsWaypointAccessibleToPlayer(Transform wpt, Transform pt)
    {
        Vector3 ppos = new Vector3(pt.position.x, pt.position.y+1.5f, pt.position.z);
        var layers = LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall") | LayerMask.GetMask("DeadlyDanger") | LayerMask.GetMask("PC");

        float distM = Vector2.Distance(wpt.position, pt.position);
        Vector3 dir = pt.position - wpt.position;

        RaycastHit2D hit = Physics2D.Raycast(wpt.position, dir.normalized, distM, layers);

        if (!hit.collider) {
            //Debug.DrawLine(p, ppos, Color.yellow, 0.02f, false);
            return true;
        }

        if (hit.collider.tag == "Player" || hit.collider.tag == "Enemy") {
                //Debug.DrawLine(p, ppos, Color.green, 0.02f, false);
                return true;
            } else {
                //Debug.DrawLine(p, ppos, Color.red, 0.02f, false);
                return false;
            }



    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {
        base.hurt(force, damageType, damage);
    }


   protected override void die()
   {
       base.die();
       Destroy(this.gameObject, 0.6f);
   }    
   
}
