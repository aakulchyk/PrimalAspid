using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractableBehavior : MonoBehaviour
{
    protected Game game;

    protected PlayerControl player;
    private Transform playerTransform;
    private GameObject canvas = null;

    public float interactRadius = 3f;
    public bool openForDialogue = false;
    public string interactableName;
    public string[] initialTexts;

    public string[] currentTexts = {"...",};
    

    void Start()
    {
        game = (Game)FindObjectOfType(typeof(Game));

        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        playerTransform = GameObject.FindWithTag("Player").transform;

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
            float pDist = Vector2.Distance(playerTransform.position, transform.position); 
            openForDialogue = (pDist < interactRadius);
            canvas.SetActive(openForDialogue);
        }

        if (openForDialogue) {
            player.activeSpeaker = this;
        }
    }

    public virtual void talkToPlayer()
    {
        game.SetPopupTexts(interactableName, currentTexts);
        game.OpenPopup();
    }
}
