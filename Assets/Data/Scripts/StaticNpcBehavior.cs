using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticNpcBehavior : NpcBehavior
{
    private bool faceRight = false;

    public string[] initialTexts;
    public string[] randomTexts;

    private bool _firstEncounder;

    void Start()
    {
        BaseInit();
        interactable.currentTexts = initialTexts;
        _firstEncounder = true;
    }
    
    void Update()
    {
        Transform target = PlayerTransform();
        EnsureRightDirection(target); 
    }

    void EnsureRightDirection(Transform pt)
    {
        if (pt.position.x > transform.position.x && !faceRight) {
            flip();
            faceRight = true;
        } else if (pt.position.x < transform.position.x && faceRight) {
            flip();
            faceRight = false;
        }
    }

    public override void onTalk()
    {
        if (_firstEncounder) {
            interactable.currentTexts = initialTexts;
            _firstEncounder = false;
        } else {
            int currIndex = UnityEngine.Random.Range(0, randomTexts.Length);
            interactable.currentTexts = new string[1] { randomTexts[currIndex] };
        }
    }
}