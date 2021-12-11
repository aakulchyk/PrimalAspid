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
    private bool faceRight;
    public float _maxSpeed = 5;
    public float _flapForce;
    //private Time startTime;
    public bool isPulling = false;


    private bool invulnerable;

    private bool _isMoving = false;
    private bool _isDashing = false;
    private bool _isHanging = false;

    private bool _isHangingOnWall = false;

    private bool _isSideGrabbing = false;
    private bool _isUpwardGrabbing = false;

    private bool _isPlayingWalkSound = false;

    private bool _isGrounded = false;

    protected AudioSource sounds;
    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public AudioClip clip_flap;
    public AudioClip clip_jump;
    public AudioClip clip_land;
    public AudioClip clip_walk;

    public AudioClip clip_dash;
    public AudioClip clip_swing;
    public AudioClip clip_clutch;

    public const int INITIAL_HP = 2;
    public const float FLAP_MIN_TIMEOUT = 0.4f;

    public const float COYOTE_TIME_SEC = 0.1f;
    private float jumpCoyoteTimeStarted;
    private float grabCoyoteTimeStarted;

    private System.DateTime startTime;
    private System.DateTime prevUpdateTime;


    private bool isDead;

    private int _knockback = 0;

    private float lastFlapTime;

    private bool _turnStarted = false;
    private bool _flapStarted = false;

    private bool _jumpStarted = false;

    private bool _cannotGrab = false;

    
    private Game game;

    public InteractableBehavior activeSpeaker;
    public SavePointBehavior activeSavePoint;
    public GrabbableBehavior activeGrabbable;

    protected PcAttack _attack;
    private bool _attackStarted = false;

    private Transform nearestHanger = null;
    private Collider2D _touchingWall = null;

    public LayerMask groundLayer;

    private float pWidth, pHeight;

    private bool dash_axis_flag = false;

    // Start is called before the first frame update
    void Start()
    {
        
        isDead = false;
        startTime = System.DateTime.UtcNow;
        lastFlapTime = Time.time;

	    body = GetComponent<Rigidbody2D> ();
        anim = GetComponent<Animator>();  
        _renderer = GetComponent<Renderer>();
        sounds = GetComponent<AudioSource>();

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

        Physics2D.IgnoreLayerCollision(7, 6, true);

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

    IEnumerator shortInabilityToGrab() {
        _cannotGrab = true;
        yield return new WaitForSeconds(0.5F);
        _cannotGrab = false;
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

        if (_isDashing || _isSideGrabbing)
            return;

        checkGrounded();

        if (Input.GetButtonDown("Interact")) {
            if (activeSpeaker && activeSpeaker.openForDialogue) {
                
                /*var speakerPosX = activeSpeaker.transform.parent.position.x;
                if (transform.position.x - speakerPosX < 1f )
                    transform.position = activeSpeaker.transform.parent.position + new Vector3(-4f, 0, 0);
                if (!faceRight) {
                    faceRight = true;
                    flip();
                }*/
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

        if (Input.GetButton("Flap") 
                && !_isHanging && !_isHangingOnWall
                && !_isUpwardGrabbing && !_isSideGrabbing) {

            if (_turnStarted)
                onTurnFinished();

            bool stillCoyoteTime = Time.time - jumpCoyoteTimeStarted < COYOTE_TIME_SEC;
            if (_isGrounded || stillCoyoteTime) {
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
        bool dash_triggered = (d_axis>0.9f || d_axis < -0.9f) || Input.GetKeyDown(KeyCode.G);

        if (dash_axis_flag && !dash_triggered) {
            dash_axis_flag = false;
        }
        
        if (!dash_axis_flag && dash_triggered && 
            !_isDashing && !_isSideGrabbing &&
            !_isUpwardGrabbing && !isPulling &&
            !_isHanging && !_isHangingOnWall) {
            
            dash_axis_flag = true;
            startDash();
            return;
        }

        float moveXfloat = Input.GetAxis ("Horizontal");
        int moveX = moveXfloat > 0.3f ? 1 : moveXfloat < -0.3f ? -1 : 0;

        bool newTurn = false;   
        if (moveX > 0 && !faceRight) {
            newTurn = true;
        } else if (moveX < 0 && faceRight) {
            newTurn = true;
        }

        if (!_turnStarted && newTurn && !_attackStarted) {
            if (_isHanging) {
                _turnStarted = true;
                onTurnFinished();
            } else {
                _turnStarted = true;
                InterruptFlyOrJump();
                anim.SetTrigger("TurnTrigger");
                
            }
        }

        float moveSpeedX = _turnStarted && _isGrounded ? 0 : _maxSpeed*moveX;
        if (_knockback > 0) {
            moveSpeedX = body.velocity.x;
            --_knockback;
        }

        body.velocity = new Vector2 (moveSpeedX, body.velocity.y);
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
                // stop walking sounds
                if (_isPlayingWalkSound && sounds.isPlaying)
                    sounds.Stop();
            }
            _isMoving = false;
        }

        bool up = Input.GetKey(KeyCode.UpArrow) || Input.GetAxis ("Vertical") > 0.1f;
        if (Input.GetButtonDown("Grab")) {
             if (up) {
                 startUpwardGrab();
                 return;
            }
            if (moveX != 0) {
                startSideGrab();
                return;
            }
        }

        if (Input.GetButton("Grab")) {
            if (_isHangingOnWall || _isHanging) {
                body.constraints |= RigidbodyConstraints2D.FreezePosition;
            }
            // TODO rethink mechanic
            if (activeGrabbable) {
                Debug.Log("try to grab body");
                Rigidbody2D b = activeGrabbable.gameObject.GetComponentInParent<Rigidbody2D>();
                if (b && !_cannotGrab) {
                    if (_isGrounded) {
                        throwByImpulse(new Vector2(0, 50), false);
                        b.constraints |= RigidbodyConstraints2D.FreezePosition;
                        StartCoroutine(GrabBodyAfterShortDelay(activeGrabbable, b, 0.1f));
                    } else {
                        grabBody(activeGrabbable, b);
                    }
                }
            }
        }
        else {
            if (_isHangingOnWall)
                endHangOnWall();

            if (isPulling)
                throwByImpulse(new Vector2 (0, 15), false);
            
            if (_isHanging)
                endHangOnCeiling();

            releaseBody();
        }
    }

    void FixedUpdate()
    {
        System.DateTime now = System.DateTime.UtcNow;
        //System.TimeSpan diff = now-prevUpdateTime;
        // var millis = diff.Milliseconds;
        var millis = 16.6f;
        if ((_isGrounded || _isHanging || _isHangingOnWall)) {
            var delta = (0.002f * millis);
            if (PlayerStats.Stamina < 1 - delta)
                PlayerStats.Stamina += delta;
            else
                PlayerStats.Stamina = 1f;
        } else if (!isPulling) {
            var delta = (0.00003f * millis);
            if (PlayerStats.Stamina < 1 - delta)
                PlayerStats.Stamina += delta;
            else
                PlayerStats.Stamina = 1f;

        }
        //prevUpdateTime = now;
    }

    public void AnticipateAttack()
    {
        if (_isDashing || _isSideGrabbing || _isHanging)
            return;
        
        if (_turnStarted)
            onTurnFinished();

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


    IEnumerator GrabBodyAfterShortDelay(GrabbableBehavior grabbable, Rigidbody2D b, float delay)
    {
        yield return new WaitForSeconds(delay);
        b.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        grabBody(grabbable, b);
    }

    public void grabBody(GrabbableBehavior grabbable, Rigidbody2D b)
    {
        grabbable.getCaptured();
        GetComponent<FixedJoint2D>().connectedBody = b;
        float coeff = b.gameObject.transform.localScale.y;
        GetComponent<FixedJoint2D>().connectedAnchor  = new Vector2(0f, +1.2f / coeff);
        GetComponent<FixedJoint2D>().enabled = true;
        isPulling = true;

        // deactivate ability to speak while grabbing
        if (activeSpeaker)
            activeSpeaker.SetActive(false);
    }

    public void releaseBody()
    {
        isPulling = false;

        // activate ability again after grabbing
        if (activeSpeaker)
            activeSpeaker.SetActive(true);

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

        StartCoroutine(shortInabilityToGrab());
    }
    
    public void onTurnFinished()
    {
        // prevent turning two times during attack
        if (!_turnStarted) return;

        _turnStarted = false;
        faceRight = !faceRight;
        flip();
    }

    public void startDash()
    {
        Physics2D.IgnoreLayerCollision(7, 8, true);

        StartCoroutine(shortInvulnerability());
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        if (_turnStarted)
            onTurnFinished();
        
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
        Physics2D.IgnoreLayerCollision(7, 8, false);
        _isDashing = false;
    }

    public void startSideGrab()
    {
        StartCoroutine(shortInvulnerability());
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        if (_turnStarted) {
            onTurnFinished();
        }
        
        InterruptFlyOrJump();

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(faceRight ? 20 : -20, 0);
        //body.drag = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionY;

        Debug.Log("SideGrab " + body.velocity.x);
        _isSideGrabbing = true;
        anim.SetTrigger("SideGrab");


        if (_touchingWall)
            startHangOnWall();
    }

    public void endSideGrab()
    {
        //body.drag = 2;
        Debug.Log("End SideGrab ");
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        body.velocity = new Vector2(0, body.velocity.y);
        _isSideGrabbing = false;
        grabCoyoteTimeStarted = Time.time;
    }

    public void startHangOnWall()
    {
        Debug.Log("Start Hang");
        //endSideGrab();
        InterruptFlyOrJump();
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
    }

    public void startUpwardGrab()
    {
        StartCoroutine(shortInvulnerability());
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        if (_turnStarted) {
            onTurnFinished();
        }
        
        InterruptFlyOrJump();

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(0, 10);
        //body.drag = 0;
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        Debug.Log("UpGrab " + body.velocity.y);
        anim.SetTrigger("Grab");
        _isUpwardGrabbing = true;
        grabCoyoteTimeStarted = Time.time;
    }

    public void endUpwardGrab()
    {
        Debug.Log("End UpGrab ");
        //body.drag = 2;
        body.gravityScale = 4.4f;
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        //body.velocity = new Vector2(body.velocity.x, 0);
        _isUpwardGrabbing = false;
    }

    public void startHangOnCeiling()
    {
        endUpwardGrab();
        InterruptFlyOrJump();
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

    void flip()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    void startFlap()
    {
        if (_jumpStarted || _flapStarted)
            return;

        if (_attackStarted)
            return;

        if (Time.time - lastFlapTime <= FLAP_MIN_TIMEOUT)
            return;

        lastFlapTime = Time.time;
        
        float staminaSpent = 0.1f;
        float potentialStamina = PlayerStats.Stamina - staminaSpent;

        if (potentialStamina < 0f) {
            PlayerStats.Stamina = 0f;
            return;
        }

        _flapStarted = true;
        PlayerStats.Stamina = potentialStamina;

        //float magnitude = body.velocity.y > 0 ? body.velocity.y : 0; 
        //float force = magnitude < 4f ? _flapForce : _flapForce / magnitude;
        float force = _flapForce;

        if (isPulling) {
            force *= 1.2f;
        }

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

        if (_turnStarted)
            onTurnFinished();
            

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
        body.AddForce(new Vector2(0f, force));
    }

    void EndJump() {
        _jumpStarted = false;
        anim.SetBool("IsJumping", false);
    }

    void InterruptFlyOrJump() {
        if (_flapStarted)
            endFlap();
        if (_jumpStarted)
            EndJump();
    }

    void checkGrounded()
    {

        Vector3 v1 = new Vector3(0, 1, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + v1, Vector2.down, 1.0f, groundLayer);
        
        bool gr = (hit.collider != null);

        if (gr != _isGrounded) {
            anim.SetBool("IsGrounded", gr);
            if (gr) {
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

        if (collider.gameObject.layer == LayerMask.NameToLayer("Wall")) {
            bool stillCoyoteTime = Input.GetButton("Grab") && Time.time - grabCoyoteTimeStarted < (COYOTE_TIME_SEC * 2);
            _touchingWall = collider;
            if (_isSideGrabbing || stillCoyoteTime)
                startHangOnWall();
            else
                Debug.Log("Collided with Wall");
        }

        if (collider.tag == "Hanger") {
            bool stillCoyoteTime = Input.GetButton("Grab") && Time.time - grabCoyoteTimeStarted < (COYOTE_TIME_SEC * 4);
            nearestHanger = collider.gameObject.transform;
            if (_isUpwardGrabbing || stillCoyoteTime)
                startHangOnCeiling();
            else
                Debug.Log("Collided with CEILING");
        }

        if (invulnerable == true)
            return;

        if (collider.tag == "Boulder") {
            //Debug.Log("Relative Velocity " + collision.relativeVelocity.magnitude);
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike")
        {
            hurt(Vector2.zero);
        }

        if (collider.tag == "Enemy") {
            NpcBehavior behavior = collider.gameObject.GetComponent<NpcBehavior>();
            if (!behavior.isDead)
                hurt(new Vector2(0, 1000f));
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        Collider2D collider = other.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("Wall")) {
            _touchingWall = null;
        }

        if (collider.tag == "Hanger") {
            //Debug.Log("trigger hang exit");
            nearestHanger = null;
        }
        
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "LevelPortal") {

            Debug.Log("WinWin");

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


        /*if (other.tag == "Hanger") {
            //Debug.Log("trigger hang enter");
            nearestHanger = other.gameObject.transform;
        }*/

        if (other.tag == "Grabbable") {
            //Debug.Log("trigger GRAB enter");
            activeGrabbable = other.gameObject.GetComponent<GrabbableBehavior>();
            if (activeGrabbable) {
                Debug.Log("Now has active grabbable");
                activeGrabbable.SetCanvasActive(true);
            }
            //nearestGrabbableTransform - other.gameObject.
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        /*if (other.tag == "Hanger") {
            //Debug.Log("trigger hang exit");
            nearestHanger = null;
        }*/

        if (other.tag == "Grabbable") {
            //Debug.Log("trigger GRAB exit");
            if (activeGrabbable) {
                activeGrabbable.SetCanvasActive(false);
                activeGrabbable = null;
            }
        }
    }

    void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {
        
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
        
        // preliminary stop turning
        if (_turnStarted) {
            onTurnFinished();
        }

        InterruptFlyOrJump();

        if (_isDashing)
            endDash();

        
    }

    void dieAndRespawn() {
        anim.SetBool("IsDying", false);
        PlayerStats.Deaths++;
        Debug.Log("Deaths: " + PlayerStats.Deaths);
        SceneManager.LoadScene("Level_2");
    }

    IEnumerator RestartAfterDelay() {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Level_2");
    }

    public void LoseAndRespawn() {
        PlayerStats.Losses++;
        Debug.Log("Losses: " + PlayerStats.Losses);
        StartCoroutine(RestartAfterDelay());
    }

    public void onSaveGame() {
        PlayerStats.HP = INITIAL_HP;
    }

}
