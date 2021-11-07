using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyRatBehavior : NpcBehavior
{
    public Transform targetSibling;

    public AudioClip clip_sad_idle;
    public AudioClip clip_follow_start;
    public AudioClip clip_follow_stop;
    public AudioClip clip_success;

    private bool _gone = false;
    private bool _pulled = false;
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

        // interact with player
        float distP = Vector2.Distance(playerTransform.position, transform.position);

        if (grabbable.captured != _pulled) {
            if (grabbable.captured) {
                _pulled = true;
                if (sounds.isPlaying)
                    sounds.Stop();

                    sounds.PlayOneShot(clip_follow_start);
                anim.SetBool("pulled", true);
            } else {
                _pulled = false;
                anim.SetBool("pulled", false);
                sounds.PlayOneShot(clip_follow_stop);
            }
        } else {
            if (!sounds.isPlaying && !grabbable.captured)
                    sounds.PlayOneShot(clip_sad_idle);
        }
    }

    public override void hurt(float force) {
        Debug.Log("Baby Rat Hurt");
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_death);
        anim.SetTrigger("die");
        isDead = true;
    }

    protected override void die() {
        Debug.Log("Maggot is DEAD");
        GetComponent<Animator>().SetBool("dead", true);
        GetComponent<AudioSource>().enabled = false;
    }

    protected override void processCollision(Collision2D collision) {
        base.processCollision(collision);
        Collider2D collider = collision.collider;
        if (collider.tag == "Enemy")
        {
            hurt(0);
        }
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
