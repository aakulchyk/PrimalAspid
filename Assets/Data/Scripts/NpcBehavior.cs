using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    protected Game game;

    public bool isDead = false;

    public int MAX_KNOCKBACK;
   
    [NonSerialized] protected Animator anim;
    [NonSerialized] protected Rigidbody2D body;
    
    [NonSerialized] protected GrabbableBehavior grabbable = null;
    [NonSerialized] protected InteractableBehavior interactable = null;

    [SerializeField] protected Droppable droppable;

    protected AudioSource sounds;

    public AudioClip clip_hurt;
    public AudioClip clip_death;
    
    protected int _hp;

    public bool invulnerable = false;

    protected int _knockback = 0;

    [SerializeField] protected int INITIAL_HP;
    [SerializeField] protected ParticleSystem damageParticles;
    [SerializeField] protected GameObject deathParticles;


        
    protected void BaseInit()
    {
        game = (Game)FindObjectOfType(typeof(Game));
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D> ();

        sounds =  GetComponent<AudioSource>();

        Transform t = transform.Find("Grabbable");
        if (t) {
            grabbable = t.gameObject.GetComponent<GrabbableBehavior>();
        }

        t = transform.Find("Interactable");
        if (t) {
            interactable = t.gameObject.GetComponent<InteractableBehavior>();
        }

        _hp = INITIAL_HP;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        processCollision(collision);
    }

    protected virtual void processCollision(Collision2D collision) {
       // Debug.Log("NPC collision");
        Collider2D collider = collision.collider;
        if (collider.tag == "Boulder") {
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike")
        {
            hurt(Vector2.zero);
        }
    }


    public virtual void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {

        if (isDead) return;

        if (Utils.GetPlayer().IsPulling() && Utils.GetPlayer().GetComponentInParent<FixedJoint2D>().connectedBody == GetComponent<Rigidbody2D>()) {
            Utils.GetPlayer().releaseBody();
            Utils.GetPlayer().throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*20), true);
            invulnerable = true;
        }

        sounds.PlayOneShot(clip_hurt);
        anim.SetTrigger("hurt");


        _hp -= damage;
        //Debug.Log("NPC Hurt");
        if (_hp < 0 && !isDead) {
            die();
        } else {
            knockback(force);
        }

        if (damageParticles) {
            damageParticles.Emit(10);
        }
    }

    public void knockback(Vector2 force) {
        _knockback = MAX_KNOCKBACK;
        body.velocity = force;
    }

    protected virtual void die() {
        Debug.Log("NPC die");
        StartCoroutine(DieNow());
    }

    IEnumerator DieNow() {
        yield return new WaitForSeconds(0.07F);
        sounds.PlayOneShot(clip_death);
        isDead = true;
        
        //body.isKinematic = true;
        //body.velocity = Vector2.zero;
        anim.SetTrigger("die");

        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }

        StartCoroutine(Utils.FreezeFrameEffect(.01f));

        GetComponent<Collider2D>().enabled = false;

        if (grabbable)
            grabbable.gameObject.SetActive(false);

        if (interactable)
            interactable.gameObject.SetActive(false);


        if (droppable)
            droppable.Drop();
        
    }

    public virtual bool CheckGrounded()
    {
        Vector3 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + Vector3.up, Vector3.down, 1.2f, LayerMask.GetMask("Ground"));
        Debug.DrawLine(pos+Vector3.up, pos+Vector3.down*1.2f, Color.black, 0.02f, false);

        return hit.collider != null;
    }

    protected Transform PlayerTransform()
    {
        return Utils.GetPlayerTransform();
    }

        // Update is called once per frame
    protected Vector3 GetVectorToPlayer()
    {
        // get direction to player
        Vector3 direction = Utils.GetPlayerTransform().position - transform.position;
        return direction.normalized;
    }

    protected virtual bool checkAccessibility(Transform wp)
    {
        Vector3 p = new Vector3(transform.position.x, transform.position.y, 0);

        float distM = Vector2.Distance(p, wp.position);
        Vector3 dir = wp.position - p;
        RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);
        if (hit.collider != null && hit.collider.tag != "Player") {
            Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
            return false;
        } else {
            Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
            return true;
        }
    }

    public virtual void getCaptured()
    {
        if (grabbable)
            grabbable.getCaptured();
        else
            Debug.Log("Capture Error: No Grabbable found");
    }

    public virtual void getReleased()
    {
        if (grabbable)
            grabbable.getReleased();
        else
            Debug.Log("Capture Error: No Grabbable found");
    }

    public virtual void LoadInActualState()
    {
        Debug.Log("Dummy LoadInActualState");
    }

    public virtual void onTalk()
    {
    }

    public void flip()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);

        if (grabbable) {
            grabbable.FlipCanvas();
        }

        if (interactable) {
            interactable.FlipCanvas();
        }
    }

}
