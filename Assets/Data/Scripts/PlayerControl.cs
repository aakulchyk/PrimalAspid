using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Assertions;

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;






public class PlayerControl : MonoBehaviour
{
    [Header ("Reference")]
    public Rigidbody2D body;
    private Animator anim;
    private Renderer _renderer;

    public InteractableBehavior activeInteractor;

    [SerializeField] private PcAttack _attack;
    private PlayerGrabber grabber;
    private Rigidbody2D platformBody = null;
    private Transform thisTransform = null;
    public CameraEffects cameraEffects = null;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem stepParticles;
    [SerializeField] private ParticleSystem jumpTrailParticles;

    [SerializeField] private GameObject flapTrail;

    [Header ("Input")]
    public PlayerInputActions playerInputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction floatAction;
    private InputAction hitAction;
    private InputAction interactAction;

    private InputAction menuAction;
    private InputAction submitAction;
    // temp
    private InputAction exitAction;

    
    [Header ("Sounds")]
    protected AudioSource sounds;
    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public AudioClip clip_flap;
    public AudioClip clip_jump;
    public AudioClip clip_land;
    public AudioClip clip_walk;

    public AudioClip clip_dash;
    public AudioClip clip_swing;
    public AudioClip clip_swing_crack;
    public AudioClip clip_float;

    [Header ("Constants")]
    public const float FLAP_MIN_TIMEOUT = 0.3f;
    public const int FLAP_STAMINA_COST = 1;
    public const int DASH_STAMINA_COST = 1;
    public const float COYOTE_TIME_SEC = 0.1f;
    public const int MAX_KNOCKBACK = 4;


    [Header ("State")]
    

    public bool faceRight;
    public float _maxSpeed;
    public float _gravityScale = 8f;
    public float _mass = 2.5f;
    public float _drag = 2f;
    [SerializeField] private float _flapForce;
    [SerializeField] private float _jumpForce;
    
    [Header ("Private")]
    private float jumpCoyoteTimeStarted;
    private bool invulnerable;

    private bool _isMoving = false;
    [SerializeField] private bool _isDashing = false;

    private bool _isPlayingWalkSound = false;
    private bool _isPlayingFloatSound = false;
    public bool _isGrounded = false;
    private System.DateTime startTime;
    private System.DateTime prevUpdateTime;

    private bool isDead;

    private int _knockback = 0;

    private float lastFlapTime;

    private bool _turnStarted = false;
    private bool _flapStarted = false;
    private bool _floatStarted = false;

    private bool _jumpStarted = false;

    private bool _tailHitImpulse = false;

    private bool _prevInvulnerable = false;

    
    public bool _attackStarted = false;
    private bool _attackActivePhase = false;

    private bool _controlEnabled = true;

    private LayerMask groundLayerMask;
    private int playerLayer, enemyLayer, npcLayer, ignoreRaycastLayer;

    public float pWidth, pHeight;

    private int moveX, moveY;

    private float upTime, downTime;

    private bool flap_button_triggered = false;
    private float JumpRequestTime;

    private bool hit_button_triggered = false;

    private bool dash_button_triggered = false;
    private bool prev_dash = false;

    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
        playerLayer = LayerMask.NameToLayer("PC");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        npcLayer = LayerMask.NameToLayer("NPC");
        ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        moveAction = playerInputActions.Player.Move;
        moveAction.Enable();

        lookAction = playerInputActions.Player.Look;
        lookAction.Enable();

        jumpAction = playerInputActions.Player.Jump;
        jumpAction.Enable();

        floatAction = playerInputActions.Player.Float;
        floatAction.Enable();
        jumpAction.performed += OnJumpPressed;

        hitAction = playerInputActions.Player.Fire;
        hitAction.Enable();
        hitAction.performed += OnHitPressed;

        interactAction = playerInputActions.Player.Interact;
        interactAction.Enable();
        interactAction.performed += OnInteractPressed;

        menuAction = playerInputActions.UI.Menu;
        menuAction.Enable();
        menuAction.performed += OnMenuPressed;

        submitAction = playerInputActions.UI.Submit;
        submitAction.Enable();
        submitAction.performed += OnSubmitPressed;

        exitAction = playerInputActions.UI.Exit;
        exitAction.Enable();
        exitAction.performed += OnExitPressed;


