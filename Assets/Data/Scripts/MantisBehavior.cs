using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisBehavior : NpcBehavior
{
    public float _followRadius = 20f;
    public float _shootRadius = 10f;
    private float moveSpeed = 3f;

    public int maxShotTimeout = 120;

    private bool faceRight = false;

    public GameObject projectilePrefab;     // the prefab of our bullet

    public GameObject[] deathChunkPrefabs;
    //GameObject currentProjectileObject = null;
    MantisProjectile currentProjectile;
    private int shootTimeout = 0;
    private bool shootAnticipate = false;
    // Start is called before the first frame update

    public AudioClip clip_shoot;
    public AudioClip clip_captured;

    public Transform[] waypoints;

    public GameObject controlledWall;

    public bool idle = true;


    private bool prev_captured = false;


    void Start()
    {
        BaseInit();

        if (grabbable) {
            grabbable.SetGetCapturedCallback(getCaptured);
            grabbable.SetGetReleasedCallback(getReleased);
        }
    }

    IEnumerator DeleteProjectileAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        currentProjectile = null;
    }

    void FixedUpdate()
    {
        if (isDead || Time.timeScale == 0) return;
         // interact with player

        if (shootTimeout > 0) {
            shootTimeout--;
        } else {
            if (currentProjectile) {
                //Debug.Log("Destroy this fucking bullet!");
                currentProjectile.Delete();
                DeleteProjectileAfterDelay(1f);
            }
        }

        Transform pt = Utils.GetPlayerTransform();
        if (pt == null)
            return;

        float distP = Vector2.Distance(pt.position, transform.position);
        if (distP < _followRadius && !idle)
        {
            if (pt.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (pt.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }


            if (distP < _shootRadius) {
                if (currentProjectile == null && !shootAnticipate && !invulnerable && !grabbable.captured) {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y));

                    if (hit.collider != null && hit.collider.tag == "Player") {
                        shootAnticipate = true;
                        shootTimeout = maxShotTimeout;
                        anim.SetBool("shooting", true);
                    }
                }
            }

            if (!grabbable) {
                Debug.Log("Capture Error: No Grabbable found");
                return;
            }

            if (!grabbable.captured) {
                Vector2 moveVector;

                if (checkAccessibility(pt)) {
                    Debug.DrawLine(transform.position, pt.position, Color.green, 0.1f, false);    
                    moveVector = new Vector2 (GetVectorToPlayer().x * moveSpeed, GetVectorToPlayer().y * moveSpeed);
                } else {
                    // find a waypoint closest to the player
                    float minDist = 99999f;
                    Transform minDistWp = transform;
                    foreach (var wp in waypoints) {
                        if (!checkAccessibility(wp))
                            continue;

                        float distPl = Vector2.Distance(pt.position, wp.position);
                        if (distPl < minDist) {
                            minDist = distPl;
                            minDistWp = wp;
                        }
                    }

                    Vector3 direction = minDistWp.position - transform.position;
                    Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0.1f, false);    
                    moveVector = new Vector2 (direction.normalized.x * moveSpeed, direction.normalized.y * moveSpeed);
                }
                
                GetComponent<Rigidbody2D>().velocity = moveVector;
            } else {
                if (!prev_captured)
                    sounds.Stop(); 

                if (!sounds.isPlaying) {
                    Debug.Log("Play capture sound");
                    sounds.PlayOneShot(clip_captured);
                }
            }

            prev_captured = grabbable.captured;
        } else {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.196f);
        }
    }

    private void move() {

    }

    public void shoot() {
        if (!shootAnticipate) return;
        
        Debug.Log("Mantis: Shoot");

        GetComponent<AudioSource>().PlayOneShot(clip_shoot);

        GameObject go = Instantiate(projectilePrefab);

        go.transform.position = gameObject.transform.position + GetVectorToPlayer() * 2.0f;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = new Vector3(5, 5, 1);
        
        currentProjectile = go.GetComponent<MantisProjectile>();
        currentProjectile.setImpulse(GetVectorToPlayer());
        shootAnticipate = false;
        anim.SetBool("shooting", false);
    }

    public override void getCaptured()
    {
        anim.SetBool("shooting", false);
        anim.SetBool("captured", true);
        
        shootAnticipate = false;
    }

    public override void getReleased()
    {
        anim.SetBool("captured", false);
    }

    protected override bool checkAccessibility(Transform wp)
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D> ();
        Vector3 pos = transform.position;

        Vector3[] points = new Vector3[] {
            new Vector3(pos.x - col.size.x*4f, pos.y, 0),
            new Vector3(pos.x + col.size.x*4f, pos.y, 0),
            new Vector3(pos.x, pos.y - col.size.y*4.5f, 0),
            new Vector3(pos.x, pos.y + col.size.y*3f, 0)
        };

        bool accessible = true;
        foreach (var p in points) {
            float distM = Vector2.Distance(p, wp.position);
            Vector3 dir = wp.position - p;
            RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);
            if (hit.collider != null && hit.collider.tag != "Player" && hit.collider != col) {
                Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
                accessible = false;
                continue;
            } else {
                Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
            }
        }
        
        return accessible;    
    }

    protected override void processCollision(Collision2D collision) {
        if (invulnerable) return;

        Collider2D collider = collision.collider;
        if (collider.tag == "Boulder") {
            if (collision.relativeVelocity.magnitude > 7) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike")
        {
            if (currentProjectile && currentProjectile.gameObject == collider.gameObject) {
                return;
            }
            
            hurt(Vector2.zero);
        }
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {

        if (isDead) return;

        Debug.Log("Mantis Hurt");

        PlayerControl pc = Utils.GetPlayer();

        if (pc.IsPulling() && pc.GetComponentInParent<FixedJoint2D>().connectedBody == GetComponent<Rigidbody2D>()) {
            pc.releaseBody();
            pc.throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*30), true);
        }

        knockback(force);
        
        if (--_hp < 0) {
            die();
        } else {
            sounds.Stop();
            sounds.PlayOneShot(clip_hurt);
            invulnerable = true;
            StartCoroutine(blinkInvulnerable());
        }

        GetComponent<ParticleSystem>().Play();
    }

    protected override void die() {
        isDead = true;
        anim.SetTrigger("die");

        sounds.Stop();
        sounds.pitch = 0.8f;
        sounds.volume = 1f;
        sounds.PlayOneShot(clip_death);
        
        GetComponent<Renderer>().enabled = false;

        StartCoroutine(OpenDoorAfterDelay(2));

        //dismantle
        Vector3 v = new Vector3 (-1.5f, 0f, 0f);
        foreach (var prefab in deathChunkPrefabs) {
            GameObject go = Instantiate(prefab);
            go.transform.position = gameObject.transform.position;
            go.transform.position = gameObject.transform.position + v;
            v.x += 0.5f;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = new Vector3(9, 9, 1);

            Rigidbody2D rb = gameObject.GetComponentInChildren<Rigidbody2D>();
        }

        // stop boss fight music
        GameObject bossMusic = GameObject.Find("MantisBossFightMusic");
        bossMusic.GetComponent<AudioSource>().enabled = false;

        Destroy(this.gameObject, 3.5f);

        if (droppable)
            droppable.Drop();
    }

    void OnDestroy()
    {
        if (currentProjectile) {
            currentProjectile.Delete();  
        } 
    }

    IEnumerator blinkInvulnerable() {
        Renderer r = GetComponent<Renderer>();
        invulnerable = true;
        for (int i=0; i<4; i++)
        {
            r.enabled = false;
            yield return new WaitForSeconds(0.1F);
            r.enabled = true;
            yield return new WaitForSeconds(0.1F);
        }
        invulnerable = false;
    }

    IEnumerator OpenDoorAfterDelay(float time)
    {
        Debug.Log("Open Door!");
        if (controlledWall == null) yield break;
        yield return new WaitForSeconds(time);
        controlledWall.GetComponent<Animator>().SetTrigger("Opened");
        yield return new WaitForSeconds(0.5f);
        controlledWall.GetComponent<AudioSource>().enabled = true;
    }

}
