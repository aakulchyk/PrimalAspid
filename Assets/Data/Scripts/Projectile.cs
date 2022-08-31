using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip clip_bump;
    [SerializeField] private AudioClip clip_destroy;

    [SerializeField] protected GameObject deathParticles;
    [SerializeField] protected Explosion explosion;
    [SerializeField] protected ParticleSystem impactParticles;


    [SerializeField] protected Rigidbody2D body;

    public int speed;          // The speed our bullet travels

    // Start is called before the first frame update
    protected float ejectTime;

    public float MaxLiveTimeout;

    public bool strongImpact;

    public string targetColliderTag = "Enemy";


    public bool isOwnedByPlayer;

    protected bool isDead = false;

    void Start()
    {
        string layer = isOwnedByPlayer ? "PlayerProjectile" : "EnemyProjectile";
        string ownerLayer = isOwnedByPlayer ? "PC" : "Enemy";
        Debug.Log("Projectile: created");

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(layer), LayerMask.NameToLayer(ownerLayer), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(layer), LayerMask.NameToLayer("Background"), true);

        if (impactParticles)
            impactParticles.Stop();

        ejectTime = Time.time;
    }

    void Update()
    {
        if (Time.time - ejectTime > MaxLiveTimeout)
            Die();
    }

    public void setImpulse(Vector3 vector) {
        // add force 
        body.velocity = new Vector2 (vector.x, vector.y);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        if (collider.tag == "Ground" || collider.tag == "Platform" || collider.tag == "Obstacle" || collider.tag == "StickyWall" || collider.tag == "Breakable")
        {
            if (strongImpact) {
                Utils.GetPlayer().cameraEffects.Shake(0.2f, 500, 0.4f);
            }
            
            if (impactParticles)
                impactParticles.Emit(10);

            if (clip_bump)    
                GetComponent<AudioSource>().PlayOneShot(clip_bump);
            
        } else if (collider.tag == targetColliderTag) {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
            return;


        isDead = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().enabled = false;

        //GetComponent<AudioSource>().PlayOneShot(clip_destroy);

        if (explosion) {
            body.velocity = Vector2.zero;
            body.isKinematic = true;
            body.freezeRotation = true;
            explosion.Explode();

        }
        
        /*if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }*/
        
        Destroy(gameObject, 3f);
    }

    public void Delete() 
    {
        Destroy(gameObject);
        Debug.Log("Projectile: Deleted");
    }


    void OnDestroy()
    {
        Debug.Log("Projectile: Destroyed");
    }
}
