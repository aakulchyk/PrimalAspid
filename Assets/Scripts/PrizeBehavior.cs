using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeBehavior : MonoBehaviour
{
    protected bool collected = false;
    public AudioClip clip_collect;
    // Start is called before the first frame update

    public virtual void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        PlayerStats.HP++;
        Debug.Log("Collect item. new HP=" + PlayerStats.HP);

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1f);
    }
}
