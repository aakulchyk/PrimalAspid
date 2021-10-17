using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBehavior : NpcBehavior
{

    public float _followRadius = 14f;
    public float moveSpeed = 3f;
 
    private bool faceRight = false;

    public AudioClip clip_captured;

    private bool prev_captured = false;
    
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
        foreach (var p in points) {
            float distM = Vector2.Distance(p, wp.position);
            Vector3 dir = wp.position - p;
            RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);

            if (!hit.collider) {
                Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
                continue;
            }

            if (hit.collider.tag == "Player" || hit.collider == col) {
                Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
                continue;
                
            } else {
                Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
                accessible = false;
            }
        }
        
        
        return accessible;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDead || Time.timeScale == 0) return;

        // interact with player
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        //bool visible = ;

        if (_followRadius > distP  &&  checkAccessibility(playerTransform))
        {
            anim.SetBool("hurt", false);
            if (playerTransform.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (playerTransform.position.x < transform.position.x && faceRight) {
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

                GetComponent<Rigidbody2D>().velocity = new Vector2 (GetVectorToPlayer().x * moveSpeed, GetVectorToPlayer().y * moveSpeed);
                anim.SetBool("chase", true);
            } else {
                anim.SetBool("hurt", true);
                
                if (!prev_captured)
                    sounds.Stop();    
                

                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_captured);
            }

            prev_captured = grabbable.captured;


        } else {
            anim.SetBool("chase", false);
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.25f);
        }
    }

   protected override void die()
   {
        base.die();
        Destroy(this.gameObject, 0.5f);
   }    

    public void flip()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);

        
        if (grabbable) {
            grabbable.FlipCanvas();
        }
    }
    
}
