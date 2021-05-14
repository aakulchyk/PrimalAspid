using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeBehavior : MonoBehaviour
{
    public AudioClip clip_collect;
    // Start is called before the first frame update

    public void GetCollected() {
        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        Destroy(this.gameObject, 0.8f);
    }
}
