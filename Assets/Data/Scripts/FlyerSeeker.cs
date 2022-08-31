using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu ("AggregatGames/Pathfinding/FlyerSeeker")]
/**
 *You are allowed to change this script in order to get the best out of your Pathfinder
 **/

public class FlyerSeeker : NpcBehavior {

	// mine
	public float _followRadius;
    public float moveSpeed;
	private bool faceRight = false;	
	private Vector2 _moveVector;
	private bool _chasingPlayer = false;
	public int MaxChaseTimeout = 125;
    private int _chaseTimeout;

	void Start () {
		
		BaseInit();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Hanger"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Background"), true);

        //_chaseTimeout = 0;
	}

	void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        /*if (_chaseTimeout>0)
            --_chaseTimeout;*/


        if (_knockback > 0) {
            _moveVector.x = body.velocity.x;
            --_knockback;
        } else {
            anim.SetBool("chase", true);
        }

        // do not fall
        body.velocity = _moveVector + new Vector2( 0, 0.86f );
    }

	void Update () {
		if (Time.timeScale == 0) {
            return;
        }
        
        if (isDead) {
            _moveVector = Vector2.zero;
            return;
        }


		Transform target = PlayerTransform();
		if (target == null) {
			_moveVector =  Vector2.zero;
            return;
		}
		
		float distP = Vector2.Distance(target.position, transform.position);

		bool noticed = distP < _followRadius;

		if (noticed) {
            _chaseTimeout = MaxChaseTimeout;
            _chasingPlayer = true;
        }

		EnsureRightDirection(target);

		if (_chaseTimeout > 0) {
            _chasingPlayer = true;
            //_moveVector = TryToReachTarget(pt);
        } else {
            _chasingPlayer = false;
			_moveVector =  Vector2.zero;
			return;
        }


		// FIND PATH

		//// move to target
		_moveVector = new Vector2(0, 0); // TBD

			
		
		if (!_chasingPlayer) 
			anim.SetBool("chase", false);


	}

	void EnsureRightDirection(Transform pt)
    {
        if (pt.position.x > transform.position.x && !faceRight) {
                flip();
                faceRight = true;
            } else if (pt.position.x < transform.position.x && faceRight) {
                flip();
                faceRight = false;
            }
    }

	public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {
        base.hurt(force, damageType, damage);
    }


   protected override void die()
   {
       base.die();
       Destroy(this.gameObject, 0.6f);
   }    
}
