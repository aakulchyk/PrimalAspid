using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyWallBehavior : MonoBehaviour
{
    public void AttachBody(GameObject go, bool fromLeft)
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        if (bc == null) {
            Debug.LogError("Hanger: BoxCollider2d Not Found!");
            return;
        }


        var rb = go.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        
        var joint = GetComponent<FixedJoint2D>();

        if (joint) {
            joint.connectedBody = rb;
            float coeff = go.transform.localScale.y;
            //joint.anchor = fromLeft ? Vector2.left : Vector2.right * 5;//  fromLeft ? -5 : 5;

            float y = (go.transform.position.y - transform.position.y) / transform.localScale.y;
            joint.anchor = new Vector2( fromLeft ? -5 : 5, y);
            joint.connectedAnchor = new Vector2(0f, (fromLeft ? +1f : -1f) / coeff);
            joint.enabled = true;
        } else {
            Debug.LogError("Hanger: Joint Not Found!");
        }
    }

    public void MoveBody(float moveY)
    {
        var joint = GetComponent<FixedJoint2D>();

        //Debug.Log("Joint: " + joint.anchor.y + " size: " + GetComponent<Collider2D>().bounds.size.y);

        float anchor = joint.anchor.y * transform.localScale.y;
        if (moveY < 0 && (anchor + moveY) < -2)
            return;

        if (moveY > 0 && (anchor + moveY) > GetComponent<Collider2D>().bounds.size.y - 3)
            return;
        
        joint.anchor += new Vector2(0, moveY);
    }

    public void DetachBody()
    {
        var joint = GetComponent<FixedJoint2D>();
        if (joint) {
            joint.connectedBody = null;
            joint.enabled = false;
        }
    }
}
