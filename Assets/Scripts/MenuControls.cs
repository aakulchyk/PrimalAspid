
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuControls : MonoBehaviour
{
    public bool _gamePad = false;
    public void StartPressed()
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
        //UI.Text text = buttonText.GetComponent<Text>;
        //text.text = "hui";
    }
}
