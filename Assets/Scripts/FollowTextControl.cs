using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FollowTextControl : MonoBehaviour
{
    private PlayerControl player;
    private MaggotRescuedBehavior maggot;
    // Start is called before the first frame update
    private int currentState = -1;

    string[] text;
    void Start()
    {
        text = new String[4] {
            "Find the lost children!\nSome of them will never get back though...",
            "Come closer so he notice you and he will follow you",
            "Lead him to his siblings! Good luck!",
            "YOU KILLED HIM! YOU FUCKING BASTARD!!!"
        };

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        player = (PlayerControl)FindObjectOfType(typeof(PlayerControl));
        maggot = (MaggotRescuedBehavior)FindObjectOfType(typeof(MaggotRescuedBehavior));

        int newState = -1;
        if (maggot.isDead)
            newState = 3;
        else if (player.isLeading && maggot.isFollowing)
            newState = 2;
        else if (player.isLeading && !maggot.isFollowing)
            newState = 1;
        else
            newState = 0;

        if (currentState != newState) {
            if (newState == 3)
                GetComponent<Text>().color = new Color(1f, 0f, 0f, 1f);
            else
                GetComponent<Text>().color = new Color(0.29f, 0.43f, 0.27f, 1f);

            currentState = newState;
            GetComponent<Text>().text = text[currentState];
            

        }
    }
}
