using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaggotRescuedBehavior : MonoBehaviour
{
    private float moveSpeed = 0.8f;
    private float _followRadius = 8f;

    private Transform playerTransform;
 

    private Animator anim;
    private Rigidbody2D body; 
    private AudioSource sounds;

    public bool isDead = false;
    public bool isFollowing = false;

    private PlayerControl player;

    public Transform targetSibling;

    public AudioClip clip_death;
    public AudioClip clip_sad_idle;
    public AudioClip clip_follow_start;
    public AudioClip clip_follow_stop;
    public AudioClip clip_success;



    // Start is called before the first frame update
    void Start()
    {
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        sounds = GetComponent<AudioSource>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;


        // interact with sibling
        float distS = Vector2.Distance(targetSibling.position, transform.position);
        if (distS < _followRadius) {
            if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_success);

            Vector3 direction = targetSibling.position - transform.position;     
            body.velocity = new Vector2 (direction.x * moveSpeed, body.velocity.y);
            return;
        }


        // interact with player
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius && player.isLeading)
        {
            if (isFollowing == false) {
                isFollowing = true;
                if (sounds.isPlaying)
                    sounds.Stop();

                sounds.PlayOneShot(clip_follow_start);
                anim.SetBool("isFollowing", true);
            }

            /*if (distP < 1.5) {
                body.velocity = new Vector2(0, 0);
                return;
            }*/
                
            //Vector3 direction = playerTransform.position - transform.position;
            //body.velocity = new Vector2 (direction.x * moveSpeed, direction.y * moveSpeed);
        }
        else {
            if (isFollowing) {
                isFollowing = false;
                anim.SetBool("isFollowing", false);
                sounds.PlayOneShot(clip_follow_stop);
            } else {
                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_sad_idle);
            }
            
            //body.velocity = new Vector2(0, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
	    if (other.tag == "Spike" || other.tag == "Enemy") {
            GetComponent<AudioSource>().PlayOneShot(clip_death);
            Debug.Log("Maggot Die");
	        anim.SetTrigger("Die");
            
            isDead = true;
            //this.transform.tag = "Untagged";
            //GetComponent<CapsuleCollider2D>().isTrigger = false;
        }
    }

    public void die() {
        Debug.Log("Maggot is DEAD");
        anim.SetBool("isDead", true);
        GetComponent<AudioSource>().enabled = false;
        player.LoseAndRespawn();
    }
}
