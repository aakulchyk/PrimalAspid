using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private AudioClip clip_open;

    [SerializeField] private AudioClip clip_close;

    private bool opened = false;

    private string _keyName;


    public void Awake()
    {
        _keyName = SceneManager.GetActiveScene().name + transform.parent.gameObject.name;
        if (PlayerPrefs.GetInt(_keyName) == 1)
            SetOpened();
        //opened = true;
    }

    public void Open()
    {
        opened = true;
        PlayerPrefs.SetInt(_keyName, 1);

        StartCoroutine(OpenCoroutine());
    }

    private void SetOpened()
    {
        GetComponent<Animator>().SetBool("opened", true);
        opened = true;
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

}
