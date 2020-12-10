using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    private bool faceRight = false;
    private float maxSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
	    body = GetComponent<Rigidbody2D> ();  
        anim = GetComponent<Animator>();      
    }

    // Update is called once per frame
    void Update()
    {
        bool isAlreadyTurning = anim.GetCurrentAnimatorStateInfo(0).IsName("AspidTurn");
        bool isAlreadyFlapping = anim.GetCurrentAnimatorStateInfo(0).IsName("AspidFlap");

        //float magnitude = body.velocity.y; 
        if (Input.GetKeyDown(KeyCode.Space) && !isAlreadyFlapping && !isAlreadyTurning) {
        	body.AddForce(new Vector2(0f, 180f));
            anim.SetBool("IsFlapping", true);
            anim.SetTrigger("Flap");
        }
        
        float moveX = Input.GetAxis ("Horizontal");
        body.velocity = new Vector2 (moveX * maxSpeed, body.velocity.y);
        

        
        bool newTurnStarted = false;
        if (moveX > 0 && !faceRight) {
            newTurnStarted = true;
            Debug.Log("Start turn right");
            faceRight = !faceRight;
        }

        if (moveX < 0 && faceRight) {
            newTurnStarted = true;
            Debug.Log("Start turn left");
            faceRight = !faceRight;
            
        }

        if (!isAlreadyTurning && newTurnStarted) {
            anim.SetBool("Turn", true);
        }
        if (isAlreadyTurning && newTurnStarted) {
            flip();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }
    
    public void onTurnFinished() {
        Debug.Log("onTurnFinished");
        anim.SetBool("Turn", false);
        flip();
    }

    public void flip() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void endFlap() {
        anim.SetBool("IsFlapping", false);
    }
    
    void OnTriggerEnter2D(Collider2D other) {
	    if (other.tag == "Spike" || other.tag == "Enemy")
	        SceneManager.LoadScene("MainScene");
    }
}
