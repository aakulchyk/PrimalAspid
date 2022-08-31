using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingSpell : Projectile
{
    public const int DAMAGE_AMOUNT = 2;

    Vector3 endVelocity;
    float acceleration = 0.05f;

    void Start()
    {
        string layer = isOwnedByPlayer ? "PlayerProjectile" : "EnemyProjectile";
        string ownerLayer = isOwnedByPlayer ? "PC" : "Enemy";
        Debug.Log("Spell: created");

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(layer), LayerMask.NameToLayer(ownerLayer), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(layer), LayerMask.NameToLayer("Enemy"), false);

        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(layer), LayerMask.NameToLayer("Background"), true);

        if (impactParticles)
            impactParticles.Stop();

        ejectTime = Time.time;
    }

    void Update()
    {
        if (Time.time - ejectTime > MaxLiveTimeout)
            Die();

        Vector3 newVel = Vector3.Lerp(Vector3.zero, endVelocity, acceleration);
        body.velocity = new Vector2 (newVel.x, newVel.y);
        acceleration += 0.005f;
    }

    public void setImpulse(Vector3 vector) {
        // add force 
        endVelocity = vector;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Debug.Log("Spell: trigger enter");
        Vector3 dir = (collider.transform.position - transform.position).normalized;
        
        if (collider.tag == targetColliderTag) {
            NpcBehavior behavior = collider.gameObject.GetComponent<NpcBehavior>();
            if (behavior == null) {
                behavior = collider.GetComponentInParent<NpcBehavior>();
            }
            
            behavior.hurt(dir, Types.DamageType.Spell, DAMAGE_AMOUNT);
        }
    }

    public void Die()
    {
        if (isDead)
            return;


        isDead = true;

        Destroy(gameObject, 1f);
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
