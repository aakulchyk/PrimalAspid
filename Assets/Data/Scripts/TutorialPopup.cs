using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public bool IsControlIndependent;

    public string[] schemeDependableText;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {

            if (!IsControlIndependent) {
                int index = (int)Utils.GetCurrentControlType();
                GetComponent<Text>().text = schemeDependableText[index];
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
