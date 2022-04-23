using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangerBehavior : MonoBehaviour
{
    public bool IsBodyAttached = false;
    public void AttachBody(GameObject go)
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
            joint.connectedAnchor = new Vector2(0f, +1f / coeff);
            joint.enabled = true;
            IsBodyAttached = true;
        } else {
            Debug.LogError("Hanger: Joint Not Found!");
        }
    }

    public void DetachBody()
    {
        var joint = GetComponent<FixedJoint2D>();
        if (joint) {
            joint.connectedBody = null;
            joint.enabled = false;
            IsBodyAttached = false;
        }
    }

    public Transform hangerTransform() 
    {
        return transform;
    }
}
