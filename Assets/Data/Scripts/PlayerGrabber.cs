using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System;
using System.Text;
using System.Collections;

//public delegate void SimpleCallback();

public class PlayerGrabber : MonoBehaviour
{
    private PlayerControl playerMainControl;
    private Rigidbody2D body;
    private AudioSource sounds;
    private Animator anim;

    public AudioClip clip_clutch;
    public AudioClip clip_grab;

    private bool _isHanging = false;
    private bool _isHangingOnWall = false;
    private bool _isSideGrabbing = false;
    private bool _isDownwardGrabbing = false;
    private bool _isUpwardGrabbing = false;

    public bool _isPulling = false;

    private int _VerticalGrabVelocity = 20;
    private int _HorizontalGrabVelocity = 20;

    private float grabCoyoteTimeStarted;
    
    public const float GRAB_COOLDOWN_TIME_SEC = 0.75f;
    public const float GRAB_COYOTE_TIME_SEC = 0.2f;
    private float grabCooldownTimeStarted;

    private float wallJumpCoyoteTimeStarted;

    private Transform nearestHanger = null;
    private Collider2D _touchingWall = null;

    public const float WALLJUMP_COYOTE_TIME_SEC = 0.1f;

    public GrabbableBehavior activeGrabbable;

    private bool _cannotGrab = false;

    public const float GRAB_STAMINA_COST = 0.25f;

    private bool grab_button_triggered = false;
    private bool grab_button_hold = false;

