using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidEnemyBehavior : MonoBehaviour
{
    public AudioClip clip_death;

    private PlayerControl player;
    private Transform playerTransform;
    private Animator anim;
    private float _followRadius = 14f;
    private float moveSpeed = 2f;


    public bool captured = false;

   
    private bool faceRight = false;
    void Start()
    {
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
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

    void OnTriggerEnter2D(Collider2D other) {
	    if (other.tag == "Spike") {
            GetComponent<AudioSource>().PlayOneShot(clip_death);
            Debug.Log("Enemy Die");
            anim.SetBool("hurt", true);
	        
            Destroy(this.gameObject, 0.5f);
            
            //GetComponent<AudioSource>().enabled = false;
            //isDead = true;
            //this.transform.tag = "Untagged";
            //GetComponent<CapsuleCollider2D>().isTrigger = false;
        }
    }

    public void die() {
        Debug.Log("Enemy is DEAD");
        anim.SetTrigger("death");
        GetComponent<AudioSource>().enabled = false;
        
    }

    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }
}
