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

    [SerializeField] private bool _isHangingUpsideDown = false;
    [SerializeField] private bool _isHangingOnWall = false;
    private bool _isSideGrabbing = false;
    private bool _isDownwardGrabbing = false;
    private bool _isUpwardGrabbing = false;

    public bool _isPulling = false;

    private int _VerticalGrabVelocity = 20;
    private int _HorizontalGrabVelocity = 25;

    private float grabCoyoteTimeStarted;
    
    public const float GRAB_COOLDOWN_TIME_SEC = 0.75f;
    public const float GRAB_COYOTE_TIME_SEC = 0.3f;
    private float grabCooldownTimeStarted;

    private float wallJumpCoyoteTimeStarted;
    private float hangerJumpCoyoteTimeStarted;

    private Transform nearestHanger = null;
    private Collider2D _touchingWall = null;
    private bool _isWallOnRight;
    private bool _doNotGrabRightNow_kostyl = false;

    public const float WALLJUMP_COYOTE_TIME_SEC = 0.1f;

    public GrabbableBehavior activeGrabbable;

    private bool _cannotGrab = false;

    public const int GRAB_STAMINA_COST = 5;

    // RB
    private bool grab_button_triggered = false;
    private bool grab_button_hold = false;

    // RT
    //private bool dash_button_triggered = false;
    //private bool prev_dash = false;
    //private bool dash_hold = false;

    private int moveX=0, moveY=0;
    private int prev_moveX = 0, prev_moveY = 0;

    private float pWidth, pHeight;

    [SerializeField] private Transform _attachedTo = null;

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

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        pWidth = col.size.x;
        pHeight = col.size.y;
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

        /*float d_axis = Input.GetAxisRaw("Dash");
        dash_hold = (d_axis>0.8f || d_axis < -0.8f) || Input.GetKey(KeyCode.G);
        if (!prev_dash && dash_hold) {
            dash_button_triggered = true;
        }

        prev_dash = dash_hold;*/
    }

    void FixedUpdate()
    {
        if (grab_button_triggered) {
            if (!_isPulling && activeGrabbable && !_cannotGrab) {
                if (playerMainControl.IsGrounded()) {
                    playerMainControl.throwByImpulse(new Vector2(0, 3000), false);            
                    StartCoroutine(GrabBodyAfterShortDelay(activeGrabbable, 0.15f));
                } else {
                    grabBody(activeGrabbable);
                }
                grab_button_triggered = false;
            }
        }

        if (!grab_button_hold && _isPulling) {
            releaseBody();
        }


        bool direction_triggered = (prev_moveX==0 && prev_moveY==0) && (moveX!=0 || moveY!=0);
        //if (grab_button_triggered || (grab_button_hold && direction_triggered)) {

        /*if (dash_button_triggered || (dash_hold && direction_triggered)) {
            //grab_button_triggered = false;
            // gran dash            
            if (!IsHanging() && !_isPulling && !playerMainControl._attackStarted) {
                if (moveX != 0 && PlayerStats.SideGrabEnabled) {
                    startSideGrab();
                } else if (moveY>0 && PlayerStats.UpGrabEnabled) {
                    startUpwardGrab();
                } else if (moveY<0) {
                    startDownwardGrab();
                }
                dash_button_triggered = false;
            }
        }*/

        

        // if (dash_button_triggered) {
        //     if (_isHangingOnWall) {
        //         endHangOnWall();
        //         /*if (moveX!=0) {
        //             if ((_isWallOnRight && moveX < 0) || (!_isWallOnRight && moveX > 0)) {
        //                 Debug.Log("StartSideGrab?");
        //                 playerMainControl.MakeInstantTurn();
        //                 startSideGrab();
        //                 _doNotGrabRightNow_kostyl = true;
        //             }
        //         }*/
        //     }

        //     if (_isHangingUpsideDown)
        //         endHangOnCeiling();

        //     if (_isPulling)
        //         releaseBody();

        //     dash_button_triggered = false;
        // }

        bool stillCoyoteTime = Time.time - hangerJumpCoyoteTimeStarted < GRAB_COYOTE_TIME_SEC;

        bool NotHangingConditions = playerMainControl._attackStarted || stillCoyoteTime;    
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("Hanger"), moveY <= 0 || NotHangingConditions);

        if (moveY > 0 && nearestHanger) {
            Debug.Log("Start hang on ceiling");
            startHangOnCeiling();
        }

        if (moveY != 0 && _isHangingOnWall) {
            MoveOnWall(0.3f * moveY);
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

        //if (Input.GetButton("Grab")) {
        //if (dash_hold) {
            /*if (IsHanging()) {
                body.constraints |= RigidbodyConstraints2D.FreezePosition;
            }*/
        //}
        /*else {
            if (_isHangingOnWall)
                endHangOnWall();

            //if (_isPulling)
            //    throwByImpulse(new Vector2 (0, 1000), false);
            
            if (_isHangingUpsideDown)
                endHangOnCeiling();

            releaseBody();
        }*/

        prev_moveX = moveX;
        prev_moveY = moveY;
    }

    public bool IsGrabbing()
    {
        return _isSideGrabbing || _isUpwardGrabbing || _isDownwardGrabbing;
    }

    public bool IsHanging()
    {
        return _isHangingUpsideDown || _isHangingOnWall;
    }

    public bool IsHangingOnCeiling()
    {
        return _isHangingUpsideDown;
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
        if (collider.tag == "StickyWall" ) {
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC);
            _touchingWall = collider;
            _isWallOnRight = _touchingWall.transform.position.x > transform.position.x;
            //Debug.Log("IsWallOnRight: " + _isWallOnRight);
            
            if (_isSideGrabbing || stillCoyoteTime)
                startHangOnWall();
            else if ((_isWallOnRight && moveX > 0) || (!_isWallOnRight && moveX < 0))
                startHangOnWall();
            //else
            //    Debug.Log("Collided with Wall");
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Hanger")) {
            //Debug.Log("nearestHanger ON");
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC*2);
            nearestHanger = collider.gameObject.transform;
            if (_isUpwardGrabbing || stillCoyoteTime)
                startHangOnCeiling();
            //else
            //    Debug.Log("Collided with CEILING");
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if (collider.tag == "StickyWall") {
            _touchingWall = null;
        }
        if (collider.gameObject.layer == LayerMask.NameToLayer("Hanger")) {
            //Debug.Log("nearestHanger OFF");
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
                //Debug.Log("NO MORE active grabbable");
                activeGrabbable.SetCanvasActive(false);
                activeGrabbable = null;
            }
        }
    }

    public void startSideGrab()
    {
        bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
        if (stillCooldownTime)
            return;

        int potentialStamina = PlayerStats.Stamina - GRAB_STAMINA_COST;
        if (potentialStamina < 0) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

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

        if (_touchingWall && !_doNotGrabRightNow_kostyl)
            startHangOnWall();

        _doNotGrabRightNow_kostyl = false;

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
        //Debug.Log("Start Hang");

        var wall = _touchingWall.gameObject.GetComponent<StickyWallBehavior>();
        if (wall == null)
            return;

        if (playerMainControl.IsDashing()) {
            playerMainControl.endDash();
        }

        wall.AttachBody(this.gameObject, _isWallOnRight);
        //body.velocity = Vector2.zero;
        _isHangingOnWall = true;    
        _attachedTo = _touchingWall.transform;

        playerMainControl.FinishTurnIfStarted();
        playerMainControl.InterruptFlyOrJump();
        _isSideGrabbing = false;
        
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHangingOnWall", true);
        body.velocity = Vector2.zero;

    }

    public void MoveOnWall(float moveY)
    {
        if (_attachedTo == null)
            return;

        var wall = _attachedTo.gameObject.GetComponent<StickyWallBehavior>();
        if (wall == null)
            return;

        wall.MoveBody(moveY);
    }

    public void endHangOnWall()
    {
        //Debug.Log("End Hang");

        var wall = _attachedTo.gameObject.GetComponent<StickyWallBehavior>();

        if (wall) {
            wall.DetachBody();
        }

        _isHangingOnWall = false;
        anim.SetBool("IsHangingOnWall", false);
        wallJumpCoyoteTimeStarted = Time.time;
    }

    public void startUpwardGrab()
    {
        bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
        if (stillCooldownTime)
            return;

        int potentialStamina = PlayerStats.Stamina - GRAB_STAMINA_COST;
        if (potentialStamina < 0) {
            return;
        }
        PlayerStats.Stamina = potentialStamina;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("Hanger"), false);

        playerMainControl.FinishTurnIfStarted();
        playerMainControl.InterruptFlyOrJump();
        
        if (sounds.isPlaying) {
            sounds.Stop();
        }
        sounds.PlayOneShot(clip_grab);
        body.velocity = new Vector2(0, _VerticalGrabVelocity);
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        Debug.Log("Whu Grab??");
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
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("Hanger"), true);
    }

    public void startHangOnCeiling()
    {
        playerMainControl.InterruptFlyOrJump();
        if (_isUpwardGrabbing)
            endUpwardGrab();

        if (playerMainControl.IsDashing()) {
            playerMainControl.endDash();
        }
        
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHanging", true);

        var hanger = nearestHanger.gameObject.GetComponent<HangerBehavior>();

        if (hanger) {
            hanger.AttachBody(this.gameObject);
            body.velocity = Vector2.zero;
            _isHangingUpsideDown = true;
            _attachedTo = nearestHanger;
        }


        var destroyable = nearestHanger.gameObject.GetComponent<DestroyablePlatform>();

        if (destroyable) {
            Debug.Log("Destroyable");
            if (!destroyable.isCollapsing) {
                destroyable.StartCollapsing();
            }
        }
    }

    public void endHangOnCeiling()
    {
        //dash_button_triggered = false;
        anim.SetBool("IsHanging", false);
        _isHangingUpsideDown = false;

        hangerJumpCoyoteTimeStarted = Time.time;

        var hanger = _attachedTo.gameObject.GetComponent<HangerBehavior>();
        if (hanger) {
            hanger.DetachBody();
        }
    }

    public void startDownwardGrab()
    {
        bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
        if (stillCooldownTime)
            return;

        int potentialStamina = PlayerStats.Stamina - GRAB_STAMINA_COST;
        if (potentialStamina < 0) {
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
        if (playerMainControl.activeInteractor)
            playerMainControl.activeInteractor.SetActive(false);
    }

    public void releaseBody()
    {
        Debug.Log("releaseBody");
        _isPulling = false;

        // activate ability again after grabbing
        if (playerMainControl.activeInteractor)
            playerMainControl.activeInteractor.SetActive(true);

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

    public GrabbableBehavior TryGetCurrentGrabbable()
    {
        if (activeGrabbable)
            return activeGrabbable;

        Rigidbody2D b = GetComponent<FixedJoint2D>().connectedBody;

        if (b) {
            Transform t = b.transform.Find("Grabbable");
            if (!t)
                t = b.transform.parent.Find("Grabbable");
            if (t)
                return t.gameObject.GetComponent<GrabbableBehavior>();
            else
                Debug.LogError("ERROR: Cannot get current grabbable!");
            
        }
        
        return null;
    }
}


