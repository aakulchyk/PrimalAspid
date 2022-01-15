using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    [Header ("Reference")]
    public Rigidbody2D body;
    private Animator anim;
    private Renderer _renderer;

    protected AudioSource sounds;
    private Game game = null;
    public InteractableBehavior activeSpeaker;
    public SavePointBehavior activeSavePoint;
    protected PcAttack _attack;
    private PlayerGrabber grabber;
    private Rigidbody2D platformBody = null;
    private Transform thisTransform = null;
    public CameraEffects cameraEffects = null;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem stepParticles;
    
    [Header ("Sounds")]
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
    public const int INITIAL_HP = 2;
    public const float FLAP_MIN_TIMEOUT = 0.3f;
    public const int FLAP_STAMINA_COST = 5;
    public const int DASH_STAMINA_COST = 5;
    public const float COYOTE_TIME_SEC = 0.1f;
    public const int MAX_KNOCKBACK = 4;


    [Header ("State")]
    

    public bool faceRight;
    public float _maxSpeed;
    public float _gravityScale = 8f;
    public float _mass = 2.5f;
    public float _drag = 2f;
    public float _flapForce;
    
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

    private bool _jumpStarted = false;

    private bool _tailHitImpulse = false;

    private bool _prevInvulnerable = false;

    
    public bool _attackStarted = false;
    private bool _attackActivePhase = false;

    private LayerMask groundLayerMask;
    private int playerLayer, enemyLayer, npcLayer, ignoreRaycastLayer;

    public float pWidth, pHeight;

    private int moveX, moveY;
    private int prev_moveX = 0, prev_moveY = 0;

    private float upTime, downTime;

    private bool flap_button_triggered = false;
    private float JumpRequestTime;

    private bool hit_button_triggered = false;

    private bool dash_button_triggered = false;
    private bool prev_dash = false;

    private bool _isOnPlatform = false;


    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
        playerLayer = LayerMask.NameToLayer("PC");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        npcLayer = LayerMask.NameToLayer("NPC");
        ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private Game GetGame()
    {
        return Game.SharedInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("LOAD");

        thisTransform = this.transform;

        if (PlayerStats.playerSpawnCoord != Vector2.zero) {
            Debug.Log("Spawn PC at " + PlayerStats.playerSpawnCoord);
            thisTransform.position = PlayerStats.playerSpawnCoord;
        }

        isDead = false;
        startTime = System.DateTime.UtcNow;
        lastFlapTime = Time.time;

	    body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sounds = GetComponent<AudioSource>();
        _renderer = GetComponent<Renderer>();

        grabber = GetComponent<PlayerGrabber>();
        if (!grabber)
            Debug.LogError("ERROR: PlayerGrabber not found!");


        PlayerStats.HP = INITIAL_HP;
        
        faceRight = true;
        
        StartCoroutine(blinkInvulnerable());

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        pWidth = col.size.x;
        pHeight = col.size.y;

        GetComponent<FixedJoint2D>().enabled = false;

        Physics2D.IgnoreLayerCollision(playerLayer, npcLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Hanger"), true);
        Physics2D.IgnoreLayerCollision(playerLayer, ignoreRaycastLayer, true);

        body.gravityScale = _gravityScale;
        body.mass = _mass;
        body.drag = _drag;

        Transform t = thisTransform.Find("HitBox");
        if (t) {
            _attack = t.gameObject.GetComponent<PcAttack>();
        }
    }

    public void throwByImpulse(Vector2 vector, bool enemy = true) {
        Debug.Log("Throw player back " + vector);
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

    IEnumerator shortInvulnerability() {
        invulnerable = true;
        yield return new WaitForSeconds(0.3F);
        invulnerable = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (GetGame().isPopupOpen && Input.anyKeyDown) {
            GetGame().ClosePopup();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            //SceneManager.LoadScene("TitleScreen");
            Application.Quit();
        }

        if (isDead)
            return;

        if (Time.timeScale == 0)
            return;

        // if (_isDashing || grabber.IsGrabbing())
        //     return;

        checkGrounded();

        if (Input.GetButtonDown("Interact")) {
            if (activeSpeaker && activeSpeaker.openForDialogue) {
                activeSpeaker.talkToPlayer();
            } 
            else 
            if (activeSavePoint && activeSavePoint.canInteract) {
                activeSavePoint.SaveGame();
            }
        }

        if (Input.GetButtonDown("Hit")) {
            AnticipateAttack();
        }

        if (Input.GetButtonDown("Flap")) {
            flap_button_triggered = true;
            JumpRequestTime = Time.time;
        }

        // KeyCode.JoystickButton4 - LB
        // KeyCode.JoystickButton5 - RB

        float d_axis = Input.GetAxisRaw("Dash");
        bool dash_pressed = (d_axis>0.8f || d_axis < -0.8f) || Input.GetKey(KeyCode.G);
        if (!prev_dash && dash_pressed) {
            dash_button_triggered = true;
        }

        float treshold = 0.01f;
        float moveXfloat = Input.GetAxis ("Horizontal");
        moveX = moveXfloat > treshold ? 1 : moveXfloat < -treshold ? -1 : 0;

        float moveYfloat = Input.GetAxis ("Vertical");
        moveY = moveYfloat > treshold ? 1 : moveYfloat < -treshold ? -1 : 0;

        bool newTurn = false;   
        if (moveX > 0 && !faceRight) {
            newTurn = true;
        } else if (moveX < 0 && faceRight) {
            newTurn = true;
        }

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
            if (_isGrounded && !sounds.isPlaying) {
                //sounds.PlayOneShot(clip_walk);
                //_isPlayingWalkSound = true;
            }
        } else {
            if (move != _isMoving) {
                anim.SetBool("IsMoving", false);
                //if (_isPlayingWalkSound && sounds.isPlaying)
                //    sounds.Stop();
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
        // reset jump/flap request after some time
        if (flap_button_triggered && Time.time - JumpRequestTime > COYOTE_TIME_SEC*2)
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

        if (body.velocity.y<0f) {
            float f_axis = Input.GetAxisRaw("Float");
            bool floatPressed = (f_axis>0.5f || f_axis < -0.5f) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            //bool floatPressed = Input.GetButton("Float");//moveY > 0;
            if (!floatPressed) {
                //body.gravityScale = _gravityScale;
                if (body.velocity.y > -35f)
                    body.velocity += Vector2.up * Physics2D.gravity.y * 2.5f * Time.deltaTime;
            } else {
                //body.gravityScale = _gravityScale / 2.5f;
                if (body.velocity.y < -8.5f) 
                    body.velocity = new Vector2(body.velocity.x, -8.5f);
                _isPlayingFloatSound = true;
                if (!sounds.isPlaying)
                    sounds.PlayOneShot(clip_float);
            }
            //Debug.Log("velocity: " + body.velocity);
            anim.SetBool("IsFloating", floatPressed);

            if (!floatPressed && _isPlayingFloatSound && sounds.isPlaying) {
                _isPlayingFloatSound = false;
                sounds.Stop();
            }
        } /*else 
            body.gravityScale = _gravityScale;*/
        else if (_jumpStarted && !Input.GetButton("Flap")) {
            body.velocity += Vector2.up * Physics2D.gravity.y * 8f * Time.deltaTime;
        }

        
        body.drag = _attackActivePhase ? _drag * 20 : _drag;



        // RESTORE FLAPS
        if ((_isGrounded || grabber.IsHanging())) {
            PlayerStats.FlapsLeft = PlayerStats.MaxFlaps;
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

        dash_button_triggered = false;
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
            sounds.PlayOneShot(clip_swing_crack);
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
        body.velocity = new Vector2(faceRight ? 80 : -80, 10);

        Debug.Log("Dash " + body.velocity.x);
        _isDashing = true;
        anim.SetTrigger("Dash");
    }

    public void endDash()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        _isDashing = false;
    }

    void flip()
    {
        Vector3 scale = thisTransform.localScale;
        thisTransform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    void startFlap()
    {
        //Debug.Log("flap?");
        if (_jumpStarted || _flapStarted)
            return;

        if (PlayerStats.FlapsLeft < 1)
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

        --PlayerStats.FlapsLeft;

        _flapStarted = true;

        body.velocity = new Vector2(body.velocity.x, 0);
        //float magnitude = body.velocity.y > 0 ? body.velocity.y : 0; 
        //float force = magnitude < 4f ? _flapForce : _flapForce / magnitude;
        float force = _flapForce;

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_flap);
        
        anim.SetBool("IsFlapping", true);
        body.AddForce(new Vector2(0f, force));
    }

    public void endFlap()
    {
        anim.SetBool("IsFlapping", false);
        _flapStarted = false;
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

        sounds.PlayOneShot(clip_jump);
    }

    void StartJump()
    {
        if (_jumpStarted || _flapStarted)
            return;

        flap_button_triggered = false;

        if (_attackStarted)
            RestoreAttack();

        FinishTurnIfStarted();

        float xImpulse = 0f;
        if (grabber.IsHangingOnWall()) {
            grabber.endHangOnWall();   
            // TODO/TBD: Will wall hanging affect jump direction?
        }       

        if (grabber.IsHangingOnCeiling()) {
            grabber.endHangOnCeiling();   
        }

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;

        _jumpStarted = true;
        float force = _flapForce * 1.5f;
        if (sounds.isPlaying)
            sounds.Stop();

        JumpEffect();

        anim.SetBool("IsJumping", true);
        
        body.velocity = new Vector2(body.velocity.x, 0);
        body.AddForce(new Vector2(xImpulse, force));
    }

    public void JumpEffect()
    {
        jumpParticles.Emit(1);
        //sounds.pitch = (UnityEngine.Random.Range(0.6f, 1f));
        sounds.PlayOneShot(clip_jump);
    }

    public void LandEffect()
    {
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_land);
        landParticles.Emit(10);
        //sounds.pitch = (UnityEngine.Random.Range(0.6f, 1f));
        //sounds.PlayOneShot(clip_jump);
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
    }

    public void InterruptFlyOrJump()
    {
        if (_flapStarted)
            endFlap();
        if (_jumpStarted)
            EndJump();
    }

    void checkGrounded()
    {
        Vector3 v1 = new Vector3(0, 1, 0);
        RaycastHit2D hit = Physics2D.Raycast(thisTransform.position + Vector3.up, Vector2.down, 1.3f, groundLayerMask);

        
        bool gr = (hit.collider != null);

        if (gr != _isGrounded) {
            anim.SetBool("IsGrounded", gr);
            if (gr) {
                anim.SetTrigger("Land");
                LandEffect();
                _isPlayingWalkSound = false;
            } else {
                // Coyote time
                jumpCoyoteTimeStarted = Time.time;
            }
        }
        _isGrounded = gr;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollisionWithCollider(collision.collider, collision.relativeVelocity.magnitude);
    }

    private void ProcessCollisionWithCollider(Collider2D collider, float magnitude) {
        if (collider == null || invulnerable == true)
            return;


        if (collider.tag == "Ground" || collider.tag == "StickyWall") {
            //collider.gameObject.transform.SetParent(transform, true);
            thisTransform.SetParent(collider.gameObject.transform, true);
            platformBody = collider.gameObject.GetComponent<Rigidbody2D>();
        }

        if (collider.tag == "Boulder") {
            if (magnitude > 8) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike") {
            hurt(new Vector2(0, 3000f));
            PlayerStats.PartlyRestoreStamina(5);
        }

        if (collider.tag == "Enemy") {
            NpcBehavior behavior = collider.gameObject.GetComponent<NpcBehavior>();
            if (!behavior.isDead)
                hurt(new Vector2(0, 1000f));
        }

        if (collider.tag == "DestroyablePlatform") {
            DestroyablePlatform pl = collider.gameObject.GetComponent<DestroyablePlatform>();
            if (pl && !pl.isCollapsing) {
                pl.StartCollapsing();
            }
        }
    }

    void OnCollisionExit2d(Collision2D collision)
    {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "StickyWall") {
            thisTransform.parent = null;
            platformBody = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Lever") {
            LeverBehavior script = other.gameObject.GetComponent<LeverBehavior>();
            if (!script || script.toggled)
                return;
            
            script.SwitchLever();
        }

        if (other.tag == "LevelPortal") {
            /*System.DateTime endTime = System.DateTime.UtcNow;
            PlayerStats.Time = endTime - startTime;
            if (other.gameObject.GetComponent<LevelPortal>().backward)
                PlayerStats.currentSceneIndex--;
            else
                PlayerStats.currentSceneIndex++;

            PlayerStats.playerSpawnCoord = other.transform.Find("SpawnPoint").transform.position;
            SceneManager.LoadScene(PlayerStats.CurrentScene());*/
            other.gameObject.GetComponent<LevelPortal>().TransferToAnotherLevel(gameObject);
            return;
        }

        if (other.tag == "Collectable") {
            other.gameObject.GetComponent<PrizeBehavior>().GetCollected();
        }

        if (other.tag == "BossFightTrigger") {
            MantisBehavior boss = (MantisBehavior)FindObjectOfType(typeof(MantisBehavior));

            if (boss && !boss.isDead) {
                boss.idle = false;

                GameObject bossMusic = GameObject.Find("MantisBossFightMusic");
                bossMusic.GetComponent<AudioSource>().enabled = true;
            }
        }

        if (other.tag == "Text" && PlayerStats.ShowTutorial) { 
            Text text = other.gameObject.GetComponent<Text>();
            GetGame().SetPopupText("Tutorial", text.text);
            other.gameObject.SetActive(false);
            GetGame().OpenPopup();
        }

        if (other.tag == "HiddenRoomVeil") {
            Debug.Log("hrv");
            HiddenRoomBehavior hrb = other.gameObject.GetComponent<HiddenRoomBehavior>();
            hrb.MakeVisible(false);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "HiddenRoomVeil") {
            Debug.Log("hrv exit");
            HiddenRoomBehavior hrb = other.gameObject.GetComponent<HiddenRoomBehavior>();
            hrb.MakeVisible(true);
        }
    }

    void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes)
    {    
        if (isDead) return; // one cannot die twice...

        body.AddForce(force);
        cameraEffects.Shake(1000, 1f);
        
        if (--PlayerStats.HP < 0) {
            sounds.PlayOneShot(clip_death);
            _isPlayingWalkSound = false;
            anim.SetBool("IsDying", true);
            isDead = true;
            body.velocity = Vector2.zero;
        } else {
            anim.SetTrigger("Hurt");
            if (sounds.isPlaying)
                sounds.Stop();
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
        SceneManager.LoadScene(Game.currentScene);
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(Game.currentScene);
    }



    public void LoseAndRespawn()
    {
        PlayerStats.Losses++;
        Debug.Log("Losses: " + PlayerStats.Losses);
        StartCoroutine(RestartAfterDelay());
    }

    public void onSaveGame()
    {
        PlayerStats.HP = INITIAL_HP;
    }
}
