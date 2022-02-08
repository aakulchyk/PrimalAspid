using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows object to break after depleting its "health".


public class Breakable : MonoBehaviour
{
//    [SerializeField] private Animator animator;
    [SerializeField] private Sprite brokenSprite; //If destroyAfterDeath is false, a broken sprite will appear instead
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private GameObject pointLight;

    [SerializeField] private Animator animator;
    [SerializeField] private bool destroyAfterDeath = true; //If false, a broken sprite will appear instead of complete destruction
    public int health;
    public bool destroyed = false;
  //  [SerializeField] private Instantiator instantiator;
    [SerializeField] private AudioClip clip_hit;
    [SerializeField] private AudioClip clip_destroy;
    private bool recovered;

    [SerializeField] private Rigidbody2D rigidBody;
    //[SerializeField] private RecoveryCounter recoveryCounter;
    //[SerializeField] private bool requireDownAttack;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D collider;

    [SerializeField] private GameObject attachedObject;



    // Use this for initialization
    void Start()
    {
        //recoveryCounter = GetComponent<RecoveryCounter>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();

        if (destroyed) {
            collider.enabled = false;
            spriteRenderer.enabled = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        if (collider.tag == "Boulder") {
            if (collision.relativeVelocity.magnitude > 6) {
                Die();
            }
        }
    }

    public void hurt(int hitPower)
    {
        //If breakable object health is above zero, it's not recovering from a recent hit, get hit!
        if (health > 0)
        {
            /*if (NewPlayer.Instance.pounding)
            {
                NewPlayer.Instance.PoundEffect();
            }*/

            
            //Ensure the player can't hit the box multiple times in one hit
            //recoveryCounter.counter = 0;

            //StartCoroutine(NewPlayer.Instance.FreezeFrameEffect());

            health--;
            

            if (health <= 0) {
                Die();
            } else {
                GetComponent<AudioSource>().PlayOneShot(clip_hit);
                if (animator)
                    animator.SetTrigger("hurt");
            }
        }
    }



    public void Die()
    {
        //Ensure timeScale is forced to 1 after breaking
        Time.timeScale = 1;

        GetComponent<BoxCollider2D>().enabled = false;

        if (rigidBody)
            rigidBody.simulated = false;

        if (attachedObject) {
            var joint = attachedObject.GetComponent<HingeJoint2D>();
            if (joint) {
                //joint.connectedBody = null;
                joint.breakForce = 0;
            }
        }

        spriteRenderer.enabled = false;

        if (animator)
            animator.SetTrigger("die");

        //Activate deathParticles & unparent from this so they aren't destroyed!
        deathParticles.SetActive(true);
        deathParticles.transform.parent = null;

        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_destroy);
        
        if (pointLight)
            pointLight.SetActive(false);

        /*if (instantiator != null)
        {
            instantiator.InstantiateObjects();
        }*/

        //Destroy me, or set my sprite to the brokenSprite
        if (destroyAfterDeath)
        {
            Destroy(gameObject, 1f);
        }
        else
        {
            spriteRenderer.sprite = brokenSprite;
        }
    }
}
