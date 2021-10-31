using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SimpleCallback();  

public class GrabbableBehavior : MonoBehaviour
{
    private Transform playerTransform;

    // Update is called once per frame
    private GameObject canvas = null;
    public bool captured = false;  
    private bool hasBodyParentFrame = false;

    private SimpleCallback getCapturedCallback;
    private SimpleCallback getReleasedCallback;

    private bool prev_grabbable = false;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;

        Transform t = transform.Find("Canvas");
        if (t) {
            canvas = t.gameObject;
        }

        hasBodyParentFrame = (transform.parent.Find("Body") != null);
    }
    void Update()
    {
        if (canvas) {
            
            bool showGrabText = CheckGrabability() && !captured;
            canvas.SetActive(showGrabText);

            /*if (showGrabText)
                Time.timeScale = 0.5f;
            else if (prev_grabbable)
                Time.timeScale = 1f;
            prev_grabbable = showGrabText;*/
        }

        if (hasBodyParentFrame) {
            Transform t = transform.parent.Find("Body");
            //Vector3 scale = t.localScale;
            transform.localPosition = t.localPosition;
        }
    }

    protected bool CheckGrabability() {
        Vector3 checkPosition = new Vector2(playerTransform.position.x, playerTransform.position.y-1.2f);
        Collider2D col = gameObject.GetComponentInParent<Collider2D>();
        //Debug.DrawLine(playerTransform.position, checkPosition, Color.green, 0.05f, false);

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
}