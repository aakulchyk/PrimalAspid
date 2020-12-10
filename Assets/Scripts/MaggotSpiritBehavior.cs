using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaggotSpiritBehavior : MonoBehaviour
{
    public float _moveSpeed;
    public float _followRadius;

    private Transform playerTransform;
    private Animator anim;
    private Rigidbody2D body; 

    private bool _isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();  
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDead)
            return;

        if (checkFollowRadius(playerTransform.position, transform.position))
        {
            anim.SetTrigger("Troubled");
            Debug.Log("Maggot Move");
            Vector3 direction = playerTransform.position - transform.position;
            //this.transform.position += new Vector3(_moveSpeed * Time.deltaTime, _moveSpeed * Time.deltaTime, 0f);
            this.transform.position += direction * _moveSpeed  * Time.deltaTime;
            //body.velocity = new Vector2 (direction.x * _moveSpeed  * Time.deltaTime, direction.y * _moveSpeed  * Time.deltaTime);
        }
        else {
            //body.velocity = new Vector2(0, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
	    if (other.tag == "Spike") {
            Debug.Log("Maggot Die");
	        anim.SetTrigger("Die");
            anim.SetBool("isDead", true);
            _isDead = true;
            //this.transform.tag = "Untagged";
            GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    //if player in radius move toward him 
    public bool checkFollowRadius(Vector2 playerPos, Vector2 enemyPos)
    {
        //double dist  = Math.Sqrt(playerPos.x-enemyPos.x)*(playerPos.x-enemyPos.x) + (playerPos.y-enemyPos.y)*(playerPos.y-enemyPos.y);
        return (Vector2.Distance(playerPos, enemyPos) < _followRadius);
    }
}
