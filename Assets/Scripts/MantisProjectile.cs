using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisProjectile : MonoBehaviour
{
    public int speed;          // The speed our bullet travels
    //public Vector3 targetVector;    // the direction it travels
    //public int lifetime = 200;     // how long it lives before destroying itself
    //public float damage = 10;       // how much damage this projectile causes

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Projectile: created");
    }

    public void setImpulse(Vector3 vector) {
        // add force 
        Rigidbody2D rb = gameObject.GetComponentInChildren<Rigidbody2D>();
        //Debug.Log("vector" + vector);
        //rb.AddForce(vector * speed);
        rb.velocity = new Vector2 (vector.x * speed, vector.y * speed);
    }

    void OnCollisionEnter2D(Collision2D collision) {

        Collider2D collider = collision.collider;


        if (collider.tag == "Ground" && collider.tag == "Obstacle")
        {
            Debug.Log("Projectile: Hit to obstacle");
            Destroy(gameObject);
        }
    }

    public void Delete() 
    {
        Destroy(gameObject);
        Debug.Log("Projectile: Deleted");
    }
}
