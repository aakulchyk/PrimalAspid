using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System;
using System.Text;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public Rigidbody2D body;
    private Animator anim;
    private Renderer _renderer;

    protected AudioSource sounds;
    public bool faceRight;
    public float _maxSpeed;
    public float _gravityScale = 8f;
    public float _mass = 2.5f;
    public float _drag = 2f;
    public float _flapForce;
    //private Time startTime;

    private bool invulnerable;

    private bool _isMoving = false;
    private bool _isDashing = false;

    private bool _isPlayingWalkSound = false;
    private bool _isGrounded = false;

    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public AudioClip clip_flap;
    public AudioClip clip_jump;
    public AudioClip clip_land;
    public AudioClip clip_walk;

    public AudioClip clip_dash;
    public AudioClip clip_swing;

    public const int INITIAL_HP = 2;
    public const float FLAP_MIN_TIMEOUT = 0.4f;

    public const float FLAP_STAMINA_COST = 0.3f;

    public const float COYOTE_TIME_SEC = 0.1f;
    private float jumpCoyoteTimeStarted;

    private System.DateTime startTime;
    private System.DateTime prevUpdateTime;


    private bool isDead;

    private int _knockback = 0;

    private float lastFlapTime;

    private bool _turnStarted = false;
    private bool _flapStarted = false;

    private bool _jumpStarted = false;

    
    private Game game;

    public InteractableBehavior activeSpeaker;
    public SavePointBehavior activeSavePoint;

    protected PcAttack _attack;
    private PlayerGrabber grabber;
    private bool _attackStarted = false;

    private LayerMask groundLayerMask;
    private int playerLayer, enemyLayer, npcLayer;

    private float pWidth, pHeight;

    private bool dash_axis_flag = false;

    private int moveX, moveY;
    private int prev_moveX = 0, prev_moveY = 0;

    private float upTime, downTime;

    void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
        playerLayer = LayerMask.NameToLayer("PC");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        npcLayer = LayerMask.NameToLayer("NPC");
    }
    // Start is called before the first frame update
    void Start()
    {
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

        game = (Game)FindObjectOfType(typeof(Game));
        game.LoadGame();
        game.isGameInProgress = true;

        PlayerStats.HP = INITIAL_HP;
        
        faceRight = true;
        
        StartCoroutine(blinkInvulnerable());

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        pWidth = col.size.x;
        pHeight = col.size.y;

        GetComponent<FixedJoint2D>().enabled = false;

        Physics2D.IgnoreLayerCollision(playerLayer, npcLayer, true);

        body.gravityScale = _gravityScale;
        body.mass = _mass;
        body.drag = _drag;

        Transform t = transform.Find("HitBox");
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
        _knockback = 16;
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
        if (game.isPopupOpen && Input.anyKeyDown) {
            game.ClosePopup();
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

        if (_isDashing || grabber.IsGrabbing())
            return;

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

        if (Input.GetButtonDown("Flap") && !grabber.IsGrabbing() && !grabber.IsHangingOnCeiling()) {
            FinishTurnIfStarted();
            bool stillCoyoteTime = Time.time - jumpCoyoteTimeStarted < (COYOTE_TIME_SEC);
            if (_isGrounded || grabber.IsWallJumpPossible() || stillCoyoteTime) {
                StartJump();
            } else {
                startFlap();
            }
        }

        if (_jumpStarted && body.velocity.y<0f)
            EndJump();

        // KeyCode.JoystickButton4 - LB
        // KeyCode.JoystickButton5 - RB

        float d_axis = Input.GetAxisRaw("Dash");
        bool dash_pressed = (d_axis>0.8f || d_axis < -0.8f) || Input.GetKey(KeyCode.G);

        if (!dash_pressed) {
            dash_axis_flag = false;
        }
        
        if (!dash_axis_flag && dash_pressed && !_isDashing &&
            !grabber.IsDoingSomething()) {
            
            dash_axis_flag = true;
            //startDash();
            //return;
        }

        float treshold = 0.01f;
        float moveXfloat = Input.GetAxis ("Horizontal");
        moveX = moveXfloat > treshold ? 1 : moveXfloat < -treshold ? -1 : 0;

        float moveYfloat = Input.GetAxis ("Vertical");
        moveY = moveYfloat > treshold ? 1 : moveYfloat < -treshold ? -1 : 0;

        //Debug.Log("x:" + moveXfloat + " y:" + moveYfloat);


        /*if (!_isGrounded && !_jumpStarted && !_flapStarted) {
            bool floating = moveY > 0;
            //body.drag = floating ? _drag*3 : _drag;
            body.gravityScale = floating ? _gravityScale/3 : _gravityScale;
            anim.SetBool("IsFloating", floating);
        } else
            //body.drag = 2;
            body.gravityScale = _gravityScale;
*/

        //prev_moveY = moveY;

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
            } else {
                _turnStarted = true;
                if (_flapStarted)
                    endFlap();
                anim.SetTrigger("TurnTrigger");
            }
        }

        bool move = Math.Abs(body.velocity.x) > 0.1f;
        if (move) {
            if (move != _isMoving)
                anim.SetBool("IsMoving", true);
            _isMoving = true;
            if (_isGrounded && !sounds.isPlaying) {
                sounds.PlayOneShot(clip_walk);
                _isPlayingWalkSound = true;
            }
        } else {
            if (move != _isMoving) {
                anim.SetBool("IsMoving", false);
                if (_isPlayingWalkSound && sounds.isPlaying)
                    sounds.Stop();
            }
            _isMoving = false;
        }

        
    }

    void FixedUpdate()
    {
        //System.DateTime now = System.DateTime.UtcNow;
        //System.TimeSpan diff = now-prevUpdateTime;
        // var millis = diff.Milliseconds;
        
        //prevUpdateTime = now;

        float moveSpeedX = _turnStarted && _isGrounded ? 0 : _maxSpeed*moveX;
        if (_knockback > 0) {
            moveSpeedX = body.velocity.x;
            --_knockback;
        }

        if (!grabber.IsDoingSomething())
            body.velocity = new Vector2 (moveSpeedX, body.velocity.y);


        if (body.velocity.y<0f) {
            bool floatPressed = moveY > 0;
            if (!floatPressed) {
                //body.gravityScale = _gravityScale;
                if (body.velocity.y > -35f)
                    body.velocity += Vector2.up * Physics2D.gravity.y * 2.5f * Time.deltaTime;
            } else {
                //body.gravityScale = _gravityScale / 2.5f;
                if (body.velocity.y < -10.0f)
                    body.velocity = new Vector2(body.velocity.x, -10.0f);
            }

            //Debug.Log("velocity: " + body.velocity);
            anim.SetBool("IsFloating", floatPressed);
        } /*else 
            body.gravityScale = _gravityScale;*/

        // RESTORE FLAPS
        if ((_isGrounded || grabber.IsHanging())) {
            PlayerStats.FlapsLeft = PlayerStats.MaxFlaps;
        }

        // RESTORE STAMINA
        var millis = 16.6f;
        if (!grabber.IsPulling()) {
            TryRestoreStamina(0.00006f * millis);
        }
    }

    private void TryRestoreStamina(float delta)
    {   
        if (PlayerStats.Stamina < 1 - delta)
            PlayerStats.Stamina += delta;
        else
            PlayerStats.Stamina = 1f;
    }

    public void AnticipateAttack()
    {
        if (_isDashing || grabber.IsDoingSomething())
            return;
        
        FinishTurnIfStarted();
        InterruptFlyOrJump();
                
        _attackStarted = true;
        anim.SetTrigger("SwingAttack");
    }

    public void PerformAttack()
    {
        sounds.PlayOneShot(clip_swing);
        _isPlayingWalkSound = false;
        if (_attack)
            _attack.Animate();
    }

    public void RestoreAttack()
    {
        _attackStarted = false;
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

    public void startDash()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        StartCoroutine(shortInvulnerability());
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
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
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    void startFlap()
    {
        if (PlayerStats.FlapsLeft < 1)
            return;
        if (_jumpStarted || _flapStarted)
            return;

        if (_attackStarted)
            return;

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;
        
        /*float potentialStamina = PlayerStats.Stamina - FLAP_STAMINA_COST;

        if (potentialStamina < 0f) {
            PlayerStats.Stamina = 0f;
            return;
        }*/

        --PlayerStats.FlapsLeft;

        _flapStarted = true;
        //PlayerStats.Stamina = potentialStamina;

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

    void StartJump()
    {
        if (_jumpStarted || _flapStarted)
            return;

        if (_attackStarted)
            RestoreAttack();

        FinishTurnIfStarted();

        float xImpulse = 0f;
        if (grabber.IsHangingOnWall()) {
            grabber.endHangOnWall();   
            // TODO/TBD: Will wall hanging affect jump direction?
            //_knockback = 80;
            /*if (_touchingWall) {
                bool wallFromLeft = _touchingWall.gameObject.transform.position.x < transform.position.x;
                xImpulse = 5000 * (wallFromLeft ? 1f:-1f);
            }*/
        }       

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;

        _jumpStarted = true;
        float force = _flapForce * 1.5f;
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_jump); // TODO jump sound
        anim.SetBool("IsJumping", true);
        
        body.velocity = new Vector2(body.velocity.x, 0);
        body.AddForce(new Vector2(xImpulse, force));
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, 1.0f, groundLayerMask);

        
        bool gr = (hit.collider != null);

        if (gr != _isGrounded) {
            anim.SetBool("IsGrounded", gr);
            if (gr) {
                downTime = Time.time - lastFlapTime;
                //Debug.Log("UpTime: " + upTime + ", DownTime: " + downTime);
                anim.SetTrigger("Land");
                sounds.PlayOneShot(clip_land);
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
        Collider2D collider = collision.collider;

        if (invulnerable == true)
            return;

        if (collider.tag == "Boulder") {
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike") {
            hurt(new Vector2(0, 2500f));
            TryRestoreStamina(0.1f);
        }

        if (collider.tag == "Enemy") {
            NpcBehavior behavior = collider.gameObject.GetComponent<NpcBehavior>();
            if (!behavior.isDead)
                hurt(new Vector2(0, 1000f));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "LevelPortal") {
            System.DateTime endTime = System.DateTime.UtcNow;
            PlayerStats.Time = endTime - startTime;
            SceneManager.LoadScene("WinScreen");
            return;
        }

        if (other.tag == "Lever") {
            LeverBehavior script = other.gameObject.GetComponent<LeverBehavior>();
            if (!script || script.toggled)
                return;
            
            script.SwitchLever();
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
            game.SetPopupText("Tutorial", text.text);
            other.gameObject.SetActive(false);
            game.OpenPopup();
        }
    }

    void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes)
    {    
        if (isDead) return; // one cannot die twice...

        body.AddForce(force);
        
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

        if (_isDashing)
            endDash();
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
        SceneManager.LoadScene("Level_2");
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Level_2");
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
