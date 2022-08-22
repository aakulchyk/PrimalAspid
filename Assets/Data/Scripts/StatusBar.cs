using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
public class StatusBar : MonoBehaviour
{
    private Transform bar;

    //[SerializeField] private GameObject energyPanel;
    //[SerializeField] private GameObject energyLevel;
    private Vector2 initialEnergyPanelPos;

    GameObject popup;


    public int popupCloseStatus = 0; // 0=ok, 1=yes. -1=no
    void Start() {

        popup = GameObject.Find("PopUp");

        //energyLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -76);
        //initialEnergyPanelPos = energyPanel.GetComponent<RectTransform>().anchoredPosition;
    }


    void Update() {

        // -23 .. -77
        // 0  ... 100
        float coeff = (90-9)/100f;
        float y = ((float)PlayerStats.Energy * coeff) - 90;

        //energyLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);

        if (PlayerStats.Energy > 99) {
            Vector2 randomVector = Random.insideUnitCircle.normalized;
            //energyPanel.GetComponent<RectTransform>().anchoredPosition = initialEnergyPanelPos + randomVector;
        }
    }

    public void onPopupClose() {
        Debug.Log("close");
        popupCloseStatus = 0;
        popup.SetActive(false);
    }

    public void onPopupAccepted() {
        popupCloseStatus = 1;
        popup.SetActive(false);
    }

    public void onPopupRejected() {
        popupCloseStatus = -1;
        popup.SetActive(false);
    }
}
