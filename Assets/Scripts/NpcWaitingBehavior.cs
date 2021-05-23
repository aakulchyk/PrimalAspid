using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaitState
{
    Waiting = 0,
    Happy = 1,
    Sad = 2
}

public class NpcWaitingBehavior : MonoBehaviour
{
    public bool waitSuccess = false;
    private bool isTargetAlive = true;

    private Animator anim;
    private Rigidbody2D body; 
    private AudioSource sounds;
    private float noticeRadius = 6;
    //private float _moveSpeed = 0.8f;

    

    private bool wallOpened = false;
    
    public Transform target;
    public GameObject controlledWall;
    
    public AudioClip clip_sad_idle;
    public AudioClip clip_success;

    public AudioClip clip_cry;

    public WaitState waitState = WaitState.Waiting;

    private bool _loading = false;
    //private bool isCurrentlyLoading = false;
    
    public string happyText;
    public string sadText;
    
    private Game game;
    // Start is called before the first frame update
    void Start()
    {
        game = (Game)FindObjectOfType(typeof(Game));
        anim = GetComponent<Animator>();
        sounds = GetComponent<AudioSource>(); 
        body = GetComponent<Rigidbody2D>();
    }

    IEnumerator OpenDoorAfterDelay(float time)
    {
        if (controlledWall == null) yield break;
        yield return new WaitForSeconds(time);
        controlledWall.GetComponent<Animator>().SetTrigger("Opened");
        yield return new WaitForSeconds(0.5f);
        controlledWall.GetComponent<AudioSource>().enabled = true;

        // every door opening is a checkpoint
        game.SaveGame();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (_loading==false) {
            if (waitState != WaitState.Waiting) {
                
            }
        }*/
        float dist = Vector2.Distance(target.position, transform.position);

        if (dist < noticeRadius) {
            //Debug.Log("Waitting: found");
            if (!waitSuccess || _loading) {

                isTargetAlive = !target.gameObject.GetComponent<MaggotRescuedBehavior>().isDead;

                if (isTargetAlive) {

                    if (!_loading) {
                        PlayerStats.NpcsSavedAlive++;
                        
                        game.SetPopupText(happyText);
                        game.OpenPopup();
                    }
                    waitState = WaitState.Happy;
                    anim.SetTrigger("Found");
                }
                else {

                    if (!_loading) {
                        PlayerStats.NpcsLostDead++;
                        game.SetPopupText(sadText);
                        game.OpenPopup();
                    }
                    waitState = WaitState.Sad;
                    anim.SetTrigger("FoundDead");
                }
                
                waitSuccess = true;
                _loading = false;
                

                // open the wall
                if (!wallOpened) {
                    wallOpened = true;
                    StartCoroutine(OpenDoorAfterDelay(2));
                }
            }

            /*Vector3 direction = target.position - transform.position;
            body.velocity = new Vector2 (direction.x * _moveSpeed, direction.y * _moveSpeed);*/

            if (!sounds.isPlaying)
                sounds.PlayOneShot( isTargetAlive ? clip_success : clip_cry);
        }

        if (!waitSuccess) {
            if (!sounds.isPlaying)
                sounds.PlayOneShot(clip_sad_idle);
        }
    }


    public void LoadInActualState() {
        if (waitSuccess) {
            if (controlledWall) {
                controlledWall.GetComponent<Animator>().SetTrigger("Opened");
                wallOpened = true;
            }

            _loading = true;

            /*if (target) {
                isTargetAlive = !target.gameObject.GetComponent<MaggotRescuedBehavior>().isDead;
                anim.SetTrigger(isTargetAlive ? "Found" : "FoundDead");
                waitState = isTargetAlive ? WaitState.Happy : WaitState.Sad;
            } else {
                Debug.Log("Error: no target");
            }*/

        }
    }
}

