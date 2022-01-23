using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractableBehavior : MonoBehaviour
{
    
    private GameObject canvas = null;

    public float interactRadius = 3f;
    public bool openForDialogue = false;
    
    public bool active = true;
    public string interactableName;
    public string[] initialTexts;

    public string[] currentTexts = {"...",};
    

    void Start()
    {
        Transform t = transform.Find("Canvas");
        if (t) {
            canvas = t.gameObject;
        }

        currentTexts = initialTexts;
    }

    // Update is called once per frame
    void Update()
    {
        if (canvas) {
            var pTransform = Utils.GetPlayerTransform();

            if (pTransform == null)
                return;
            float pDist = Vector2.Distance(pTransform.position, transform.position); 
            openForDialogue = (pDist < interactRadius && active);
            canvas.SetActive(openForDialogue);
        }

        if (openForDialogue) {
            Utils.GetPlayer().activeSpeaker = this;
        }
    }

    public void SetActive(bool val) {
        active = val;
    }

    public virtual void talkToPlayer()
    {
        var npc = gameObject.GetComponentInParent<NpcBehavior>();
        if (npc)
            npc.onTalk();
        Game.SharedInstance.SetPopupTexts(interactableName, currentTexts);
        Game.SharedInstance.OpenPopup();
    }
}
