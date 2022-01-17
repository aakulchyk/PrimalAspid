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

    public string[] happyTexts;
    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Hanger"), true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || Time.timeScale == 0 || _gone)
            return;

        Transform pt = PlayerTransform();

        if (pt == null)
            return;
        // interact with player
        float distP = Vector2.Distance(pt.position, transform.position);

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

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        base.hurt(force);
    }

    protected override void die() {
        base.die();
        GetComponent<Animator>().SetBool("dead", true);
        GetComponent<AudioSource>().enabled = false;
    }

    protected override void processCollision(Collision2D collision) {
        base.processCollision(collision);
        Collider2D collider = collision.collider;
        if (collider.tag == "Enemy")
        {
            hurt(Vector2.zero);
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

    public void onFound() {
        Debug.Log("Bady rat: on found");
        if (grabbable) {
            Utils.GetPlayer().releaseBody();
            grabbable.gameObject.SetActive(false);
        }

        if (interactable)
            interactable.currentTexts = happyTexts;
    }
}
