using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrabBehavior : MonoBehaviour
{
    public AudioClip clip_death;
    public AudioClip clip_crawl;
    public AudioClip clip_jump;

    private PlayerControl player;
    private Transform playerTransform;
    public  float _followRadius = 7f;
    public float _moveSpeed = 1.5f;
    public float _jumpForce = 600f;

    private float distToGround;
    private Animator anim;
    private Rigidbody2D body;

    private bool isGrounded = true;
    private bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;

        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D> ();  

        distToGround = GetComponent<CircleCollider2D>().bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

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
        
            // jump
                        // Cast a ray straight down.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up);
            // If it hits something...
            if (hit.collider != null)
            {
                Debug.Log("raycast true " + hit.collider.tag);
                if (hit.collider.tag == "Player") {
                    anim.SetBool("jump", true);
                    body.AddForce(new Vector2(0f, _jumpForce));
                    isGrounded = false;
                    GetComponent<AudioSource>().Stop();
                    GetComponent<AudioSource>().PlayOneShot(clip_jump);
                }
            }
        }
    }



   void OnCollisionEnter2D(Collision2D hit)
    {
        if (hit.gameObject.CompareTag ("Obstacle")) {
            isGrounded = true;
            anim.SetBool("jump", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
	    if (other.tag == "Spike" && !isDead)
        {
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().PlayOneShot(clip_death);
            anim.SetTrigger("die");
	        isDead = true;
            body.constraints = 0;
            Destroy(this.gameObject, 5f);
        }
    }
}
