
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class LeverBehavior : MonoBehaviour
{
    public GameObject target;
    public bool toggled = false;
    private string _keyName;

    [SerializeField] private GameObject interactable;

    public void Awake()
    {
        _keyName = SceneManager.GetActiveScene().name + transform.parent.gameObject.name;
        if (PlayerPrefs.GetInt(_keyName) == 1)
            SetToggled();
    }

    private void SetToggled()
    {
        toggled = true;
        transform.Rotate(0, 0, -32f);
        interactable.GetComponent<InteractableBehavior>().openForInteraction = false;
        interactable.SetActive(false);
        PlayerPrefs.SetInt(_keyName, 1);
    }
    
    public void SwitchLever()
    {
        switchItself();
        switchSpecificTarget();
        SetToggled();
    }

    public void switchItself()
    {
        GetComponent<AudioSource>().Play();
    }

    public abstract void switchSpecificTarget();

}


