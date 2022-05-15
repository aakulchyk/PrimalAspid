using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private AudioClip clip_open;

    [SerializeField] private AudioClip clip_close;

    private bool opened = false;

    public bool InitiallyOpened = false;

    private string _keyName;

    


    public void Awake()
    {
        _keyName = SceneManager.GetActiveScene().name + transform.parent.gameObject.name;
        if (PlayerPrefs.GetInt(_keyName) == 1 || InitiallyOpened)
            SetOpened(true);

    }

    public void Open()
    {
        PlayerPrefs.SetInt(_keyName, 1);
        StartCoroutine(OpenCoroutine());
    }

    public void Close()
    {
        Debug.Log("Door close");
        GetComponent<AudioSource>().PlayOneShot(clip_close);
        PlayerPrefs.SetInt(_keyName, 0);
        SetOpened(false);
    }

    private void SetOpened(bool val)
    {
        GetComponent<Animator>().SetBool("opened", val);
        opened = val;
    }

    IEnumerator OpenCoroutine()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().PlayOneShot(clip_open);

        GetComponent<Animator>().SetTrigger("open");

        yield return new WaitForSeconds(5f);
        
        GetComponent<AudioSource>().Stop();
        GetComponent<Collider2D>().enabled = true;
    }

    public void OnDoorOpeningFinished()
    {
        SetOpened(true);
    }

}