    private int moveX=0, moveY=0;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sounds = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        playerMainControl = GetComponent<PlayerControl>();
    }

    void Start()
    {
        if (!playerMainControl)
            Debug.LogError("ERROR: No PlayerControl!");
    }

    void Update()
    {
        float treshold = 0.01f;
        float moveXfloat = Input.GetAxis ("Horizontal");
        moveX = moveXfloat > treshold ? 1 : moveXfloat < -treshold ? -1 : 0;

        float moveYfloat = Input.GetAxis ("Vertical");
        moveY = moveYfloat > treshold ? 1 : moveYfloat < -treshold ? -1 : 0;

        if (Input.GetButtonDown("Grab"))
            grab_button_triggered = true;

        grab_button_hold = Input.GetButton("Grab");

    }
    void FixedUpdate()
    {
        if (grab_button_triggered) {
            grab_button_triggered = false;
             if (moveX != 0) {
                //StartCoroutine(shortInvulnerability());
                startSideGrab();
            } else 
                if (moveY>0) {
                 //StartCoroutine(shortInvulnerability());
                 startUpwardGrab();
            } else if (moveY<0) {
                //StartCoroutine(shortInvulnerability());
                startDownwardGrab();
            }
            

            // TODO rethink mechanic
            if (activeGrabbable && !_cannotGrab) {
                /*if (_isGrounded) {
                    throwByImpulse(new Vector2(0, 2000), false);            
                    StartCoroutine(GrabBodyAfterShortDelay(activeGrabbable, 0.15f));
                } else {*/
                    grabBody(activeGrabbable);
                //}
            }
        }

        /*if (activeGrabbable && IsGrabbing()) {
            Debug.Log("Grab body?");
            Rigidbody2D b = activeGrabbable.gameObject.GetComponentInParent<Rigidbody2D>();
            if (b && !_cannotGrab) {
                _isSideGrabbing = _isDownwardGrabbing = _isUpwardGrabbing = false;
                Debug.Log("Grab body");
                if (_isGrounded) {
                    throwByImpulse(new Vector2(0, 2000), false);            
                    StartCoroutine(GrabBodyAfterShortDelay(activeGrabbable, b, 0.15f));
                } else {
                    grabBody(activeGrabbable, b);
                }
            }
        }*/

        if (Input.GetButton("Grab")) {
            if (IsHanging()) {
                Debug.Log("freeze pos");
                body.constraints |= RigidbodyConstraints2D.FreezePosition;
            }
        }
        else {
            if (_isHangingOnWall)
                endHangOnWall();

            /*if (_isPulling)
                throwByImpulse(new Vector2 (0, 1000), false);*/
            
            if (_isHanging)
                endHangOnCeiling();

            releaseBody();
        }
    }

    public bool IsGrabbing()
    {
        return _isSideGrabbing || _isUpwardGrabbing || _isDownwardGrabbing;
    }

    public bool IsHanging()
    {
        return _isHanging || _isHangingOnWall;
    }

    public bool IsHangingOnCeiling()
    {
        return _isHanging;
    }
    
    public bool IsHangingOnWall()
    {
        return _isHangingOnWall;
    }

    public bool IsPulling()
    {
        return _isPulling;
    }
    
    public bool IsDoingSomething()
    {
        return IsGrabbing() || IsHanging();
    }

    public bool IsWallJumpPossible()
    {
        bool stillCoyoteTime =  Time.time - wallJumpCoyoteTimeStarted < WALLJUMP_COYOTE_TIME_SEC;
        return _isHangingOnWall || stillCoyoteTime;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("StickyWall")) {
            bool stillCoyoteTime = Input.GetButton("Grab") && Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC);
            _touchingWall = collider;
            if (_isSideGrabbing || stillCoyoteTime)
                startHangOnWall();
            else
                Debug.Log("Collided with Wall");
        }

        if (collider.tag == "Hanger") {
            bool stillCoyoteTime = Input.GetButton("Grab") && Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC*2);
            nearestHanger = collider.gameObject.transform;
            if (_isUpwardGrabbing || stillCoyoteTime)
                startHangOnCeiling();
            else
                Debug.Log("Collided with CEILING");
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("StickyWall")) {
            _touchingWall = null;
        }

        if (collider.tag == "Hanger") {
            nearestHanger = null;
        } 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Grabbable") {
            activeGrabbable = other.gameObject.GetComponent<GrabbableBehavior>();
            if (activeGrabbable) {
                Debug.Log("Now has active grabbable");
                activeGrabbable.SetCanvasActive(true);
                if (IsGrabbing()) {
                    grabBody(activeGrabbable);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Grabbable") {
            if (activeGrabbable) {
                Debug.Log("NO MORE active grabbable");
            activeGrabbable.SetCanvasActive(false);
                activeGrabbable = null;
            }
        }
    }

    public void startSideGrab()
    {
        /*float potentialStamina = PlayerStats.Stamina - GRAB_STAMINA_COST;
        if (potentialStamina < 0f) {
            return;
        }*/

        //PlayerStats.Stamina = potentialStamina;

        bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
        if (stillCooldownTime)
            return;

        playerMainControl.FinishTurnIfStarted();
        playerMainControl.InterruptFlyOrJump();

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_grab);
        body.velocity = new Vector2(playerMainControl.faceRight ? _HorizontalGrabVelocity : -_HorizontalGrabVelocity, 0);
        body.constraints |= RigidbodyConstraints2D.FreezePositionY;

        Debug.Log("SideGrab " + body.velocity.x);
        _isSideGrabbing = true;
        anim.SetTrigger("SideGrab");

        if (_touchingWall)
            startHangOnWall();

        grabCooldownTimeStarted = Time.time;
    }

    public void endSideGrab()
    {
        Debug.Log("End SideGrab ");
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        body.velocity = new Vector2(0, body.velocity.y);
        _isSideGrabbing = false;
        grabCoyoteTimeStarted = Time.time;
        
    }

    public void startHangOnWall()
    {
        Debug.Log("Start Hang");
        playerMainControl.InterruptFlyOrJump();
        _isSideGrabbing = false;
        _isHangingOnWall = true;
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHangingOnWall", true);
        body.velocity = Vector2.zero;
    }

    public void endHangOnWall()
    {
        Debug.Log("End Hang");
        _isHangingOnWall = false;
        body.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        anim.SetBool("IsHangingOnWall", false);
        wallJumpCoyoteTimeStarted = Time.time;
    }

    public void startUpwardGrab()
    {
        /*float potentialStamina = PlayerStats.Stamina - GRAB_STAMINA_COST;
        if (potentialStamina < 0f) {
            return;
        }
        PlayerStats.Stamina = potentialStamina;*/

        bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
        if (stillCooldownTime)
            return;

        playerMainControl.FinishTurnIfStarted();
        playerMainControl.InterruptFlyOrJump();
        
        if (sounds.isPlaying) {
            sounds.Stop();
        }
        sounds.PlayOneShot(clip_grab);
        body.velocity = new Vector2(0, _VerticalGrabVelocity);
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        anim.SetTrigger("Grab");
        grabCoyoteTimeStarted = Time.time;
        grabCooldownTimeStarted = Time.time;
        _isUpwardGrabbing = true;
    }

    
    public void endUpwardGrab()
    {
        Debug.Log("End UpGrab ");
        body.gravityScale = playerMainControl._gravityScale;
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        _isUpwardGrabbing = false;
    }

    public void startHangOnCeiling()
    {
        endUpwardGrab();
        if (sounds.isPlaying)
            sounds.Stop();    
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHanging", true);

        //BoxCollider2D bc = nearestHanger.gameObject.GetComponent<BoxCollider2D>();
        //float w = bc.size.x;
        //transform.position = nearestHanger.position + new Vector3(w/2, -pHeight*1.5f, 0);
        body.constraints |= RigidbodyConstraints2D.FreezePosition;

        _isHanging = true;
    }

    public void endHangOnCeiling()
    {
        anim.SetBool("IsHanging", false);
        _isHanging = false;
        body.constraints &= ~RigidbodyConstraints2D.FreezePosition;
    }

    public void startDownwardGrab()
    {
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }
        PlayerStats.Stamina = potentialStamina;
        playerMainControl.FinishTurnIfStarted();
        playerMainControl.InterruptFlyOrJump();
        if (sounds.isPlaying) {
            sounds.Stop();
        }
        sounds.PlayOneShot(clip_grab);
        body.velocity = new Vector2(0, -_VerticalGrabVelocity);
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        anim.SetTrigger("GrabDown");
        grabCoyoteTimeStarted = Time.time;
        _isDownwardGrabbing = true;
    }

    
    public void endDownwardGrab()
    {
        Debug.Log("End DownGrab ");
        body.gravityScale = playerMainControl._gravityScale;
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        _isDownwardGrabbing = false;
    }

    IEnumerator GrabBodyAfterShortDelay(GrabbableBehavior grabbable, float delay)
    {
        yield return new WaitForSeconds(delay);
        grabBody(grabbable);
    }

    public void grabBody(GrabbableBehavior grabbable)
    {
        Debug.Log("grabBody");
        grabbable.getCaptured();
        Rigidbody2D b = activeGrabbable.gameObject.GetComponentInParent<Rigidbody2D>();
        GetComponent<FixedJoint2D>().connectedBody = b;
        float coeff = b.gameObject.transform.localScale.y;
        GetComponent<FixedJoint2D>().connectedAnchor  = new Vector2(0f, +1.2f / coeff);
        GetComponent<FixedJoint2D>().enabled = true;
        _isPulling = true;

        // deactivate ability to speak while grabbing
        if (playerMainControl.activeSpeaker)
            playerMainControl.activeSpeaker.SetActive(false);
    }

    public void releaseBody()
    {
        _isPulling = false;

        // activate ability again after grabbing
        if (playerMainControl.activeSpeaker)
            playerMainControl.activeSpeaker.SetActive(true);

        Rigidbody2D b = GetComponent<FixedJoint2D>().connectedBody;

        if (b) {
            Transform t = b.transform.Find("Grabbable");
            if (!t)
                t = b.transform.parent.Find("Grabbable");
            if (t)
                t.gameObject.GetComponent<GrabbableBehavior>().getReleased();
            GetComponent<FixedJoint2D>().connectedBody = null;
        }

        GetComponent<FixedJoint2D>().enabled = false;
        //StartCoroutine(shortInabilityToGrab());
    }
}
