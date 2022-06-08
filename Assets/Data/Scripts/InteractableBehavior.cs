using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractableBehavior : MonoBehaviour
{
    protected GameObject canvas = null;

    public float interactRadius = 3f;
    public bool openForInteraction = false;

    public bool active = true;
    public string interactableName;
    public string[] initialTexts;

    public string[] currentTexts = {"...",};

    [SerializeField] private LeverBehavior lever;

    [SerializeField] private AudioClip clip_interact;

    public bool IsSavePoint;
 
    void Start()
    {
        Transform t = transform.Find("Canvas");
        if (t) {
            canvas = t.gameObject;
            canvas.SetActive(false);
        }
        currentTexts = initialTexts;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player") {
            openForInteraction = true;
            if (canvas) {
                canvas.SetActive(openForInteraction);
            }
            Utils.GetPlayer().activeInteractor = this;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player") {
            openForInteraction = false;
            if (canvas) {
                canvas.SetActive(false);
            }
        }
    }

    public void SetActive(bool val)
    {
        active = val;
    }

    public virtual void Interact()
    {
        if (lever) {
            lever.SwitchLever();
            return;
        }

        if (IsSavePoint) {
            if (canvas)
                canvas.SetActive(false);

            // restore player HP to maximum
            PlayerStats.FullyRestoreHP();
            GetComponent<AudioSource>().PlayOneShot(clip_interact);

            // Memorize Save Point position
            Game.SharedInstance.MemorizeCheckPoint(transform.position);
            Game.SharedInstance.SaveGame();
            Game.SharedInstance.SetPopupText("Bonfire", "The Game is Succesfully Saved");
            Game.SharedInstance.OpenPopup();

            if (canvas)
                canvas.SetActive(true);
            return;
        }

        var npc = gameObject.GetComponentInParent<NpcBehavior>();
        if (npc) {
            npc.onTalk();
        }

        Game.SharedInstance.SetPopupTexts(interactableName, currentTexts);
        Game.SharedInstance.OpenPopup();
    }

    public void FlipCanvas() {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(-1*scale.x, scale.y, scale.z);
    }
}
