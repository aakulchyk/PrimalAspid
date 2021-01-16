using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    private Renderer _renderer;
    private bool faceRight = true;
    public float _maxSpeed = 5;
    public float _flapForce;
    //private Time startTime;
    private bool invulnerable;

    public bool isLeading = false;
    public AudioClip clip_death;
    public AudioClip clip_flap;


    private System.DateTime startTime;

    private int _lives = 2;

    // Start is called before the first frame update
    void Start()
    {
        startTime = System.DateTime.UtcNow;

	    body = GetComponent<Rigidbody2D> ();  
        anim = GetComponent<Animator>();      
        _renderer = GetComponent<Renderer>();
        
        StartCoroutine(blinkInvulnerable());
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

    // Update is called once per frame
    void Update()
    {
        bool isAlreadyTurning = anim.GetCurrentAnimatorStateInfo(0).IsName("AspidTurn");
        bool isAlreadyFlapping = anim.GetCurrentAnimatorStateInfo(0).IsName("AspidFlap");

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
                && !isAlreadyFlapping && !isAlreadyTurning) {
            startFlap();
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

        if (!isAlreadyTurning && newTurnStarted) {
            anim.SetBool("Turn", true);
        }
        if (isAlreadyTurning && newTurnStarted) {
            flip();
        }

        if ((Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.JoystickButton5)))
        {
            
            Vector3 checkPosition = new Vector3(transform.position.x, transform.position.y-0.5f, transform.position.z);
            Collider2D[] grabColliders = Physics2D.OverlapCircleAll(checkPosition, 0.5f);
            foreach (var grabCollider in grabColliders)
            {
                
                if (grabCollider.tag == "Player")
                    continue;
                Rigidbody2D body = grabCollider.gameObject.GetComponent<Rigidbody2D>();

                if (body == null)
                    continue;

                StupidEnemyBehavior behavior = grabCollider.gameObject.GetComponent<StupidEnemyBehavior>();
                if (behavior) {
                    behavior.captured = true;
                }
                
                GetComponent<FixedJoint2D>().connectedBody = body;
                GetComponent<FixedJoint2D>().enabled = true;
                isLeading = true;//!isLeading;
                break;
            }
        }
        else {
            isLeading = false;
            GetComponent<FixedJoint2D>().enabled = false;
            GetComponent<FixedJoint2D>().connectedBody = null;
        }

        if (!isLeading && PlayerStats.Stamina < 1f) {
             PlayerStats.Stamina += 0.001f;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

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

        if (isLeading) {
            
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

        if (collider.tag == "Spike" || collider.tag == "Enemy")
        {
            hurt(0);
        }

        if (collider.tag == "Ground" && !isLeading/*GetComponent<FixedJoint2D>().enabled*/)
        {
            PlayerStats.Stamina = 1f;
            /*if (PlayerStats.Stamina < 1f)
                PlayerStats.Stamina += 0.05f;*/
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

	    /*if (other.tag == "Spike" || other.tag == "Enemy")
        {
            body.velocity = new Vector2 (0, 0);

            MovingSpikeBehavior cs = other.GetComponent<MovingSpikeBehavior>();
            float force = 90f;
            if (cs !=null) {
                if (cs._fromUpside) force = -90f;
            }
            
            hurt(force);
        }*/

        if (other.tag == "Lever") {
            LeverBehavior script = other.gameObject.GetComponent<LeverBehavior>();
            if (!script || script.toggled)
                return;
            
            script.SwitchLever();
        }       
    }

    void hurt(float force) {
        body.AddForce(new Vector2(0f, force));
        GetComponent<AudioSource>().PlayOneShot(clip_death);
        if (--_lives < 0) {
            anim.SetBool("IsDying", true);
        } else {
            anim.SetTrigger("Hurt");
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
        Debug.Log("Losses: " + PlayerStats.Losses);
        StartCoroutine(RestartAfterDelay());
    }

}
