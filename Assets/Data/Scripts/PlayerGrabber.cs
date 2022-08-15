using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System;
using System.Text;
using System.Collections;

public class PlayerGrabber : MonoBehaviour
{
    private PlayerControl playerMainControl;
    private Rigidbody2D body;
    private FixedJoint2D joint;
    private AudioSource sounds;
    private Animator anim;

    public AudioClip clip_clutch;
    public AudioClip clip_grab;

    [Header ("Input")]
    public PlayerInputActions playerInputActions;
    private InputAction moveAction;
    private InputAction grabAction;


    [Header ("GrabStates")]
    [SerializeField] private bool _isHangingUpsideDown = false;
    [SerializeField] private bool _isHangingOnWall = false;
    private bool _isSideGrabbing = false;
    private bool _isDownwardGrabbing = false;
    private bool _isUpwardGrabbing = false;

    public bool _isPulling = false;

    private int _VerticalGrabVelocity = 20;
    private int _HorizontalGrabVelocity = 25;

    private float grabCoyoteTimeStarted;
    
    public const float GRAB_COOLDOWN_TIME_SEC = 0.3f;
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

    public const int GRAB_STAMINA_COST = 1;

    // RT
    private bool grab_button_triggered = false;
    private bool grab_button_hold = false;
    private bool bodygrab_from_ground_started = false;

    private int moveX=0, moveY=0;
    private int prev_moveX = 0, prev_moveY = 0;

    private float pWidth, pHeight;

    [SerializeField] private Transform _attachedTo = null;

    void Awake()
    {
        body = GetComponentInParent<Rigidbody2D>();
        joint = GetComponentInParent<FixedJoint2D>();
        sounds = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        playerMainControl = GetComponent<PlayerControl>();
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        moveAction = playerInputActions.Player.Move;
        moveAction.Enable();

        grabAction = playerInputActions.Player.Grab;
        grabAction.Enable();
        grabAction.started += OnGrabPressed;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        grabAction.Disable();

    }

    private void OnGrabPressed(InputAction.CallbackContext context)
    {
        if (Game.SharedInstance.isPopupOpen) return;
        grab_button_triggered = true;
        grabCoyoteTimeStarted = Time.time;
        anim.SetTrigger("Grab");

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
        if (null == moveAction) {
            Debug.LogError("ERROR: No moveAction!");
            return;
        }

        float treshold = 0.5f;
        Vector2 moveDirection = moveAction.ReadValue<Vector2>();
        float moveXfloat = moveDirection.x;

        moveX = moveXfloat > treshold ? 1 : moveXfloat < -treshold ? -1 : 0;

        float moveYfloat = moveDirection.y;
        moveY = moveYfloat > treshold ? 1 : moveYfloat < -treshold ? -1 : 0;


        /*bool curr_grab = grabAction.ReadValue<float>() > 0.05f;

        if (!grab_button_triggered && !grab_button_hold && curr_grab)
            grab_button_triggered = true;
        grab_button_hold = curr_grab;*/

        /*float d_axis = Input.GetAxisRaw("Dash");
        dash_hold = (d_axis>0.8f || d_axis < -0.8f) || Input.GetKey(KeyCode.G);
        if (!prev_dash && dash_hold) {
            dash_button_triggered = true;
        }

        prev_dash = dash_hold;*/
    }

    void FixedUpdate()
    {
        // reset jump/flap request after some time
        if (grab_button_triggered && Time.time - grabCoyoteTimeStarted > GRAB_COYOTE_TIME_SEC)
            grab_button_triggered = false;


        if (grab_button_triggered) {
            if (!_isPulling && activeGrabbable && !_cannotGrab) {
                bool stillCooldownTime = Time.time - grabCooldownTimeStarted < GRAB_COOLDOWN_TIME_SEC;
                if (stillCooldownTime)
                        return;
                grab_button_triggered = false;
                if (playerMainControl.IsGrounded() && !bodygrab_from_ground_started) {
                    playerMainControl.throwByImpulse(new Vector2(0, 3000), false);
                    StartCoroutine(GrabBodyAfterShortDelay(activeGrabbable, 0.2f));
                } else {
                    grabBody(activeGrabbable);
                }
            } else if (_isPulling) {
                grab_button_triggered = false;
                releaseBody();
            }
        }

        //bool direction_triggered = (prev_moveX==0 && prev_moveY==0) && (moveX!=0 || moveY!=0);

        bool stillCoyoteTime = Time.time - hangerJumpCoyoteTimeStarted < GRAB_COYOTE_TIME_SEC;

        bool NotHangingConditions = playerMainControl._attackStarted || stillCoyoteTime;    
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("Hanger"), /*moveY <= 0*/!grab_button_triggered || NotHangingConditions);

