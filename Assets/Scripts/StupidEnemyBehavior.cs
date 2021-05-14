using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidEnemyBehavior : NpcBehavior
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

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDead) return;
         // interact with player
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius)
        {
            anim.SetBool("hurt", false);
            if (playerTransform.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (playerTransform.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }

            if (!captured) {

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

            prev_captured = captured;


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
    }
}
