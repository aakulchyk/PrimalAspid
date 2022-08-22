using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System.Collections;

public class MenuControls : MonoBehaviour
{
    public bool _gamePad = false;

    [SerializeField] private AudioClip sound_select;

    private void Start()
    {
        Game.SharedInstance.LightenScreenAsync();
    }
    
    public void StartPressed()
    {
        StartCoroutine(OnStartPressed());
    }

    public IEnumerator OnStartPressed()
    {

        //if (!Game.SharedInstance.CheckIfGameSaveExistsAndNeedsToBeSaved())
        {
            // darken
            Game.SharedInstance.StartNewGame(0);
        }
    }

    public void ContinuePressed()
    {
        Game.SharedInstance.LoadGame(0);
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
        PlayerStats.ShowTutorial = !PlayerStats.ShowTutorial;

        Text buttonText = GameObject.Find("TutorialButtonText").GetComponent<Text>();
        Debug.Log("Button text: " + buttonText.text);
        buttonText.text = PlayerStats.ShowTutorial ? "Tutorial ON" : "Tutorial OFF";
    }

    public void AnyButtonSelected()
    {
        GetComponent<AudioSource>().PlayOneShot(sound_select);
    }
}
