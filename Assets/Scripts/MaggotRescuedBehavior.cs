using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaggotRescuedBehavior : NpcBehavior
{
    private float moveSpeed = 0.8f;
    private float _followRadius = 8f;

    public bool isFollowing = false;

    public Transform targetSibling;

    public AudioClip clip_sad_idle;
    public AudioClip clip_follow_start;
    public AudioClip clip_follow_stop;
    public AudioClip clip_success;

    public bool found = false;

    private bool _gone = false;

    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || Time.timeScale == 0 || _gone)
            return;

        // interact with sibling
        float distS = Vector2.Distance(targetSibling.position, transform.position);
        if (distS < _followRadius) {
            if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_success);

            Vector3 direction = targetSibling.position - transform.position;     
            body.velocity = new Vector2 (direction.x * moveSpeed, body.velocity.y);
            anim.SetTrigger("Found");
            found = true;
            return;
        }


        // interact with player
        float distP = Vector2.Distance(playerTransform.position, transform.position);
        if (distP < _followRadius && player.isPulling)
        {
            if (isFollowing == false) {
                isFollowing = true;
                if (sounds.isPlaying)
                    sounds.Stop();

                sounds.PlayOneShot(clip_follow_start);
                anim.SetBool("isFollowing", true);
            }

        }
        else {
            if (isFollowing) {
                isFollowing = false;
                anim.SetBool("isFollowing", false);
                sounds.PlayOneShot(clip_follow_stop);
            } else {
                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_sad_idle);
            }

        }
    }

    public override void hurt(float force) {
        Debug.Log("Maggot Hurt");
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_death);
        anim.SetTrigger("Die");
        isDead = true;
    }

    protected override void die() {
        Debug.Log("Maggot is DEAD");
        GetComponent<Animator>().SetBool("isDead", true);
        GetComponent<AudioSource>().enabled = false;
    }

    public override void LoadInActualState() {
        Debug.Log("LoadInActualState");
        if (isDead) {
            Debug.Log("Load DEAD");
            die();
        } else {
            Debug.Log("Load ALIVE");
        }
    }

    public void onDisappear() {
        this.gameObject.SetActive(false);
        _gone = true;
    }
}
