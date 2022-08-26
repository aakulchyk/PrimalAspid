using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
public class SpeciesSelectMenu : MonoBehaviour
{
    [SerializeField] private Button defaultButton;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    public void BatSelected()
    {
        Debug.Log("Bat selected");
    }

    public Button DefaultButton()
    {
        return defaultButton;
    }
}
