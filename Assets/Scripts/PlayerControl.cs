using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public Rigidbody2D body;
    private Animator anim;
    private Renderer _renderer;
    private bool faceRight = true;
    public float _maxSpeed = 5;
    public float _flapForce;
    //private Time startTime;
    private bool invulnerable;

    public bool isPulling = false;
    public AudioClip clip_hurt;
    public AudioClip clip_death;
    public AudioClip clip_flap;

    public const int MAX_HP = 2;


    private System.DateTime startTime;

    private bool prev_flap = false;
    

    // Start is called before the first frame update
    void Start()
    {
        startTime = System.DateTime.UtcNow;

	    body = GetComponent<Rigidbody2D> ();  
        anim = GetComponent<Animator>();      
        _renderer = GetComponent<Renderer>();

        PlayerStats.HP = MAX_HP;

        Game game = (Game)FindObjectOfType(typeof(Game));
        game.LoadGame();
        

        StartCoroutine(blinkInvulnerable());
    }

    public void throwByImpulse(Vector2 vector) {
        Debug.Log("Throw player back " + vector);
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




    // Update is called once per frame
    void Update()
    {
        bool isAlreadyTurning = anim.GetCurrentAnimatorStateInfo(0).IsName("GrimTurn");
        bool isAlreadyFlapping = anim.GetCurrentAnimatorStateInfo(0).IsName("GrimFlap");

        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0))
                && !isAlreadyFlapping && !isAlreadyTurning) {

            if (!prev_flap) {
                startFlap();
                prev_flap = true;
            } else prev_flap = false;
        }
        
        float moveX = Input.GetAxis ("Horizontal");
        //Debug.Log("moveX: " + moveX);
        body.velocity = new Vector2 (moveX * _maxSpeed, body.velocity.y);
        
        bool newTurnStarted = false;
        if (moveX > 0 && !faceRight) {
            newTurnStarted = true;
            faceRight = !faceRight;
        }

        if (moveX < 0 && faceRight) {
            newTurnStarted = true;
            faceRight = !faceRight;
        }

        /*bool isGrounded = anim.GetCurrentAnimatorStateInfo(0).IsName("IsGrounded");
        if (isGrounded)
            isAlreadyTurning = true;*/

        if (!isAlreadyTurning && newTurnStarted) {
            anim.SetBool("Turn", true);
        }
        if (isAlreadyTurning && newTurnStarted) {
            flip();
        }

        if ((Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.JoystickButton5))) {
            Vector3 checkPosition = new Vector3(transform.position.x, transform.position.y-0.5f, transform.position.z);
            Collider2D[] grabColliders = Physics2D.OverlapCircleAll(checkPosition, 0.5f);
            foreach (var grabCollider in grabColliders)
            {
                if (grabCollider.tag == "Player" || grabCollider.tag == "Spike")
                    continue;
                Rigidbody2D body = grabCollider.gameObject.GetComponent<Rigidbody2D>();

                if (body == null)
                    continue;

                grabBody(body);
                break;
            }
        }
        else
        {
            releaseBody();
        }

        if (!isPulling && PlayerStats.Stamina < 1f) {
             PlayerStats.Stamina += 0.001f;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void grabBody(Rigidbody2D body) {

        NpcBehavior behavior = body.gameObject.GetComponent<NpcBehavior>();
        if (behavior) {
            if (!behavior.invulnerable) {
                behavior.getCaptured();
            } else return;
        }

        GetComponent<FixedJoint2D>().connectedBody = body;
        GetComponent<FixedJoint2D>().enabled = true;
        isPulling = true;
    }

    public void releaseBody() {
        isPulling = false;

        Rigidbody2D body = GetComponent<FixedJoint2D>().connectedBody;

        if (body && body.gameObject) {
            NpcBehavior behavior = body.gameObject.GetComponent<NpcBehavior>();

            if (behavior)
                behavior.getReleased();
            GetComponent<FixedJoint2D>().connectedBody = null;
        }

        GetComponent<FixedJoint2D>().enabled = false;
        
       
    }
    
    public void onTurnFinished() {
        //Debug.Log("onTurnFinished");
        anim.SetBool("Turn", false);
        flip();
    }

    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void startFlap() {

        float potentialStamina = PlayerStats.Stamina - 0.05f;

        if (isPulling) {
            
            if (potentialStamina < 0f) {
                PlayerStats.Stamina = 0f;
                return;
            }

            PlayerStats.Stamina = potentialStamina;
        }

        float magnitude = body.velocity.y > 0 ? body.velocity.y : 0; 
        float force = magnitude < 2f ? _flapForce : _flapForce / magnitude;
        //Debug.Log("Magnitude = " + magnitude);
        GetComponent<AudioSource>().PlayOneShot(clip_flap);
        
        body.AddForce(new Vector2(0f, force));
        anim.SetBool("IsFlapping", true);
        anim.SetBool("IsGrounded", false);
    }

    public void endFlap() {
        anim.SetBool("IsFlapping", false);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (invulnerable == true)
            return;

        Collider2D collider = collision.collider;

        if (collider.tag == "Throwable") {
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
                hurt(0);
        }

        

        if (collider.tag == "Ground" && !isPulling/*GetComponent<FixedJoint2D>().enabled*/)
        {
            anim.SetBool("IsGrounded", true);
            PlayerStats.Stamina = 1f;
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        Collider2D collider = other.collider;
        if (collider.tag == "Ground")
        {
            //Debug.Log("Leave ground");
            anim.SetBool("IsGrounded", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (invulnerable == true)
            return;


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
            PlayerStats.HP++;
            other.gameObject.GetComponent<PrizeBehavior>().GetCollected();
        }

        if (other.tag == "CheckPoint") {
            Game game = (Game)FindObjectOfType(typeof(Game));
            game.SaveGame();
        }
    }

    void hurt(float force) {
        body.AddForce(new Vector2(0f, force));
        
        if (--PlayerStats.HP < 0) {
            GetComponent<AudioSource>().PlayOneShot(clip_death);
            anim.SetBool("IsDying", true);
        } else {
            anim.SetTrigger("Hurt");
            GetComponent<AudioSource>().PlayOneShot(clip_hurt);
            StartCoroutine(blinkInvulnerable());
        }
        
        
    }

    void dieAndRespawn() {
        anim.SetBool("IsDying", false);
        PlayerStats.Deaths++;
        Debug.Log("Deaths: " + PlayerStats.Deaths);
        SceneManager.LoadScene("MainScene");
    }

    IEnumerator RestartAfterDelay() {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("MainScene");
    }

    public void LoseAndRespawn() {
        PlayerStats.Losses++;
        //PlayerStats.HP = PlayerStats.MAX_HP;
        Debug.Log("Losses: " + PlayerStats.Losses);
        StartCoroutine(RestartAfterDelay());
    }

}
