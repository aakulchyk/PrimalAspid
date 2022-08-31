using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WalkerSpearThrower : NpcBehavior
{

    public AudioClip clip_shoot;
    //public AudioClip clip_dash;


    public float _moveSpeed;

    private bool faceRight = false;

    public float _aggravateRadius;

    private float _visibilityRadius = 50f;
    

    private float lastShootTime;
    public const float COOLDOWN_TIME = 1f;

    [SerializeField]
    private bool _isAnticipating = false;
    [SerializeField]
    private bool _isShooting = false;
    
    [SerializeField]
    private bool isGrounded = false;

    [SerializeField]
    private GameObject projectilePrefab;     // the prefab of our bullet
    ThrowableSpear currentProjectile;

    [SerializeField] private Transform projectileSpawnPoint;

    private Vector3 initialVectorToPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        lastShootTime = Time.time;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Hanger"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Background"), true);
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        if (isDead)
            return;

        /*bool oldGr = isGrounded;
        isGrounded = CheckGrounded();

        if (oldGr != isGrounded )
            onGroundedChanged();*/

        bool pInRange = IsPlayerInRange();

        if (!_isShooting && !_isAnticipating && pInRange && _knockback<=0) {
            //Debug.Log("Anticipate");
            _isAnticipating = true;
            anim.SetTrigger("shoot");
            initialVectorToPlayer = GetVectorToPlayer();
        }

        float moveX = _isAnticipating || _isShooting ? 0f : faceRight ? _moveSpeed : -_moveSpeed;

        if (_knockback > 0) {
            moveX = body.velocity.x;
            --_knockback;
        }

        /*if (!isGrounded)
            moveX = body.velocity.x;*/

        // move left/right
        body.velocity = new Vector2( moveX, body.velocity.y);

        if (_knockback>0 || _isAnticipating || _isShooting)
            return;

        /*if (Math.Abs(moveX) > 0.01f && !sounds.isPlaying) {
               sounds.PlayOneShot(clip_crawl);
        }*/

        // check ground ahead
        Vector3 v0 = Vector3.up * 0.5f;
        Vector3 v1 = faceRight ? Vector3.right : Vector3.left;
        Vector3 pos = transform.position + v1;
        RaycastHit2D hit = Physics2D.Raycast(pos+v0+v1, Vector2.down, 1.5f, LayerMask.GetMask("Ground"));
        Debug.DrawLine(pos+v0+v1, pos+v0+v1+Vector3.down*1.5f, Color.red, 0.02f, false);
        if (!hit.collider) {
            ChangeDirection();
        } else {
            RaycastHit2D hit1 = Physics2D.Raycast(pos+v0+v1, v1, 2f, LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall"));
            Debug.DrawLine(pos+v0+v1, pos+v0+v1*3f, Color.yellow, 0.02f, false);
            if (hit1.collider) {
                ChangeDirection();
            }
        }
    }

    void ChangeDirection()
    {
        faceRight = !faceRight;
        flip();
    }

    bool IsPlayerInRange()
    {
        Transform pt = Utils.GetPlayerTransform();
        if (pt == null)
            return false;

        Vector3 v1 = faceRight ? Vector3.right : Vector3.left;
        Vector3 pos = transform.position + Vector3.up;
        RaycastHit2D hit = Physics2D.Raycast(pos, v1, _aggravateRadius, LayerMask.GetMask("PC"));
        Debug.DrawLine(pos, pos+v1*_aggravateRadius, Color.yellow, 0.02f, false);

        return hit.collider != null;
    }

    bool IsVisibleToPlayer()
    {
        Transform pt = Utils.GetPlayerTransform();
        if (pt == null)
            return false;
        float distP = Vector2.Distance(pt.position, transform.position);
        return distP < _visibilityRadius;
    }

    /*void PlayDashSound()
    {
       sounds.PlayOneShot(clip_dash);
    }*/

    public void OnAnticipationFinished()
    {
        _isAnticipating = false;
        shoot();
    }

    

    void shoot()
    {
        //Debug.Log("Shoot");
        _isShooting = true;
        sounds.PlayOneShot(clip_shoot);
        
        GameObject go = Instantiate(projectilePrefab);

        go.transform.position = projectileSpawnPoint.position;
        go.transform.rotation = Quaternion.identity;
        //go.transform.localScale = new Vector3(5, 5, 1);
        
        currentProjectile = go.GetComponent<ThrowableSpear>();
        Vector3 v = (GetVectorToPlayer() + initialVectorToPlayer).normalized;
        float angle = currentProjectile.SignedAngleBetween(v, initialVectorToPlayer /*faceRight? Vector2.right : Vector2.left*/,  transform.forward);
        Debug.Log("angle = " + angle);
        const float maxAngle = 20f;
        if (angle > maxAngle) {
            v = Quaternion.AngleAxis(maxAngle, Vector3.back) * initialVectorToPlayer;
            angle = maxAngle;
        }
        else if (angle <-maxAngle) {
            v = Quaternion.AngleAxis(-maxAngle, Vector3.back) * initialVectorToPlayer;
            angle = -maxAngle;
        }

        //v = (GetVectorToPlayer() + initialVectorToPlayer).normalized;

        if (faceRight) {
            Vector3 scale = go.transform.localScale;
            go.transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
        }
        //Debug.Log("angle betwwen " + GetVectorToPlayer() + " and " + Vector3.left + " = " + angle);
        //go.transform.rotation.Set(0,0,angle,1);
        go.transform.Rotate(0,0,-angle);
        currentProjectile.setImpulse(v + Vector3.up*0.05f);

    }

    public void OnShootFinished()
    {
        _isShooting = false;
    }

    void onGroundedChanged()
    {
        if (isGrounded) {
            //_isDashing = false;
            //anim.SetBool("isDashing", false);
        }
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {
        if (isDead) return;

        _isAnticipating = false;
        _isShooting = false;
        
        if (damageType == Types.DamageType.Spikes) {
            _hp = -1;
        }

        base.hurt(force, damageType, damage);
    }
   protected override void die()
   {
        base.die();
        Destroy(this.gameObject, 1f);
   }
}
