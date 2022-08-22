
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldBatWitchBehavior : StaticNpcBehavior
{
    public Door _doorToOpen;
    
    [Header ("Texts")]
    public string[] randomTexts_beforeCompleted;
    public string[] completedTexts;

    protected bool _questAssigned = false;
    protected bool _questCompleted = false;



    void Start()
    {
        BaseInit();
        interactable.currentTexts = initialTexts;
    }
    
    public override void onTalk()
    {
        if (false == _questAssigned) {
            interactable.currentTexts = initialTexts;
            _questAssigned = true;
        } else {
            if (_questCompleted) {
                int currIndex = UnityEngine.Random.Range(0, randomTexts.Length);
                interactable.currentTexts = new string[1] { randomTexts[currIndex] };
            } else {
                // check for quest completion now
                if (PlayerStats.ObtainedSomeImportantShit_changeMyName) {
                    PlayerStats.ObtainedSomeImportantShit_changeMyName = false;
                    _questCompleted = true;
                    interactable.currentTexts = completedTexts;
                    //
                    // TODO: trigger door / give reward
                    //
                    if (_doorToOpen)
                        _doorToOpen.Open();
                } else {
                    int currIndex = UnityEngine.Random.Range(0, randomTexts_beforeCompleted.Length);
                    interactable.currentTexts = new string[1] { randomTexts_beforeCompleted[currIndex] };
                }
            }
            
        }
    }
}