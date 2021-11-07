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

    private bool isGrounded = false;

    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public AudioClip clip_flap;

    public AudioClip clip_dash;

    public const int INITIAL_HP = 2;
    public const float FLAP_MIN_TIMEOUT = 0.4f;


    private System.DateTime startTime;
    private System.DateTime prevUpdateTime;


    private bool isDead;

    private float lastFlapTime;

    private bool _turnStarted = false;
    private bool _flapStarted = false;

    private bool _cannotGrab = false;
    
    private Game game;

    // TODO: rework
    public NpcBehavior activeSpeaker;
    public SavePointBehavior activeSavePoint;

    private Transform nearestHanger = null;

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

        game = (Game)FindObjectOfType(typeof(Game));
        game.LoadGame();
        game.isGameInProgress = true;

        PlayerStats.HP = INITIAL_HP;
        
        //flip();
        faceRight = true;
        
        StartCoroutine(blinkInvulnerable());

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        pHeight = col.size.y;
    }

    public void throwByImpulse(Vector2 vector, bool enemy = true) {
        Debug.Log("Throw player back " + vector);
        if (enemy) {
            anim.SetTrigger("Kill");
            if (PlayerStats.Stamina < 0.81f) {
                PlayerStats.Stamina += 0.2f;
            }
        }
        StartCoroutine(shortInvulnerability());
        body.velocity = vector;
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
        yield return new WaitForSeconds(0.5F);
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

        if (Time.timeScale == 0)
            return;

        if (_isDashing)
            return;

        checkGrounded();

        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKey(KeyCode.JoystickButton3)) {
            if (activeSpeaker && activeSpeaker.openForDialogue) {
                activeSpeaker.talkToPlayer();
            } 
            else 
            if (activeSavePoint && activeSavePoint.canInteract) {
                activeSavePoint.SaveGame();
            }
        }


        if ( (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0) )
                 && !_isHanging) {

            if (_turnStarted)
                onTurnFinished();
                
            if (Time.time - lastFlapTime > FLAP_MIN_TIMEOUT) {
                startFlap();
                lastFlapTime = Time.time;
            }
        }

        // KeyCode.JoystickButton4 - LB
        // KeyCode.JoystickButton5 - RB

        float d_axis = Input.GetAxisRaw("Dash");
        bool dash_triggered = d_axis>0.9f || d_axis < -0.9f;

        if (dash_axis_flag && !dash_triggered) {
            dash_axis_flag = false;
        }
        
        if (!dash_axis_flag && (Input.GetKeyDown(KeyCode.G) || dash_triggered) && !_isDashing && !isPulling && !_isHanging) {
            dash_axis_flag = true;

            startDash();
            return;
        }

        float moveX = Input.GetAxis ("Horizontal");
        body.velocity = new Vector2 (moveX * _maxSpeed, body.velocity.y);
        //Debug.Log("vel " + body.velocity.x);
        bool move = Math.Abs(body.velocity.x) > 0.1f;
        if (move) {
            if (move != _isMoving)
                anim.SetBool("IsMoving", true);
            _isMoving = true;
        } else {
            if (move != _isMoving)
                anim.SetBool("IsMoving", false);
            _isMoving = false;
        }

        bool newTurn = false;   
        if ((moveX > 0 && !faceRight) || (moveX < 0 && faceRight)) {
            newTurn = true;
        }

        if (!_turnStarted && newTurn) {
            if (_isHanging) {
               onTurnFinished();
            } else {
                anim.SetBool("Turn", true);
                endFlap();
                _turnStarted = true;
            }
        }
        
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton2)) && !Input.GetKey(KeyCode.UpArrow)) {
            anim.SetTrigger("Grab");
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.JoystickButton2))) {
            Debug.Log("Vertical Axis: " + Input.GetAxis ("Vertical"));
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetAxis ("Vertical") > 0.1f) {
                
                if (nearestHanger) {
                    // HANG!
                    //Debug.Log("HANG!");
                    endFlap();
                    anim.SetBool("IsHanging", true);
                    //RectTransform rt = nearestHanger.GetComponent<RectTransform>();
                    //float w = rt.sizeDelta.x * rt.localScale.x;
                    BoxCollider2D bc = nearestHanger.gameObject.GetComponent<BoxCollider2D>();
                    float w = bc.size.x;
                    transform.position = nearestHanger.position + new Vector3(w/2, -pHeight*1.5f, 0);
                    body.constraints |= RigidbodyConstraints2D.FreezePosition;
                    //PlayerStats.Stamina = 1f; // restore full stamina the same way as on the ground
                    _isHanging = true;
                }
            }

            // TODO rethink mechanic
            Vector3 checkPosition = new Vector3(transform.position.x, transform.position.y-0.6f, transform.position.z);
            Collider2D[] grabColliders = Physics2D.OverlapCircleAll(checkPosition, 0.6f);

            foreach (var grabCollider in grabColliders)
            {
                if (grabCollider.tag == "Player" || grabCollider.tag == "Spike")
                    continue;
                Rigidbody2D body = grabCollider.gameObject.GetComponent<Rigidbody2D>();

                if (body == null)
                    continue;

                if (!_cannotGrab)
                    grabBody(body);
                break;
            }
        }
        else
        {
            if (isPulling)
                throwByImpulse(new Vector2 (0, 15), false);
            
            if (_isHanging) {
                anim.SetBool("IsHanging", false);
                Debug.Log("no hang");
                _isHanging = false;
                body.constraints &= ~RigidbodyConstraints2D.FreezePosition;
            }

            releaseBody();
        }

        System.DateTime now = System.DateTime.UtcNow;
        System.TimeSpan diff = now-prevUpdateTime;
        
        if ((isGrounded || _isHanging)) {
            var delta = (0.002f * diff.Milliseconds);
            if (PlayerStats.Stamina < 1 - delta) {
                PlayerStats.Stamina += delta;
            }
        } else if (!isPulling) {
            var delta = (0.00003f * diff.Milliseconds);
            if (PlayerStats.Stamina < 1 - delta) {
                PlayerStats.Stamina += delta;
            }
        }
        

        prevUpdateTime = now;
    }

    public void grabBody(Rigidbody2D body) {

        /*NpcBehavior behavior = body.gameObject.GetComponent<NpcBehavior>();
        if (behavior) {
            if (!behavior.invulnerable) {
                behavior.getCaptured();
            } else return;
        }*/

        Transform t = body.transform.Find("Grabbable");

        // if it's a framed object
        if (!t)
            t = body.transform.parent.Find("Grabbable");

        if (t) {
            t.gameObject.GetComponent<GrabbableBehavior>().getCaptured();
            GetComponent<FixedJoint2D>().connectedBody = body;
            GetComponent<FixedJoint2D>().enabled = true;
            isPulling = true;
        }
    }

    public void releaseBody() {
        isPulling = false;

        Rigidbody2D body = GetComponent<FixedJoint2D>().connectedBody;

        if (body) {
            //NpcBehavior behavior = body.gameObject.GetComponent<NpcBehavior>();
            Transform t = body.transform.Find("Grabbable");

            if (!t)
                t = body.transform.parent.Find("Grabbable");

            if (t)
                t.gameObject.GetComponent<GrabbableBehavior>().getReleased();
            GetComponent<FixedJoint2D>().connectedBody = null;
        }

        GetComponent<FixedJoint2D>().enabled = false;

        StartCoroutine(shortInabilityToGrab());
    }
    
    public void onTurnFinished() {
        //Debug.Log("onTurnFinished");
        anim.SetBool("Turn", false);
        _turnStarted = false;
        faceRight = !faceRight;
        flip();
    }

    public void startDash() {
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("NPC"), true);
        StartCoroutine(shortInvulnerability());
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            PlayerStats.Stamina = 0f;
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        if (_turnStarted) {
            onTurnFinished();
        }
        if (_flapStarted) {
            endFlap();
        }

        GetComponent<AudioSource>().PlayOneShot(clip_dash);
        body.velocity = new Vector2(faceRight ? 80 : -80, 10);
        Debug.Log("Dash " + body.velocity.x);
        _isDashing = true;
        anim.SetTrigger("Dash");
    }

    public void endDash() {
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PC"), LayerMask.NameToLayer("NPC"), false);
        _isDashing = false;
    }

    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void startFlap() {

        float staminaSpent = isGrounded ? 0.0f : 0.1f;
        float potentialStamina = PlayerStats.Stamina -staminaSpent;

        /*if (isPulling) {
            potentialStamina -= 0.05f;
        }*/

        if (potentialStamina < 0f) {
                PlayerStats.Stamina = 0f;
                return;
            }

        PlayerStats.Stamina = potentialStamina;

        float magnitude = body.velocity.y > 0 ? body.velocity.y : 0; 
        float force = magnitude < 4f ? _flapForce : _flapForce / magnitude;

        if (isGrounded) {
            force *= 1.3f;
        } else if (isPulling) {
            force *= 1.1f;
        }

        GetComponent<AudioSource>().PlayOneShot(clip_flap);
        
        body.AddForce(new Vector2(0f, force));
        anim.SetBool("IsFlapping", true);
        _flapStarted = true;
    }

    public void endFlap() {
        anim.SetBool("IsFlapping", false);
        _flapStarted = false;
    }

    void checkGrounded()
    {
        if (isPulling || _isHanging) return;

        Vector3 v1 = new Vector3(0,1,0);
        //Debug.Log("Check Ground BITCH");
        RaycastHit2D hit = Physics2D.Raycast(transform.position + v1, Vector2.down, 1.5f, groundLayer);
        
        bool gr = (hit.collider != null);

        
        if (gr != isGrounded) {
            //Debug.Log("GROUND = " + gr);
            anim.SetBool("IsGrounded", gr);
        }
        isGrounded = gr;
        //Debug.DrawLine(transform.position + v1, transform.position + new Vector3(0,-0.5f,0), isGrounded ? Color.yellow : Color.blue, 0.1f, false);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;

        /*if (collider.tag == "Ground" && !isPulling)
        {
            isGrounded = true;
            anim.SetBool("IsGrounded", true);
            //PlayerStats.Stamina = 1f;
        } */       
        
        if (invulnerable == true)
            return;

        if (collider.tag == "Boulder") {
            //Debug.Log("Relative Velocity " + collision.relativeVelocity.magnitude);
            if (collision.relativeVelocity.magnitude > 8) {
                hurt(0);
            }
        }

        if (collider.tag == "Spike")
        {
            hurt(0);
        }

        if (collider.tag == "Enemy") {
            NpcBehavior behavior = collider.gameObject.GetComponent<NpcBehavior>();
            if (!behavior.isDead)
                hurt(2000);
        }

        /*if (collider.tag == "Hanger") {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.JoystickButton2))) {
                Debug.Log("HANG!");
                anim.SetBool("IsHanging", true);
                body.constraints |= RigidbodyConstraints2D.FreezePosition;
                PlayerStats.Stamina = 1f; // restore full stamina the same way as on the ground
                _isHanging = true;
            }
        }*/
    }

    void OnCollisionExit2D(Collision2D other) {
        Collider2D collider = other.collider;
        /*if (collider.tag == "Ground")
        {
            //Debug.Log("Leave ground");
            isGrounded = false;
            anim.SetBool("IsGrounded", false);
        }*/
    }

    void OnTriggerEnter2D(Collider2D other) {
        //if (invulnerable == true)
        //    return;


        if (other.tag == "LevelPortal") {
            //Debug.Log("Go to Level 2");
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

        if (other.tag == "CheckPoint") {
            Game game = (Game)FindObjectOfType(typeof(Game));
            game.SaveGame();
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


        if (other.tag == "Hanger") {
            Debug.Log("trigger hang enter");
            nearestHanger = other.gameObject.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Hanger") {
            Debug.Log("trigger hang exit");
            nearestHanger = null;
        }
    }

    void hurt(float force) {
        
        if (isDead) return; // one cannot die twice...

        body.AddForce(new Vector2(0f, force));
        
        if (--PlayerStats.HP < 0) {
            GetComponent<AudioSource>().PlayOneShot(clip_death);
            anim.SetBool("IsDying", true);
            isDead = true;
        } else {
            anim.SetTrigger("Hurt");
            GetComponent<AudioSource>().PlayOneShot(clip_hurt);
            StartCoroutine(blinkInvulnerable());
        }
        
        // preliminary stop turning
        if (_turnStarted) {
            //_turnStarted = false;
            //flip();
            onTurnFinished();
        }

        if (_flapStarted) {
            endFlap();
        }

        
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
        //PlayerStats.HP = PlayerStats.INITIAL_HP;
        Debug.Log("Losses: " + PlayerStats.Losses);
        StartCoroutine(RestartAfterDelay());
    }

}
