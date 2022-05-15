using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisProjectile : MonoBehaviour
{
    [SerializeField] private AudioClip clip_bump;
    [SerializeField] private AudioClip clip_destroy;

    [SerializeField] protected GameObject deathParticles;
    [SerializeField] protected ParticleSystem impactParticles;
    public int speed;          // The speed our bullet travels

    // Start is called before the first frame update
    public int MAX_HITS = 3;
    public int hits = 0;
    void Start()
    {
        Debug.Log("Projectile: created");
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyProjectile"), LayerMask.NameToLayer("Enemy"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyProjectile"), LayerMask.NameToLayer("Background"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyProjectile"), LayerMask.NameToLayer("PlayerProjectile"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyProjectile"), LayerMask.NameToLayer("EnemyProjectile"), true);

        impactParticles.Stop();
    }

    public void setImpulse(Vector3 vector) {
        // add force 
        Rigidbody2D rb = gameObject.GetComponentInChildren<Rigidbody2D>();
        rb.velocity = new Vector2 (vector.x * speed, vector.y * speed);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        if (collider.tag == "Ground" || collider.tag == "Obstacle" || collider.tag == "StickyWall" || collider.tag == "Breakable")
        {
            if (++hits >= MAX_HITS) {
                Utils.GetPlayer().cameraEffects.Shake(0.2f, 500, 0.4f);
                Die();
            } else {
                impactParticles.Emit(10);
                GetComponent<AudioSource>().PlayOneShot(clip_bump);
            }
        } else if (collider.tag == "Player" || collider.tag == "Boulder") {
            Die();     
        }
    }

    public void Die()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().PlayOneShot(clip_destroy);
        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }
        Destroy(gameObject, 1f);
    }

    public void Delete() 
    {
        Destroy(gameObject);
        Debug.Log("Projectile: Deleted");
    }
}
