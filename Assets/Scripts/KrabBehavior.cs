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

    // Update is called once per frame
    void Update()
    {
        if (isDead || Time.timeScale == 0) return;

        if (!isGrounded) return;
        
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius)
        {
            // move left/right
            Vector3 direction = playerTransform.position - transform.position;
            direction.x = direction.x > 0 ? 1f : -1f;
            body.velocity = new Vector2 (direction.x * _moveSpeed, body.velocity.y);

            if (direction.x !=0 && ! GetComponent<AudioSource>().isPlaying) {
                GetComponent<AudioSource>().PlayOneShot(clip_crawl);
            }

            return;
        
            // jump
            // Cast a ray straight up.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up);
            // If it hits something...
            if (hit.collider != null)
            {
                //Debug.Log("raycast true " + hit.collider.tag);
                if (hit.collider.tag == "Player") {

                    if (Time.time - lastJumpTime > 0.5f) {
                        jump();
                        lastJumpTime = Time.time;
                    }
                    
                }
            }
        }
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

   protected override void die()
   {
        base.die();
        Destroy(this.gameObject, 5f);
   }
}
