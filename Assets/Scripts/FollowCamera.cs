using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public float interpVelocity;
    public float minDistance;
    public float followDistance;
    public GameObject target;
    public Vector3 offset;
    Vector3 targetPos;
    // Use this for initialization
    void Start () {
        targetPos = transform.position;
    }
    
    // Update is called once per frame
    void FixedUpdate () {
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.transform.position.z;
            float oldY = posNoZ.y;
            Vector3 targetDirection = (target.transform.position - posNoZ);

            interpVelocity = targetDirection.magnitude * 3f;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime); 
            targetPos.y = oldY;
            transform.position = Vector3.Lerp( transform.position, targetPos + offset, 0.25f);

        }
    }
}
