using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    //public float interpVelocity;
    //public float minDistance;
    //public float followDistance;
    public Transform target;
    public Vector3 offset;
    Vector3 targetPos;

    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject bottomBorder;
    public GameObject topBorder;

    
    // Update is called once per frame
    void FixedUpdate () {
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.position.z;
            float oldX = posNoZ.x;
            float oldY = posNoZ.y;

            Vector3 targetDirection = (target.position - posNoZ);

            float interpVelocity = targetDirection.magnitude * 3.5f;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime); 


            if (targetPos.x < posNoZ.x && posNoZ.x - leftBorder.transform.position.x < 11f) {
                targetPos.x = oldX;
            }

            if (targetPos.x > posNoZ.x && rightBorder.transform.position.x - posNoZ.x < 11f) {
                targetPos.x = oldX;
            }

            if (targetPos.y > posNoZ.y && topBorder.transform.position.y - posNoZ.y < 11f) {
                targetPos.y = oldY;
            }

            // find lower border
            //RaycastHit2D hit = Physics2D.Raycast(target, Vector2.down, LayerMask.GetMask("Ground"));
            float bottomBorderY = bottomBorder.transform.position.y;

            /*if (hit.collider != null) {
                bottomBorderY = hit.collider.transform.position.y;
                Debug.Log("Camera: stick to the ground: " + posNoZ.y + " " + bottomBorderY);
            }*/

            
            if (targetPos.y < posNoZ.y && posNoZ.y - bottomBorderY < 8f) {
                targetPos.y = oldY;
            }

            transform.position = Vector3.Lerp( transform.position, targetPos + offset, 0.6f);

        } else {
            target = GameObject.FindWithTag("Player").transform;
        }
    }
}
