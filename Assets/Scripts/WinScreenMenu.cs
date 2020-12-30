using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinScreenMenu : MonoBehaviour
{
    public Text timeText;
    public Text deathsText;
    public Text lossesText;
    

    void Start()
    {
        timeText.text = "Time: " + PlayerStats.Time.ToString();
        deathsText.text = "You died " + PlayerStats.Deaths + " times";
        lossesText.text = "Maggot died " + PlayerStats.Losses + " times";
    }

    public void RestartPressed()
    {
        Debug.Log("Restart pressed!");
        PlayerStats.Deaths = 0;
        PlayerStats.Losses = 0;
        SceneManager.LoadScene("MainScene");
    }

    public void ExitPressed()
    {
        Debug.Log("Exit pressed!");
        Application.Quit();
    }
}
