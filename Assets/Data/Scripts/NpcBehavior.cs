using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    protected Game game;

    public bool isDead = false;

    public int MAX_KNOCKBACK = 12;
   
    protected Animator anim;
    protected Rigidbody2D body;
    protected PlayerControl player;
    protected Transform playerTransform;

    protected GrabbableBehavior grabbable = null;
    protected InteractableBehavior interactable = null;


    protected AudioSource sounds;

    public GameObject collectiblePrefab;  // the prefab of our collectible

    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public int _hp = 1;

    public bool invulnerable = false;

    protected int _knockback = 0;

        
    protected void BaseInit()
    {
        game = (Game)FindObjectOfType(typeof(Game));
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D> ();
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;

        sounds =  GetComponent<AudioSource>();

        Transform t = transform.Find("Grabbable");
        if (t) {
            grabbable = t.gameObject.GetComponent<GrabbableBehavior>();
        }

        t = transform.Find("Interactable");
        if (t) {
            interactable = t.gameObject.GetComponent<InteractableBehavior>();
        }
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

    public virtual void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {

        if (isDead) return;

        if (player.IsPulling() && player.GetComponent<FixedJoint2D>().connectedBody == GetComponent<Rigidbody2D>()) {
            player.releaseBody();
            player.throwByImpulse(new Vector2 (GetVectorToPlayer().x, GetVectorToPlayer().y*20), true);
            invulnerable = true;
        }

        Debug.Log("NPC Hurt");
        knockback(force);
        if (--_hp < 0 && !isDead) {
            die();
        } else {
            anim.SetBool("hurt", true);
            sounds.PlayOneShot(clip_hurt);
        }
    }

    public void knockback(Vector2 force) {
        _knockback = MAX_KNOCKBACK;
        body.velocity = force;
    }

    protected virtual void die() {
        Debug.Log("NPC die");
        sounds.Stop();
        sounds.PlayOneShot(clip_death);
        isDead = true;
        anim.SetTrigger("die");

        if (collectiblePrefab) {
            StartCoroutine(dropCollectible());
        }

        if (grabbable)
            grabbable.gameObject.SetActive(false);

        if (interactable)
            interactable.gameObject.SetActive(false);
    }


    IEnumerator dropCollectible() {
        yield return new WaitForSeconds(0.4F);
        GameObject go = Instantiate(collectiblePrefab);

        go.transform.position = gameObject.transform.position + new Vector3(0,1,0);
        go.transform.rotation = Quaternion.identity;
    }

        // Update is called once per frame
    protected Vector3 GetVectorToPlayer()
    {
        // get direction to player
        Vector3 direction = playerTransform.position - transform.position;
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
    {}

}
