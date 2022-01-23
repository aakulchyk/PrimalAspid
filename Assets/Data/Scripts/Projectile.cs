using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int speed;          // The speed our bullet travels
    // Start is called before the first frame update
    private Rigidbody2D rb;

    Vector3 prev_velocity;
    void Start()
    {
        Debug.Log("Projectile: created");
        StartCoroutine(DeleteAfterDelay(3));

        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float angle = SignedAngleBetween(rb.velocity, prev_velocity, transform.forward);
        //transform.Rotate(0,0,-angle);
        //Debug.Log("angle betwwen " + rb.velocity + " and " + prev_velocity + " = " + angle);
        //transform.rotation = new Quaternion(0,0, -angle, 1);
        transform.Rotate(0,0, -angle, Space.Self);

        prev_velocity = rb.velocity;
    }

    public void setImpulse(Vector3 vector) {
        Debug.Log("Projectile: Set Impulse");
        // add force 
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        //rb.velocity = Vector3.zero;
        rb.AddForce(vector * 2000);
        prev_velocity = rb.velocity;
        //Debug.Log("vector" + vector);
        //rb.AddForce(vector * speed);
        //rb.velocity = new Vector2 (vector.x * speed, vector.y * speed);
    }

    public float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n){
        // angle in [0,180]
        float angle = Vector3.Angle(a,b);
        float sign = Mathf.Sign(Vector3.Dot(n,Vector3.Cross(a,b)));

        // angle in [-179,180]
        float signed_angle = angle * sign;

        // angle in [0,360] (not used but included here for completeness)
        //float angle360 =  (signed_angle + 180) % 360;

        return signed_angle;
    }

    void OnCollisionEnter2D(Collision2D collision) {

        Collider2D collider = collision.collider;
        if (collider.tag == "Ground" && collider.tag == "Obstacle")
        {
            Debug.Log("Projectile: Hit to obstacle");
            Destroy(gameObject);
        }
    }

    IEnumerator DeleteAfterDelay(float sec) {
        yield return new WaitForSeconds(sec);
        Destroy(gameObject);
    }

    public void Delete() 
    {
        
        Debug.Log("Projectile: Deleted");
    }
}
