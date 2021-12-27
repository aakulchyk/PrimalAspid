using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointBehavior : MonoBehaviour
{
    private Game game;
    private GameObject canvas;
    private const float interactRadius = 2.5f;

    public bool canInteract;
    // Start is called before the first frame update
    void Start()
    {
        game = (Game)FindObjectOfType(typeof(Game));
        canvas = gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Transform playerTransform = GameObject.FindWithTag("Player").transform;
        float pDist = Vector2.Distance(playerTransform.position, transform.position);

        canInteract = (pDist < interactRadius);
        canvas.SetActive(canInteract);

        if (canInteract) {
            Utils.GetPlayer().activeSavePoint = this;
        }
    }

    public void SaveGame() {
        canvas.SetActive(false);
        game.SaveGame();
        game.SetPopupText("Bonfire", "The Game is Succesfully Saved");
        game.OpenPopup();
        canvas.SetActive(true);
    }
}
