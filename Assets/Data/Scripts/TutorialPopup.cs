using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public enum ControlTypes {
        KeyboardAndMouse = 0,
        Gamepad = 1
    }

    public bool IsControlIndependent;

    public string[] schemeDependableText;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            ControlTypes currentScheme = ControlTypes.KeyboardAndMouse;

            if (!IsControlIndependent) {
                string scheme = PlayerPrefs.GetString( "ControlScheme");
                if (scheme != null) {
                    if (scheme == "Keyboard&Mouse")
                        currentScheme = ControlTypes.KeyboardAndMouse;
                    else if (scheme == "Gamepad")
                        currentScheme = ControlTypes.Gamepad;
                } 

                Debug.Log("Change Control Scheme: " + scheme + ", index: " + (int)currentScheme);
                GetComponent<Text>().text = schemeDependableText[(int)currentScheme];
            }
            GetComponent<Animator>().SetBool("showText", true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player") {
            GetComponent<Animator>().SetBool("showText", false);
        }
    }
}
