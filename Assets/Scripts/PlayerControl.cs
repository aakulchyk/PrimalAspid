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
        //float magnitude = body.velocity.y;
        if (Input.GetKeyDown(KeyCode.Space) && anim.GetBool("IsFlapping") == false) {
        	body.AddForce(new Vector2(0f, 180f));
            anim.SetTrigger("Flap");
            anim.SetBool("IsFlapping", true);
        }
        
        float moveX = Input.GetAxis ("Horizontal");
        body.velocity = new Vector2 (moveX * maxSpeed, body.velocity.y);
        
        if (moveX > 0 && !faceRight) {
            anim.SetBool("Turn", true);
        }

        if ((moveX < 0 && faceRight)) {
            anim.SetBool("Turn", true);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }
    
    public void flip() {
        anim.SetBool("Turn", false);
        faceRight = !faceRight;
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
         Debug.Log("Flip:  called at: " + Time.time);
    }

    public void endFlap() {
        anim.SetBool("IsFlapping", false);
    }
    
    void OnTriggerEnter2D(Collider2D other){
	if (other.tag == "Spike")
	    SceneManager.LoadScene("MainScene");
    }
}
