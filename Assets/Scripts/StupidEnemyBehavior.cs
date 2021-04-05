using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidEnemyBehavior : NpcBehavior
{

    public float _followRadius = 14f;
    public float moveSpeed = 3f;

    public bool captured = false;   
    private bool faceRight = false;
    
    void Start()
    {
        BaseInit();
    }

    // Update is called once per frame
    void Update()
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
                Vector3 direction = playerTransform.position - transform.position;
                direction.x = direction.x > 0 ? 1f : -1f;
                direction.y = direction.y > 0 ? 1f : -1f;
                GetComponent<Rigidbody2D>().velocity = new Vector2 (direction.x * moveSpeed, direction.y * moveSpeed);
            } else {
                anim.SetBool("hurt", true);
                captured = false;
            }


        } else {
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
