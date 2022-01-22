using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private AudioClip clip_splash;
    [SerializeField] private AudioClip clip_jumpout;

    void OnTriggerEnter2D(Collider2D other) {
        //if (other.tag == "Player") {
            
            GetComponent<AudioSource>().PlayOneShot(clip_splash);
        //}
    }


    void OnTriggerExit2D(Collider2D other) {
        //if (other.tag == "Player") {
        GetComponent<AudioSource>().PlayOneShot(clip_jumpout);
        //}
    }
}
