using UnityEngine;
using System;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    //public float interpVelocity;
    //public float minDistance;
    //public float followDistance;
    private Transform target;
    public Vector3 offset;
    Vector3 targetPos;

    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject bottomBorder;
    public GameObject topBorder;

    public Vector2 frustum;

    private bool justLoaded;

    
    void Start()
    {
        frustum = FructumSizeAtDist(Math.Abs(transform.position.z));
    }

    /*private void OnLevelWasLoaded(int level)
    {
        justLoaded = true;
    }*/

    public void SetTarget(Transform val) {
        target = val;
        justLoaded = true;
    }
    // Update is called once per frame
    void Update () {
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.position.z;
            float oldX = posNoZ.x;
            float oldY = posNoZ.y;

            Vector3 targetDirection = (target.position - posNoZ);

            float interpVelocity = targetDirection.magnitude * 3.5f;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime); 

            if (targetPos.x - frustum.x/2 < leftBorder.transform.position.x) {
                targetPos.x = leftBorder.transform.position.x + frustum.x/2;
            }

            if (targetPos.x + frustum.x/2 > rightBorder.transform.position.x) {
                targetPos.x = rightBorder.transform.position.x - frustum.x/2;
            }
            
            if (targetPos.y + frustum.y/2 > topBorder.transform.position.y) {
                targetPos.y = topBorder.transform.position.y - frustum.y/2;
            }
            
            if (targetPos.y - frustum.y/2 < bottomBorder.transform.position.y) {
                targetPos.y = bottomBorder.transform.position.y + frustum.y/2;
            }

            // find lower border
            //RaycastHit2D hit = Physics2D.Raycast(target, Vector2.down, LayerMask.GetMask("Ground"));
            float bottomBorderY = bottomBorder.transform.position.y;

            /*if (hit.collider != null) {
                bottomBorderY = hit.collider.transform.position.y;
                Debug.Log("Camera: stick to the ground: " + posNoZ.y + " " + bottomBorderY);
            }*/

            
            /*if (targetPos.y < posNoZ.y && posNoZ.y - bottomBorderY < 8f) {
                targetPos.y = oldY;
            }*/

            if (!justLoaded) {
                transform.position = Vector3.Lerp( transform.position, targetPos + offset, 0.6f);
            } else {
                Debug.Log("set pos");
                transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
                justLoaded = false;
            }

        } else {
            target = Utils.GetPlayerTransform();
            
        }
    }



    private Vector2 FructumSizeAtDist(float distance)
    {
        var frustumHeight = 2.0f * distance * Mathf.Tan(GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * GetComponent<Camera>().aspect;
        return new Vector2(frustumWidth, frustumHeight);
    }


}


