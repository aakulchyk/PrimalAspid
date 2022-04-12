using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private AudioClip clip_open;

    [SerializeField] private AudioClip clip_close;

    public void Open()
    {
        StartCoroutine(OpenCoroutine());
    }

    IEnumerator OpenCoroutine()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().PlayOneShot(clip_open);

        GetComponent<Animator>().SetTrigger("Opened");

        yield return new WaitForSeconds(5f);
        
        GetComponent<AudioSource>().Stop();
        GetComponent<Collider2D>().enabled = true;
    }

}
