using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LizardBehavior : NpcBehavior
{

    public AudioClip clip_crawl;
    public AudioClip clip_dash;


    public float _moveSpeed;

    private bool faceRight = false;

    public float _aggravateRadius;

    private float _visibilityRadius = 50f;
    

    private float lastDashTime;
    public const float COOLDOWN_TIME = 1f;

    [SerializeField]
    private bool _isAnticipating = false;
    [SerializeField]
    private bool _isDashing = false;
    
    [SerializeField]
    private bool isGrounded = false;

    [SerializeField] private ParticleSystem jumpTrailParticles;
    [SerializeField] private ParticleSystem landParticles;

    // Start is called before the first frame update
    void Start()
    {
        BaseInit();
        lastDashTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) {
            return;
        }

        if (isDead)
            return;

        bool oldGr = isGrounded;
        isGrounded = CheckGrounded();

        if (oldGr != isGrounded )
            onGroundedChanged();

        bool pInRange = IsPlayerInRange();

        if (!_isDashing && !_isAnticipating && pInRange && isGrounded && _knockback<=0) {
            Debug.Log("Anticipate");
            _isAnticipating = true;
            anim.SetTrigger("dash");
        }

        float moveX = _isAnticipating ? 0f : faceRight ? _moveSpeed : -_moveSpeed;

        if (_knockback > 0) {
            moveX = body.velocity.x;
            --_knockback;
        }


        if (_isDashing || !isGrounded)
            moveX = body.velocity.x;

        // move left/right
        body.velocity = new Vector2( moveX, body.velocity.y);

        if (_knockback>0 || _isDashing || !isGrounded)
            return;

        if (Math.Abs(moveX) > 0.01f && !sounds.isPlaying) {
               sounds.PlayOneShot(clip_crawl);
        }

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
            Debug.DrawLine(pos+v0+v1, pos+v0+v1*2f, Color.yellow, 0.02f, false);
            if (hit1.collider) {
                ChangeDirection();
            }
        }
    }

    public override bool CheckGrounded()
    {
        Vector3 v1 = faceRight ? Vector3.right : Vector3.left;
        Vector3 pos = transform.position + Vector3.up + v1;
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.down, 1.2f, LayerMask.GetMask("Ground"));
        Debug.DrawLine(pos, pos+Vector3.down*2.4f, Color.black, 0.02f, false);

        return hit.collider != null;
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
        RaycastHit2D hit = Physics2D.Raycast(pos, v1, _aggravateRadius, LayerMask.GetMask("PC") | LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall") | LayerMask.GetMask("DeadlyDanger"));
        Debug.DrawLine(pos, pos+v1*_aggravateRadius, Color.blue, 0.02f, false);

        return (hit.collider != null && hit.collider.tag == "Player");
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

    void onAnticipationFinished()
    {
        _isAnticipating = false;
        dash();
    }

    void dash()
    {
        _isDashing = true;
        //isGrounded = false;

        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(faceRight ? 50 : -50, 13);

        StartCoroutine(ShowJumpTrail());
    }

    IEnumerator ShowJumpTrail() {
        jumpTrailParticles.Play();
        yield return new WaitForSeconds(0.2F);
        jumpTrailParticles.Stop();
    }


    void onGroundedChanged()
    {
        anim.SetBool("isGrounded", isGrounded);
        if (isGrounded) {
            jumpTrailParticles.Stop();
            _isDashing = false;
            landParticles.Emit(10);
        }
    }

    protected override void processCollision(Collision2D hit)
    {
    /*    Collider2D collider = hit.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            _isDashing = false;
        }
*/
        base.processCollision(hit);
    }

    
    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {
        if (isDead) return;

        _isAnticipating = false;
        
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
