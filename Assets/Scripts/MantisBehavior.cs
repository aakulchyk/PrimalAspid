using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisBehavior : NpcBehavior
{
    private float _followRadius = 14f;
    private float _shootRadius = 8f;
    private float moveSpeed = 1f;

    public bool captured = false;   
    private bool faceRight = false;

    public GameObject projectilePrefab;     // the prefab of our bullet
    //GameObject currentProjectileObject = null;
    MantisProjectile currentProjectile;
    private int shootTimeout = 0;
    private bool shootAnticipate = false;
    // Start is called before the first frame update

    public AudioClip clip_shoot;


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
                Debug.Log("Destroy this fucking bullet!");
                currentProjectile.Delete();
                DeleteProjectileAfterDelay(1f);
            }
        }


        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius)
        {
            //anim.SetBool("hurt", false);
            if (playerTransform.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (playerTransform.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }


            if (distP < _shootRadius) {
                if (currentProjectile == null && !shootAnticipate) {
                    shootAnticipate = true;
                    shootTimeout = 100;
                    // start shoot animation. once it's played, the projectile will be actually fired
                    anim.SetTrigger("shoot");
                }
            }

            if (!captured) {
                GetComponent<Rigidbody2D>().velocity = new Vector2 (GetVectorToPlayer().x * moveSpeed, GetVectorToPlayer().y * moveSpeed);
            } else {
                //anim.SetBool("hurt", true);
                captured = false;
            }


        } else {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.196f);
        }
    }


    protected override void die()
    {
        base.die();
        Destroy(this.gameObject, 0.5f);
    }    

    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void shoot() {
        Debug.Log("Mantis: Shoot");

        GetComponent<AudioSource>().PlayOneShot(clip_shoot);

        GameObject go = Instantiate(projectilePrefab);

        go.transform.position = gameObject.transform.position + GetVectorToPlayer() * 3.3f;
        go.transform.rotation = gameObject.transform.rotation;
        go.transform.localScale = new Vector3(5, 5, 1);
        
        currentProjectile = go.GetComponent<MantisProjectile>();
        currentProjectile.setImpulse(GetVectorToPlayer());
        shootAnticipate = false;
    }

    // Update is called once per frame
    Vector3 GetVectorToPlayer()
    {
        // get direction to player
        Vector3 direction = playerTransform.position - transform.position;
        return direction.normalized;
    }

    void OnDestroy()
    {
        if (currentProjectile) {
            currentProjectile.Delete();
        } 
    }
}