        if (grab_button_triggered && nearestHanger) {
            Debug.Log("Start hang on ceiling");
            startHangOnCeiling();
        }

        if (moveY != 0 && _isHangingOnWall) {
            MoveOnWall(0.2f * moveY);
        }

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

    void OnCollisionStay2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if (collider.tag == "StickyWall" ) {
            if (_isHangingOnWall) {
                return;
            }
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC);
            _touchingWall = collider;
            _isWallOnRight = _touchingWall.transform.position.x > transform.position.x;
            if (_isSideGrabbing || stillCoyoteTime || grab_button_hold)
                startHangOnWall();
            return;
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Hanger")) {
            if (_isHangingUpsideDown) {
                return;
            }
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC*2);
            nearestHanger = collider.gameObject.transform;
            if (_isUpwardGrabbing || stillCoyoteTime)
                startHangOnCeiling();

        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        /*if (collider.tag == "StickyWall" ) {
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC);
            _touchingWall = collider;
            _isWallOnRight = _touchingWall.transform.position.x > transform.position.x;
            
            if (_isSideGrabbing || stillCoyoteTime || grab_button_hold)
                startHangOnWall();
        }*/

        if (collider.gameObject.layer == LayerMask.NameToLayer("Hanger")) {
            bool stillCoyoteTime = Time.time - grabCoyoteTimeStarted < (GRAB_COYOTE_TIME_SEC*2);
            nearestHanger = collider.gameObject.transform;
            if (_isUpwardGrabbing || stillCoyoteTime)
                startHangOnCeiling();

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

    void OnTriggerStay2D(Collider2D other)
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

        wall.AttachBody(transform.parent.gameObject, _isWallOnRight);
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
        Debug.Log("Hanger: " + hanger);

        if (hanger) {
            hanger.AttachBody(transform.parent.gameObject);
            body.velocity = Vector2.zero;
            _isHangingUpsideDown = true;
            _attachedTo = nearestHanger;
        }


        var destroyable = nearestHanger.gameObject.GetComponent<DestroyablePlatform>();

        if (destroyable) {
            Debug.Log("Destroyable");
            if (!destroyable.isShaking) {
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
        bodygrab_from_ground_started = true;
        yield return new WaitForSeconds(delay);
        grabBody(grabbable);
        bodygrab_from_ground_started = false;
    }

    public void grabBody(GrabbableBehavior grabbable)
    {
        Debug.Log("grabBody");
        grabbable.getCaptured();
        Rigidbody2D b = activeGrabbable.gameObject.GetComponentInParent<Rigidbody2D>();
        joint.connectedBody = b;
        float coeff = b.gameObject.transform.localScale.y;
        joint.connectedAnchor  = new Vector2(0f, +1.2f / coeff);
        joint.enabled = true;
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

        Rigidbody2D b = joint.connectedBody;

        if (b) {
            Transform t = b.transform.Find("Grabbable");
            if (!t)
                t = b.transform.parent.Find("Grabbable");
            if (t)
                t.gameObject.GetComponent<GrabbableBehavior>().getReleased();
            joint.connectedBody = null;
        }

        joint.enabled = false;
        grabCooldownTimeStarted = Time.time;
    }

    public GrabbableBehavior TryGetCurrentGrabbable()
    {
        if (activeGrabbable)
            return activeGrabbable;

        Rigidbody2D b = joint.connectedBody;

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


