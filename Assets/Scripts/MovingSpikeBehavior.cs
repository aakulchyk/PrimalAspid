using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpikeBehavior : MonoBehaviour
{
    public bool _fromUpside;
    public float _followRadius;
    public bool switchedOff = false;

    private Transform playerTransform;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchedOff) {
            anim.SetBool("NearPlayer", false);
            return;
        }
        if (_followRadius==0 || checkFollowRadius(playerTransform.position, transform.position))
        {
            anim.SetBool("NearPlayer", true);
        }
        else
            anim.SetBool("NearPlayer", false);
    }

        //if player in radius move toward him 
    public bool checkFollowRadius(Vector2 playerPos, Vector2 enemyPos)
    {
        //double dist  = Math.Sqrt(playerPos.x-enemyPos.x)*(playerPos.x-enemyPos.x) + (playerPos.y-enemyPos.y)*(playerPos.y-enemyPos.y);
        return (Vector2.Distance(playerPos, enemyPos) < _followRadius);
    }
}
