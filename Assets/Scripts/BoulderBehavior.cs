using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderBehavior : MonoBehaviour
{
    private AudioSource sounds;

    public AudioClip sound_hit;
    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


     void OnCollisionEnter2D(Collision2D collision) {
         Collider2D collider = collision.collider;
        if (collider.tag == "Ground" || collider.tag == "Obstacle") {
            if (collision.relativeVelocity.magnitude > 1) {

                sounds.PlayOneShot(sound_hit);
                
            }
        }
     }
}
