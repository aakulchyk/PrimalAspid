using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoolTextBehavior : MonoBehaviour
{
    Transform canvas;
    Vector3 initialPos;
    // Start is called before the first frame update
    float nextUpdateTime;
    float intervalInSec = 0.1f;
    void Start()
    {
        canvas = transform.Find("Canvas");
        initialPos = canvas.position;

        nextUpdateTime = Time.fixedTime + intervalInSec;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.fixedTime < nextUpdateTime)
            return;

        nextUpdateTime = Time.fixedTime + intervalInSec;
        Vector2 direction = Random.insideUnitCircle.normalized;

        float xdir = Random.Range(-0.04f, 0.04f);
        float ydir = Random.Range(-0.04f, 0.04f);

        Vector3 dir = new Vector3(xdir, ydir, 0f);


        canvas.position = initialPos + dir;
    }
}
