using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractableBehavior : MonoBehaviour
{
    
    private GameObject canvas = null;

    public float interactRadius = 3f;
    public bool openForInteraction = false;
    
    public bool active = true;
    public string interactableName;
    public string[] initialTexts;

    public string[] currentTexts = {"...",};

    [SerializeField] private LeverBehavior lever;
    

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
            openForInteraction = (pDist < interactRadius && active);
            canvas.SetActive(openForInteraction);
        }

        if (openForInteraction) {
            Utils.GetPlayer().activeInteractor = this;
        }
    }

    public void SetActive(bool val) {
        active = val;
    }

    public virtual void Interact()
    {
        var npc = gameObject.GetComponentInParent<NpcBehavior>();
        if (npc) {
            npc.onTalk();
            Game.SharedInstance.SetPopupTexts(interactableName, currentTexts);
            Game.SharedInstance.OpenPopup();
        }

        if (lever) {
            lever.SwitchLever();
        }
    }
}
