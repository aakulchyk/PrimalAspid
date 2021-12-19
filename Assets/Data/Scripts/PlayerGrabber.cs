using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System;
using System.Text;
using System.Collections;

//public delegate void SimpleCallback();

public partial class PlayerControl : MonoBehaviour
{
    private bool _isHanging = false;
    private bool _isHangingOnWall = false;
    private bool _isSideGrabbing = false;
    private bool _isDownwardGrabbing = false;
    private bool _isUpwardGrabbing = false;


    private int _VerticalGrabVelocity = 20;
    private int _HorizontalGrabVelocity = 20;

    private float grabCoyoteTimeStarted;

    private Transform nearestHanger = null;
    private Collider2D _touchingWall = null;

    private bool IsGrabbing()
    {
        return _isSideGrabbing || _isUpwardGrabbing || _isDownwardGrabbing;
    }

    private bool IsHanging()
    {
        return _isHanging || _isHangingOnWall;
    }


    public void startSideGrab()
    {
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }

        PlayerStats.Stamina = potentialStamina;

        if (_turnStarted) {
            onTurnFinished();
        }
        
        InterruptFlyOrJump();

        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(faceRight ? _HorizontalGrabVelocity : -_HorizontalGrabVelocity, 0);
        body.constraints |= RigidbodyConstraints2D.FreezePositionY;

        Debug.Log("SideGrab " + body.velocity.x);
        _isSideGrabbing = true;
        anim.SetTrigger("SideGrab");

        if (_touchingWall)
            startHangOnWall();
    }

    public void endSideGrab()
    {
        Debug.Log("End SideGrab ");
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        body.velocity = new Vector2(0, body.velocity.y);
        _isSideGrabbing = false;
        grabCoyoteTimeStarted = Time.time;
    }

    public void startHangOnWall()
    {
        Debug.Log("Start Hang");
        InterruptFlyOrJump();
        _isSideGrabbing = false;
        _isHangingOnWall = true;
        if (sounds.isPlaying)
            sounds.Stop();
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHangingOnWall", true);
        body.velocity = Vector2.zero;
    }

    public void endHangOnWall()
    {
        Debug.Log("End Hang");
        _isHangingOnWall = false;
        body.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        anim.SetBool("IsHangingOnWall", false);
    }

    public void startUpwardGrab()
    {
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }
        PlayerStats.Stamina = potentialStamina;
        if (_turnStarted) {
            onTurnFinished();
        }
        InterruptFlyOrJump();
        if (sounds.isPlaying) {
            sounds.Stop();
        }
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(0, _VerticalGrabVelocity);
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        anim.SetTrigger("Grab");
        grabCoyoteTimeStarted = Time.time;
        _isUpwardGrabbing = true;
    }

    
    public void endUpwardGrab()
    {
        Debug.Log("End UpGrab ");
        body.gravityScale = _gravity;
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        _isUpwardGrabbing = false;
    }

    public void startHangOnCeiling()
    {
        endUpwardGrab();
        InterruptFlyOrJump();
        if (sounds.isPlaying)
            sounds.Stop();    
        sounds.PlayOneShot(clip_clutch);
        anim.SetBool("IsHanging", true);

        //BoxCollider2D bc = nearestHanger.gameObject.GetComponent<BoxCollider2D>();
        //float w = bc.size.x;
        //transform.position = nearestHanger.position + new Vector3(w/2, -pHeight*1.5f, 0);
        body.constraints |= RigidbodyConstraints2D.FreezePosition;

        _isHanging = true;
    }

    public void endHangOnCeiling()
    {
        anim.SetBool("IsHanging", false);
        _isHanging = false;
        body.constraints &= ~RigidbodyConstraints2D.FreezePosition;
    }

    public void startDownwardGrab()
    {
        float potentialStamina = PlayerStats.Stamina - 0.25f;
        if (potentialStamina < 0f) {
            return;
        }
        PlayerStats.Stamina = potentialStamina;
        if (_turnStarted) {
            onTurnFinished();
        }
        InterruptFlyOrJump();
        if (sounds.isPlaying) {
            sounds.Stop();
        }
        sounds.PlayOneShot(clip_dash);
        body.velocity = new Vector2(0, -_VerticalGrabVelocity);
        body.gravityScale = 0;
        body.constraints |= RigidbodyConstraints2D.FreezePositionX;

        anim.SetTrigger("GrabDown");
        grabCoyoteTimeStarted = Time.time;
        _isDownwardGrabbing = true;
    }

    
    public void endDownwardGrab()
    {
        Debug.Log("End DownGrab ");
        body.gravityScale = _gravity;
        body.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        _isDownwardGrabbing = false;
    }
}
