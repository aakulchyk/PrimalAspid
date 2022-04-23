using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoConnectedChainHangers : MonoBehaviour
{
    [SerializeField] private Transform leftChain;
    [SerializeField] private Transform rightChain;

    [SerializeField] private HangerBehavior leftHanger;
    [SerializeField] private HangerBehavior rightHanger;

    [SerializeField] private Transform highestPos;
    [SerializeField] private Transform lowestPos;
    // Start is called before the first frame update

    private float _chainHeight;
    void Start()
    {
        Debug.Log("low: " + lowestPos.position.y);
        Debug.Log("high: " + highestPos.position.y);

        Debug.Log("left: " + leftHanger.hangerTransform().position.y);
        Debug.Log("right: " + rightHanger.hangerTransform().position.y);

        _chainHeight = leftChain.position.y - leftHanger.hangerTransform().position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (leftHanger.IsBodyAttached) {
            if (leftHanger.hangerTransform().position.y > lowestPos.position.y) {
                float newYL = leftChain.position.y - 0.005f;//Mathf.Lerp(leftChain.position.y,  _chainHeight + lowestPos.position.y,  0.001f);
                float newYR = rightChain.position.y + 0.005f;//Mathf.Lerp(rightChain.position.y, _chainHeight + highestPos.position.y, 0.001f);
                leftChain.position  = new Vector3(leftChain.position.x,  newYL, 0);
                rightChain.position = new Vector3(rightChain.position.x, newYR, 0);
            }
        }

        if (rightHanger.IsBodyAttached) {
            if (rightHanger.hangerTransform().position.y > lowestPos.position.y) {
                float newYL = leftChain.position.y + 0.005f;//Mathf.Lerp(leftChain.position.y,  _chainHeight + highestPos.position.y, 0.001f);
                float newYR = rightChain.position.y - 0.005f;//Mathf.Lerp(rightChain.position.y, _chainHeight + lowestPos.position.y,  0.001f);
                leftChain.position  = new Vector3(leftChain.position.x,  newYL, 0);
                rightChain.position = new Vector3(rightChain.position.x, newYR, 0);
            }
        }
    }
}
