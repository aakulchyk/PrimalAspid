using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaggotWaitingBehavior : MonoBehaviour
{
    private bool waitSuccess = false;
    private Animator anim;
    private Rigidbody2D body; 
    private AudioSource audio;
    private float noticeRadius = 6;
    private float _moveSpeed = 0.8f;

    private bool wallOpened = false;
    
    public Transform target;
    public GameObject controlledWall;
    
    public AudioClip clip_sad_idle;
    public AudioClip clip_success;

    
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>(); 
        body = GetComponent<Rigidbody2D>();
    }

    IEnumerator OpenDoorAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        controlledWall.GetComponent<Animator>().SetTrigger("Opened");
        yield return new WaitForSeconds(0.5f);
        controlledWall.GetComponent<AudioSource>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector2.Distance(target.position, transform.position);

        if (dist < noticeRadius) {
            if (!waitSuccess) {
                waitSuccess = true;
                anim.SetTrigger("Found");

                // open the wall
                if (!wallOpened) {
                    wallOpened = true;
                    StartCoroutine(OpenDoorAfterDelay(2));
                }
            }

            Vector3 direction = target.position - transform.position;
            body.velocity = new Vector2 (direction.x * _moveSpeed, direction.y * _moveSpeed);

            if (!audio.isPlaying)
                audio.PlayOneShot(clip_success);
        }

        if (!waitSuccess) {
            if (!audio.isPlaying)
                audio.PlayOneShot(clip_sad_idle);
        }
    }
}

