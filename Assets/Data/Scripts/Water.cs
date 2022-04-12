using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private AudioClip clip_splash;
    [SerializeField] private AudioClip clip_jumpout;

    [SerializeField] private AudioClip clip_reflux;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            GetComponent<AudioSource>().PlayOneShot(clip_splash);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            GetComponent<AudioSource>().PlayOneShot(clip_jumpout);
        }
    }

    public void SetCameraFocusAndReflux()
    {
        StartCoroutine(Reflux());
    }

    IEnumerator Reflux()
    {

        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().PlayOneShot(clip_reflux);

        GetComponent<Animator>().SetTrigger("Opened");

        yield return new WaitForSeconds(5f);
        
        GetComponent<AudioSource>().Stop();
        GetComponent<Collider2D>().enabled = true;
    }

}
