
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MenuControls : MonoBehaviour
{
    public bool _gamePad = false;

    public void StartPressed()
    {
        File.Delete(Application.persistentDataPath + "/gamesave.save");
        SceneManager.LoadScene("MainScene");
    }

    public void ContinuePressed()
    {
         SceneManager.LoadScene("MainScene");
    }

    public void ExitPressed()
    {
        Debug.Log("Exit pressed!");
        Application.Quit();
    }

    public void InputPressed()
    {
        _gamePad = !_gamePad;

        Text buttonText = GameObject.Find("InputButtonText").GetComponent<Text>();
        Debug.Log("Button text: " + buttonText.text);
        buttonText.text = _gamePad ? "Gamepad" : "Keyboard";
    }

    public void TutorialPressed()
    {
        Game game = (Game)FindObjectOfType(typeof(Game));
        PlayerStats.ShowTutorial = !PlayerStats.ShowTutorial;

        Text buttonText = GameObject.Find("TutorialButtonText").GetComponent<Text>();
        Debug.Log("Button text: " + buttonText.text);
        buttonText.text = PlayerStats.ShowTutorial ? "Tutorial ON" : "Tutorial OFF";
    }
}
