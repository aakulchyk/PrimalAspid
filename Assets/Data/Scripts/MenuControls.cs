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
        //Game.SharedInstance.DarkenScreenAsync();
        //Game.SharedInstance.StartNewGame();
        StartCoroutine(OnStartPressed());
    }

    public IEnumerator OnStartPressed()
    {

        //if (!Game.SharedInstance.CheckIfGameSaveExistsAndNeedsToBeSaved())
        {
            // darken
            yield return Game.SharedInstance.SetScreenAlphaAsync(0f, 1f, 0.6f);
            Game.SharedInstance.StartNewGame(0);
        }
    }

    public void ContinuePressed()
    {
        //Game.SharedInstance.DarkenScreenAsync();
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
