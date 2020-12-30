using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBehavior : MonoBehaviour
{
    public GameObject target;
    Vector3 targetPos;
    private Vector3 originalPos;
    private Vector3 originalTargetPos;
    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        originalPos = transform.position;
        originalTargetPos = target.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target) {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.transform.position.z;
            float oldX = posNoZ.x;
            

            Vector3 targetDirection = (target.transform.position - posNoZ);

            float interpVelocity = targetDirection.magnitude * 1f;

            targetPos = originalPos + ((target.transform.position - originalTargetPos) / 1.25f);
            //transform.position + (targetDirection.normalized * interpVelocity * Time.deltaTime); 

            
            targetPos.x = oldX;
            
            transform.position = Vector3.Lerp( transform.position, targetPos, 0.25f);
        }
    }
}
