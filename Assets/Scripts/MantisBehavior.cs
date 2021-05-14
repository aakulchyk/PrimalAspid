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

    public AudioClip clip_boss_music;

    public Transform[] waypoints;

    public GameObject controlledWall;

    private bool noticed = false;


    void Start()
    {
        BaseInit();
    }

    IEnumerator DeleteProjectileAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        currentProjectile = null;
    }

    void FixedUpdate()
    {
        if (isDead) return;
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


        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius)
        {
            if (!noticed) {
                GetComponent<AudioSource>().PlayOneShot(clip_boss_music);
                noticed = true;
            }

            if (playerTransform.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (playerTransform.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }


            if (distP < _shootRadius) {
                if (currentProjectile == null && !shootAnticipate && !invulnerable && !captured) {

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y));
                    //Debug.DrawLine(transform.position, playerTransform.position, Color.blue, 0.2f, false);

                    if (hit.collider != null && hit.collider.tag == "Player") {
                        // start shoot animation. once it's played, the projectile will be actually fired
                        shootAnticipate = true;
                        shootTimeout = maxShotTimeout;
                        //anim.SetTrigger("shoot");
                        anim.SetBool("shooting", true);
                    }
                }
            }

            if (!captured) {

                RaycastHit2D hit = Physics2D.Raycast(transform.position, GetVectorToPlayer());
                Vector2 moveVector;

                if (checkAccessibility(playerTransform)/*hit.collider != null && hit.collider.tag == "Player"*/) {
                    Debug.DrawLine(transform.position, playerTransform.position, Color.green, 0.1f, false);    
                    moveVector = new Vector2 (GetVectorToPlayer().x * moveSpeed, GetVectorToPlayer().y * moveSpeed);
                } else {
                    // find a waypoint closest to the player

                    float minDist = 99999f;
                    Transform minDistWp = transform;
                    foreach (var wp in waypoints) {
                        if (!checkAccessibility(wp))
                            continue;

                        float distPl = Vector2.Distance(playerTransform.position, wp.position);
                        if (distPl < minDist) {
                            minDist = distPl;
                            minDistWp = wp;
                        }
                    }

                    //Debug.Log("Move to waypoint " + minDistWp.gameObject.name);
                    Vector3 direction = minDistWp.position - transform.position;
                    Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0.1f, false);    
                    moveVector = new Vector2 (direction.normalized.x * moveSpeed, direction.normalized.y * moveSpeed);
                }

                
                GetComponent<Rigidbody2D>().velocity = moveVector;
            } else {
                /*anim.SetBool("hurt", true);
                shootAnticipate = false;*/

                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_captured);
            }


        } else {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.196f);
        }
    }


    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void shoot() {
        if (!shootAnticipate) return;
        
        Debug.Log("Mantis: Shoot");

        GetComponent<AudioSource>().PlayOneShot(clip_shoot);

        GameObject go = Instantiate(projectilePrefab);

        go.transform.position = gameObject.transform.position + GetVectorToPlayer() * 2.2f;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = new Vector3(5, 5, 1);
        
        currentProjectile = go.GetComponent<MantisProjectile>();
        currentProjectile.setImpulse(GetVectorToPlayer());
        shootAnticipate = false;
        anim.SetBool("shooting", false);
    }

    private bool checkAccessibility(Transform wp)
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
            if (hit.collider != null && hit.collider.tag != "Player") {
                //Debug.Log(wp.gameObject.name + " not visible because of " + hit.collider.tag + "/" + hit.collider.gameObject.name);
                Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
                accessible = false;
                continue;
            } else {
                Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
            }
        }
        
        
        return accessible;
        
    }

    public override void getCaptured()
    {
        anim.SetBool("shooting", false);
        anim.SetBool("hurt", true);
        captured = true;
        shootAnticipate = false;
    }

    public override void getReleased()
    {
        Debug.Log("GetReleased");
        anim.SetBool("hurt", false);
        anim.SetBool("shooting", false);
        captured = false;
    }
    protected override void processCollision(Collision2D collision) {
        if (invulnerable) return;

        //Debug.Log("Mantis collision");
        Collider2D collider = collision.collider;
        if (collider.tag == "Throwable") {
            if (collision.relativeVelocity.magnitude > 7) {
                hurt(0);
            }
        }

        if (collider.tag == "Spike")
        {
            if (currentProjectile && currentProjectile.gameObject == collider.gameObject) {
                return;
            }
            
            hurt(0);
        }
    }

    public override void hurt(float force) {

        if (isDead) return;

        Debug.Log("Mantis Hurt");

        if (player.isPulling && player.GetComponent<FixedJoint2D>().connectedBody == GetComponent<Rigidbody2D>()) {
            player.releaseBody();
            player.throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*12));
        }

        if (--_hp < 0) {
            die();
        } else {
            sounds.PlayOneShot(clip_hurt);
            invulnerable = true;
            StartCoroutine(blinkInvulnerable());
        }
    }

    protected override void die() {
        anim.SetTrigger("die");

        Destroy(this.gameObject, 1.5f);


        sounds.Stop();
        sounds.pitch = 0.8f;
        sounds.volume = 1f;
        sounds.PlayOneShot(clip_death);
        isDead = true;
        GetComponent<Renderer>().enabled = false;

        StartCoroutine(OpenDoorAfterDelay(1));

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
            //rb.velocity = new Vector2 (vector.x * speed, vector.y * speed);
        }

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

        // every door opening is a checkpoint
        Game game = (Game)FindObjectOfType(typeof(Game));
        game.SaveGame();
    }

}
