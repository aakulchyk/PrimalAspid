using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D body;
    private bool faceRight = false;
    private float maxSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
	body = GetComponent<Rigidbody2D> ();        
    }

    // Update is called once per frame
    void Update()
    {
        float magnitude = body.velocity.y;
        if (Input.GetKeyDown(KeyCode.Space) && magnitude < 2.5) {
        	body.AddForce(new Vector2(0f, 180f));
        }
        
        float moveX = Input.GetAxis ("Horizontal");
        body.velocity = new Vector2 (moveX * maxSpeed, body.velocity.y);
        
        if ((moveX > 0 && !faceRight) || (moveX < 0 && faceRight)) {
            flip();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }
    
    void flip() {
        faceRight = !faceRight;
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }
    
    void OnTriggerEnter2D(Collider2D other){
	if (other.tag == "Spike")
	    SceneManager.LoadScene("MainScene");
	    //Application.LoadLevel (Application.loadedLevel);
    }
}