        InputUser.onChange += onInputDeviceChange;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        floatAction.Disable();
        hitAction.Disable();
        interactAction.Disable();
        menuAction.Disable();
        submitAction.Disable();
        exitAction.Disable();

        InputUser.onChange -= onInputDeviceChange;
    }

    private Game GetGame()
    {
        return Game.SharedInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        thisTransform = this.transform;

        isDead = false;
        startTime = System.DateTime.UtcNow;
        lastFlapTime = Time.time;

	    body = GetComponentInParent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sounds = GetComponent<AudioSource>();
        _renderer = GetComponent<Renderer>();

        grabber = GetComponent<PlayerGrabber>();
        if (!grabber)
            Debug.LogError("ERROR: PlayerGrabber not found!");

        faceRight = true;

        checkGrounded(true);

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        pWidth = col.size.x;
        pHeight = col.size.y;

        GetComponentInParent<FixedJoint2D>().enabled = false;

        Physics2D.IgnoreLayerCollision(playerLayer, npcLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Hanger"), true);
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Background"), true);
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("PlayerProjectile"), true);

        // Ignore collision with enemies to not to stuck to them
        // To detect damage the trigger, which is child of emeny's object is used
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemy"), true);

        body.gravityScale = _gravityScale;
        body.mass = _mass;
        body.drag = _drag;

        /*Transform t = thisTransform.parent.Find("HitBox");
        if (t) {
            _attack = t.gameObject.GetComponent<PcAttack>();
        }*/

        jumpTrailParticles.Stop();
    }

    public void throwByImpulse(Vector2 vector, bool enemy = true) {
        StartCoroutine(shortInvulnerability());
        body.AddForce(vector);
    }

    public void knockback(Vector2 force) {
        _knockback = MAX_KNOCKBACK;
        body.velocity = force;
    }

    IEnumerator blinkInvulnerable() {
        invulnerable = true;
        for (int i=0; i<5; i++)
        {
            _renderer.enabled = false;
            yield return new WaitForSeconds(0.15F);
            _renderer.enabled = true;
            yield return new WaitForSeconds(0.15F);
        }
        invulnerable = false;
    }

    IEnumerator ShowJumpTrail() {
        jumpTrailParticles.Play();
        yield return new WaitForSeconds(0.2F);
        jumpTrailParticles.Stop();
    }

    IEnumerator ShowFlapTrail() {
        flapTrail.SetActive(true);
        yield return new WaitForSeconds(1F);
        flapTrail.SetActive(false);
    }

    IEnumerator shortInvulnerability() {
        invulnerable = true;
        yield return new WaitForSeconds(0.3F);
        invulnerable = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        if (Time.timeScale == 0)
            return;

        if (!_controlEnabled) {
            anim.SetBool("IsMoving", false);
            return;
        }

        checkGrounded();

        // KeyCode.JoystickButton4 - LB
        // KeyCode.JoystickButton5 - RB

        /*float d_axis = Input.GetAxisRaw("Dash");
        bool dash_pressed = (d_axis>0.8f || d_axis < -0.8f) || Input.GetKey(KeyCode.G);
        if (!prev_dash && dash_pressed) {
            dash_button_triggered = true;
            prev_dash = true;
        }

        if (!dash_pressed && prev_dash) {
            prev_dash = false;
        }*/


        Vector2 moveDirection = moveAction.ReadValue<Vector2>();

        float treshold = 0.5f;
        float moveXfloat = moveDirection.x;//Input.GetAxis ("Horizontal");
        moveX = moveXfloat > treshold ? 1 : moveXfloat < -treshold ? -1 : 0;

        float moveYfloat = moveDirection.y;//Input.GetAxis ("Vertical");
        moveY = moveYfloat > treshold ? 1 : moveYfloat < -treshold ? -1 : 0;

        bool newTurn = false;   
        if (moveX > 0 && !faceRight) {
            newTurn = true;
        } else if (moveX < 0 && faceRight) {
            newTurn = true;
        }

        Vector2 lookDirection = lookAction.ReadValue<Vector2>().normalized;
        cameraEffects.SetPlayerOffset(new Vector3(-lookDirection.x*10, -lookDirection.y*10, 0));

        if (!_turnStarted && newTurn && !_attackStarted) {
            if (grabber.IsHangingOnCeiling()) {
                _turnStarted = true;
                onTurnFinished();
            } else if (grabber.IsHangingOnWall()) {
                //faceRight = !faceRight;
            } else {
                _turnStarted = true;
                if (_flapStarted)
                    endFlap();

                if (_jumpStarted) 
                    onTurnFinished();
                else
                    anim.SetTrigger("TurnTrigger");
            }
        }

        bool move = moveX!=0;//Math.Abs(moveX) > 0.1f;
        if (move) {
            if (move != _isMoving)
                anim.SetBool("IsMoving", true);
            _isMoving = true;
        } else {
            if (move != _isMoving) {
                anim.SetBool("IsMoving", false);
            }
            _isMoving = false;
        }

        if (_prevInvulnerable != invulnerable) {
            _prevInvulnerable = invulnerable;

            if (!invulnerable) {
                //List<Collider2D> results;
                Collider2D[] results = new Collider2D[8];
                Physics2D.OverlapCollider(GetComponent<CapsuleCollider2D>(), new ContactFilter2D(), results);

                foreach(var collider in results) {
                    ProcessCollisionWithCollider(collider, 0);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!_controlEnabled) {
            body.velocity = new Vector2 (0, body.velocity.y);
            return;
        }
        // reset jump/flap request after some time
        if (flap_button_triggered && Time.time - JumpRequestTime > COYOTE_TIME_SEC)
            flap_button_triggered = false;

        float moveSpeedX = _turnStarted && _isGrounded ? 0 : _maxSpeed*moveX;
        if (_knockback > 0) {
            moveSpeedX = body.velocity.x;
            --_knockback;
        }

        if (_isDashing || grabber.IsGrabbing()) {
            body.drag = _drag;
            return;
        }

        if (flap_button_triggered && !grabber.IsGrabbing()) {
            FinishTurnIfStarted();
            bool stillCoyoteTime = Time.time - jumpCoyoteTimeStarted < COYOTE_TIME_SEC;

            if (grabber.IsHanging() && moveY<0) {
                JumpOffHanger();
            }
            else
            if (_isGrounded || grabber.IsWallJumpPossible() || grabber.IsHangingOnCeiling() || stillCoyoteTime ||
                (grabber.IsPulling() && grabber.TryGetCurrentGrabbable().isGrounded())) {
                StartJump();
            } else {
                startFlap();
            }
        }

        if (_jumpStarted && body.velocity.y<0f)
            EndJump();

        if (!grabber.IsDoingSomething())
            body.velocity = new Vector2 (moveSpeedX, body.velocity.y);

        
        if (_isGrounded && platformBody) {
            body.velocity += new Vector2(platformBody.velocity.x, 0);
        }

        if (!_jumpStarted && !_isGrounded/*body.velocity.y<0f*/ && !grabber.IsHanging()) {
            //float f_axis = Input.GetAxisRaw("Float");
            //bool floatPressed = (f_axis>0.5f || f_axis < -0.5f) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool floatPressed = floatAction.ReadValue<float>() > 0.5f;
            //bool floatPressed = Input.GetButton("Float");//moveY > 0;
            if (!floatPressed) {
                _floatStarted = false;
                if (body.velocity.y > -35f)
                    body.velocity += Vector2.up * Physics2D.gravity.y * 2.5f * Time.deltaTime;
            } else {
                
                if (body.velocity.y < -8.5f) {
                    _floatStarted = true;
                    body.velocity = new Vector2(body.velocity.x, -8.5f);
                    _isPlayingFloatSound = true;
                    if (!sounds.isPlaying)
                        sounds.PlayOneShot(clip_float);
                }
                
            }
            anim.SetBool("IsFloating", floatPressed);

            if (!floatPressed && _isPlayingFloatSound && sounds.isPlaying) {
                _isPlayingFloatSound = false;
                sounds.Stop();
            }
        } /*else 
            body.gravityScale = _gravityScale;*/
        else if (_jumpStarted && jumpAction.ReadValue<float>() < 0.5f) {
            body.velocity += Vector2.up * Physics2D.gravity.y * 8f * Time.deltaTime;
        }

        body.drag = _attackActivePhase ? _drag * 20 : _drag;

        // RESTORE FLAPS
        if ((_isGrounded || grabber.IsHanging())) {
            //PlayerStats.FlapsLeft = PlayerStats.MaxFlaps;
            PlayerStats.FullyRestoreStamina();
        }

        if (_tailHitImpulse) {
            _tailHitImpulse = false;
            if (_isGrounded && moveX == 0) {
                body.velocity = ((faceRight ? Vector2.right : Vector2.left) * 50);
            } else if (!_isGrounded && moveX != 0) {
                body.velocity = Vector2.zero;
            }
        }

        if (dash_button_triggered) {
            //startDash();
            dash_button_triggered = false;
        }
    }

    private void OnHitPressed(InputAction.CallbackContext context)
    {
        if (GetGame().isPopupOpen) return;
        AnticipateAttack();
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (GetGame().isPopupOpen) return;
        flap_button_triggered = true;
        JumpRequestTime = Time.time;
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (GetGame().isPopupOpen) return;

        if (!_isGrounded) return;

        if (activeInteractor && activeInteractor.openForInteraction) {
            activeInteractor.Interact();
        } 
    }

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        if (GetGame().isPopupOpen) {
            return;
        }

        if (GetGame().isMenuOpen) {
            Time.timeScale = 1;
            GetGame().CloseInGameMenu();
        }
        else {
            Time.timeScale = 0;
            GetGame().OpenInGameMenu();
        }
    }

    private void OnSubmitPressed(InputAction.CallbackContext context)
    {
        if (GetGame().isPopupOpen) {
            GetGame().ClosePopup();
            return;
        }
    }
    
    private void OnExitPressed(InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    public void AnticipateAttack()
    {
        if (_attackStarted || _isDashing || grabber.IsDoingSomething())
            return;
        
        FinishTurnIfStarted();
        InterruptFlyOrJump();
                
        _attackStarted = true;
        anim.SetTrigger("SwingAttack");
        sounds.pitch = 1;
        sounds.PlayOneShot(clip_swing);
    }

    public void InterruptCurrentAnimations()
    {
        FinishTurnIfStarted();
        InterruptFlyOrJump();
    }

    public void OnAnticipationFinished()
    {
        _attackActivePhase = true;
    }

    public void PerformAttack()
    {
        _isPlayingWalkSound = false;
        if (_attack)
            _attack.Animate();

        if (_isGrounded) {
            sounds.pitch = 1;
            sounds.PlayOneShot(clip_swing_crack);
        }

        if (grabber.IsPulling()) {
            grabber.releaseBody();
        }

        _tailHitImpulse = true;
    }

    public void RestoreAttack()
    {
        _attackStarted = false;
        _attackActivePhase = false;
    }

    
    public void onTurnFinished()
    {
        // prevent turning two times during attack
        if (!_turnStarted)
            return;
        _turnStarted = false;
        faceRight = !faceRight;
        flip();
    }

    public void MakeInstantTurn()
    {
        faceRight = !faceRight;
        flip();
    }

    public void startDash()
    {
        if (grabber.IsHanging())
            return;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        StartCoroutine(shortInvulnerability());
        int potentialStamina = PlayerStats.Stamina - DASH_STAMINA_COST;
        if (potentialStamina < 0) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        FinishTurnIfStarted();
        InterruptFlyOrJump();

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(faceRight ? 70 : -70, 10);

        Debug.Log("Dash " + body.velocity.x);
        _isDashing = true;
        anim.SetTrigger("Dash");
    }

    public bool IsDashing()
    {
        return _isDashing;
    }
    public void endDash()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        _isDashing = false;
    }

    void flip()
    {
        Vector3 scale = thisTransform.parent.transform.localScale;
        thisTransform.parent.transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    void startFlap()
    {
        if (_jumpStarted || _flapStarted)
            return;

        if (!PlayerStats.BatWingsUnlocked)
            return;

        if (_attackStarted)
            return;

        flap_button_triggered = false;

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;
        
        int potentialStamina = PlayerStats.Stamina - FLAP_STAMINA_COST;

        if (potentialStamina < 0)
            return;

        PlayerStats.Stamina = potentialStamina;

        if (grabber.IsHangingOnCeiling()) {
            grabber.endHangOnCeiling();   
            // TODO/TBD: Will wall hanging affect jump direction?
        }
        if (grabber.IsHangingOnCeiling()) {
            grabber.endHangOnCeiling();   
            // TODO/TBD: Will wall hanging affect jump direction?
        } 

        //--PlayerStats.FlapsLeft;

        _flapStarted = true;

        body.velocity = new Vector2(body.velocity.x, 0);
        //float magnitude = body.velocity.y > 0 ? body.velocity.y : 0; 
        //float force = magnitude < 4f ? _flapForce : _flapForce / magnitude;

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.pitch = 1;
        sounds.PlayOneShot(clip_flap);
        
        anim.SetBool("IsFlapping", true);
        body.AddForce(new Vector2(0f, _flapForce));

        StartCoroutine(ShowJumpTrail());
       //StartCoroutine(ShowFlapTrail());
    }

    public void endFlap()
    {
        anim.SetBool("IsFlapping", false);
        _flapStarted = false;
    }

    public bool WingsOpen()
    {
        return _flapStarted || _floatStarted;
    }

    void JumpOffHanger()
    {
        flap_button_triggered = false;
        if (grabber.IsHangingOnWall()) {
            grabber.endHangOnWall();   
        }       

        if (grabber.IsHangingOnCeiling()) {
            grabber.endHangOnCeiling();   
        }

    // TODO
        sounds.PlayOneShot(clip_jump);
        StartCoroutine(ShowJumpTrail());
    }

    void StartJump()
    {
        if (_jumpStarted || _flapStarted)
            return;

        //GameObject fx = GameObject.Find("PF_VFXgraph_Hit01");
        //fx.GetComponent<VisualEffect>().Play();

        float jumpForceCoefficient = 1.0f;
        flap_button_triggered = false;

        if (_attackStarted)
            RestoreAttack();

        FinishTurnIfStarted();

        float xImpulse = 0f;
        if (grabber.IsHangingOnWall()) {
            jumpForceCoefficient = 0.9f;
            grabber.endHangOnWall();   
        }       

        if (grabber.IsHangingOnCeiling()) {
            jumpForceCoefficient = 0.9f;
            grabber.endHangOnCeiling();   
        }

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("Hanger"), true);

        _jumpStarted = true;
        
        if (sounds.isPlaying)
            sounds.Stop();

        JumpEffect();

        anim.SetBool("IsJumping", true);
        
        body.velocity = new Vector2(body.velocity.x, 0);
        body.AddForce(new Vector2(xImpulse, _jumpForce * jumpForceCoefficient));
    }

    public void JumpEffect()
    {
        if (_isGrounded)
            jumpParticles.Emit(1);
        //sounds.pitch = (UnityEngine.Random.Range(0.6f, 1f));
        sounds.pitch = 1;
        sounds.PlayOneShot(clip_jump);

        //jumpTrailParticles.Play();
        StartCoroutine(ShowJumpTrail());
    }

    public void LandEffect()
    {
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.pitch = (UnityEngine.Random.Range(0.85f, 1f));
        sounds.PlayOneShot(clip_land);
        //jumpTrailParticles.Stop();
        landParticles.Emit(10);
    }

    public void WalkEffect()
    {
        stepParticles.Emit(10);
        sounds.pitch = (UnityEngine.Random.Range(0.6f, 1f));
        sounds.PlayOneShot(clip_walk);
    }

    void EndJump()
    {
        upTime = Time.time - lastFlapTime;
        _jumpStarted = false;
        anim.SetBool("IsJumping", false);
        jumpTrailParticles.Stop();
    }

    public void InterruptFlyOrJump()
    {
        if (_flapStarted)
            endFlap();
        if (_jumpStarted)
            EndJump();
        _floatStarted = false;
    }

    void checkGrounded(bool onStart = false)
    {
        Vector3 v1 = new Vector3(0, 1, 0);
        RaycastHit2D hit = Physics2D.Raycast(thisTransform.position + Vector3.up, Vector2.down, 1.3f, groundLayerMask);
        
        bool gr = (hit.collider != null);

        if (gr != _isGrounded) {
            anim.SetBool("IsGrounded", gr);
            if (gr) {
                anim.SetTrigger("Land");
                if (!onStart)
                    LandEffect();
                _isPlayingWalkSound = false;
                _floatStarted = false;
            } else {
                // Coyote time
                jumpCoyoteTimeStarted = Time.time;
            }
        }
        _isGrounded = gr;
    }

    public bool IsGrounded() { return _isGrounded; }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollisionWithCollider(collision.collider, collision.relativeVelocity.magnitude);
    }

    private void ProcessCollisionWithCollider(Collider2D collider, float magnitude) {
        if (collider == null || invulnerable == true)
            return;

        if (collider.tag == "Ground" || collider.tag == "StickyWall") {
            thisTransform.parent.SetParent(collider.gameObject.transform, true);
            platformBody = collider.gameObject.GetComponent<Rigidbody2D>();
        }

        if (collider.tag == "Boulder") {
            if (magnitude > 8) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike") {
            hurt(new Vector2(0, 10f));
            PlayerStats.PartlyRestoreStamina(1);
        }

        if (collider.tag == "DestroyablePlatform") {
            DestroyablePlatform pl = collider.gameObject.GetComponent<DestroyablePlatform>();
            if (pl && !pl.isShaking) {
                if (pl.needsExtraEffort)
                    pl.JustShake();
                else
                    pl.StartCollapsing();
            }
        }
    }

    void OnCollisionExit2d(Collision2D collision)
    {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "StickyWall") {
            thisTransform.parent.parent = null;
            platformBody = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "LevelPortal") {
            other.gameObject.GetComponent<LevelPortal>().TransferToAnotherLevel(thisTransform.parent.gameObject);
            return;
        }

        if (other.tag == "Collectable") 
        {
            other.gameObject.GetComponent<Collectable>().GetCollected();
        }

        if (other.tag == "Trigger") {
            other.gameObject.GetComponent<DoorTrigger>().GetTriggered();
        }
        
        if (other.tag == "BossFightTrigger") {
            MantisBehavior boss = (MantisBehavior)FindObjectOfType(typeof(MantisBehavior));

            if (boss && !boss.isDead) {
                boss.idle = false;

                GameObject bossMusic = GameObject.Find("MantisBossFightMusic");
                bossMusic.GetComponent<AudioSource>().enabled = true;
            }
        }

        if (other.tag == "Enemy") {
            GameObject parentObj = other.transform.parent.gameObject;
            NpcBehavior behavior =  parentObj.GetComponent<NpcBehavior>();
            if (!behavior.isDead)
                hurt((GetComponent<Collider2D>().gameObject.transform.position - transform.position).normalized * -20f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
    }

    public void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes)
    {    
        if (isDead) return; // one cannot die twice...
        if (invulnerable) return;

        knockback(force);
        cameraEffects.Shake(0.6f, 1000, 1f);
        
        if (--PlayerStats.HP < 0) {
            body.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
             sounds.volume = 0.5f;
            sounds.PlayOneShot(clip_death);
            _isPlayingWalkSound = false;
            anim.SetBool("IsDying", true);
            isDead = true;
        } else {
            anim.SetTrigger("Hurt");
            if (sounds.isPlaying)
                sounds.Stop();
            sounds.pitch = 1;
            sounds.volume = 0.5f;
            sounds.PlayOneShot(clip_hurt);
            _isPlayingWalkSound = false;
            StartCoroutine(blinkInvulnerable());
        }
        
        FinishTurnIfStarted();
        InterruptFlyOrJump();
        if (_attackStarted)
            RestoreAttack();
        if (_isDashing)
            endDash();
        if (grabber.IsHangingOnWall())
            grabber.endHangOnWall();   
        if (grabber.IsHangingOnWall())
            grabber.endHangOnWall();
    }

    public void releaseBody()
    {
        grabber.releaseBody();
    }

    public void FinishTurnIfStarted()
    {
        if (_turnStarted)
            onTurnFinished();
    }

    public bool IsPulling()
    {
        return grabber.IsPulling();
    }

    void dieAndRespawn()
    {
        anim.SetBool("IsDying", false);
        PlayerStats.Deaths++;
        Debug.Log("Deaths: " + PlayerStats.Deaths);
        
        StartCoroutine(DarkenScreenAndReload());
    }

    IEnumerator DarkenScreenAndReload()
    {
        Game.SharedInstance.DarkenScreen();
        yield return new WaitForSeconds(0.5F);
        Game.SharedInstance.LoadGame(Game.SharedInstance.selectedSaveSlot);
    }

    public void onSaveGame()
    {
    }

    public void EnableControl(bool val)
    {
        body.velocity = Vector3.zero;
        _controlEnabled = val;
    }

    void onInputDeviceChange(InputUser user, InputUserChange change, InputDevice device) {
        if (change == InputUserChange.ControlSchemeChanged) {
            //updateButtonImage(user.controlScheme.Value.name);
            var output = JsonUtility.ToJson(user.controlScheme, true);

            if (user.controlScheme != null) {
                InputControlScheme scheme = user.controlScheme.Value;

                PlayerPrefs.SetString( "ControlScheme", scheme.name);
            }
        }
    }
}
