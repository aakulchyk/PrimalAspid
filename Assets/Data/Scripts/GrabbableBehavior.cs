using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SimpleCallback();  

public class GrabbableBehavior : MonoBehaviour
{
    private GameObject canvas = null;
    public bool captured = false;  
    private bool hasBodyParentFrame = false;

    private SimpleCallback getCapturedCallback = null;
    private SimpleCallback getReleasedCallback = null;

    void Start()
    {
        Transform t = transform.Find("Canvas");
        if (t) {
            canvas = t.gameObject;
        }

        SetCanvasActive(false);

        hasBodyParentFrame = (transform.parent.Find("Body") != null);
    }

    // Update is called once per frame
    void Update()
    {
       /* if (canvas) {
            bool showGrabText = CheckGrabability() && !captured;
            canvas.SetActive(showGrabText);
        }*/

        if (hasBodyParentFrame) {
            Transform t = transform.parent.Find("Body");
            transform.localPosition = t.localPosition;
        }
    }

    protected bool CheckGrabability() {
        Vector3 checkPosition = new Vector2(Utils.GetPlayerTransform().position.x, Utils.GetPlayerTransform().position.y-0.5f);
        Collider2D col = gameObject.GetComponentInParent<Collider2D>();

        if (col) {
            return col.OverlapPoint(checkPosition);
        }
        else {
            Transform t = transform.parent.Find("Body");
            if (t) {
                col = t.gameObject.GetComponentInParent<Collider2D>();
                return col && col.OverlapPoint(checkPosition);
            } else
                return false;
        }
    }

    public void SetCanvasActive(bool active) {
        if (canvas)
            canvas.SetActive(active);
        else
            Debug.Log("ERROR: No canvas!");
    }

    public void FlipCanvas() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }

    public void SetGetCapturedCallback(SimpleCallback callback) {
        getCapturedCallback = callback;
    }

    public void SetGetReleasedCallback(SimpleCallback callback) {
        getReleasedCallback = callback;
    }

    public virtual void getCaptured()
    {
        if (getCapturedCallback != null)
            getCapturedCallback();
        captured = true;
    }

    public virtual void getReleased()
    {
        if (getReleasedCallback != null)
            getReleasedCallback();
        captured = false;
    }

    public bool isGrounded()
    {
        Debug.Log("Grounded?");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground"));

        return (hit.collider != null);
    }
}
