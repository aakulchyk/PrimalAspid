using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Water : MonoBehaviour
{
    [SerializeField] private AudioClip clip_splash;
    [SerializeField] private AudioClip clip_jumpout;

    [SerializeField] private AudioClip clip_reflux;

    private bool opened;
    private string _keyName;

    public void Awake()
    {
        _keyName = SceneManager.GetActiveScene().name + transform.parent.gameObject.name;
        if (PlayerPrefs.GetInt(_keyName) == 1)
            SetOpened();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            GetComponent<AudioSource>().PlayOneShot(clip_splash);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            GetComponent<AudioSource>().PlayOneShot(clip_jumpout);
        }
    }

    private void SetOpened()
    {
        GetComponent<Animator>().SetBool("opened", true);
        opened = true;
    }

    public void SetCameraFocusAndReflux()
    {
        opened = true;
        PlayerPrefs.SetInt(_keyName, 1);
        StartCoroutine(Reflux());
    }

    IEnumerator Reflux()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<AudioSource>().PlayOneShot(clip_reflux);

        GetComponent<Animator>().SetTrigger("open");

        yield return new WaitForSeconds(5f);
        
        GetComponent<AudioSource>().Stop();
        GetComponent<Collider2D>().enabled = true;
    }

}
