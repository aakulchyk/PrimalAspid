using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RatFatherBehavior : NpcBehavior
{
    public AudioClip clip_sad_idle;
    public AudioClip clip_success;
    public AudioClip clip_cry;

    private float noticeRadius = 6;
    public float interactRadius = 2f;

    public Transform target;


    public string initialText;
    public string happyText;
    public string sadText;


    private bool _gone = false;

    public bool waitSuccess = false;
    public bool isTargetAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        currentText = initialText;
    }

    // Update is called once per frame
    void Update()
    {
        if (_gone) return;


        // Check if is open for interaction with player
        float pDist = Vector2.Distance(playerTransform.position, transform.position); 
        GameObject cnvs = gameObject.transform.GetChild(0).gameObject;
        openForDialogue = (pDist < interactRadius);// && !canDisappear;
        cnvs.SetActive(openForDialogue);
        
        if (openForDialogue) {
            player.activeSpeaker = this;
        }

        float dist = Vector2.Distance(target.position, transform.position);
        if (dist < noticeRadius) {
            if (!waitSuccess) {
                waitSuccess =true;
                isTargetAlive = !target.gameObject.GetComponent<BabyRatBehavior>().isDead;
                if (isTargetAlive) {
                    currentText = happyText;
                    anim.SetTrigger("found");
                }
            }
            if (!sounds.isPlaying)
                sounds.PlayOneShot( isTargetAlive ? clip_success : clip_cry);
        }

        if (!waitSuccess) {
            if (!sounds.isPlaying)
                sounds.PlayOneShot(clip_sad_idle);
        }
    }

    public override void talkToPlayer() {
        base.talkToPlayer();

        if (waitSuccess) {
            //canDisappear = true;
        }
    }
}
