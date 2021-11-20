using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
"Of no... My dear son...",
"My family was ambushed by a gang of bats... They were so sudden...",
"They kindapped my dear wife...what for?",
"Me and my older son Chris tried to protect her, but they were too fierce... They just killed him!",
"My youngest daughter was able to flee... Where is she now?",
"I am injured and too weak to search for her... Please help me! Please find Kiki and bring her back!",
"If I could protect my family... *Sob*",
*/


public class RatFatherBehavior : NpcBehavior
{
    public AudioClip clip_sad_idle;
    public AudioClip clip_success;
    public AudioClip clip_cry;

    public float noticeRadius = 8;
    public float interactRadius = 2f;

    public Transform target;


    public string[] initialTexts;
    public string[] happyTexts;
    public string[] sadTexts;


    private bool _gone = false;

    public bool waitSuccess = false;
    public bool isTargetAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        interactable.currentTexts = initialTexts;
    }

    // Update is called once per frame
    void Update()
    {
        if (_gone) return;

        float dist = Vector2.Distance(target.position, transform.position);
        if (dist < noticeRadius) {
            if (!waitSuccess) {
                waitSuccess =true;
                BabyRatBehavior baby = target.gameObject.GetComponent<BabyRatBehavior>();
                if (baby) {
                    isTargetAlive = !baby.isDead;
                    if (isTargetAlive) {
                        baby.onFound();
                        interactable.currentTexts = happyTexts;
                        anim.SetTrigger("found");
                    }
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
}
