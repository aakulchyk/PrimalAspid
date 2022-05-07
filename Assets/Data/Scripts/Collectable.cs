using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    protected bool collected = false;
    public AudioClip clip_collect;
    public Renderer renderer;
    // Start is called before the first frame update

    public virtual void GetCollected() {
        if (collected) return;

        collected = true;

        // apply stats
        Debug.Log("Collect item. ");

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(clip_collect);
        
        renderer.enabled = false;
        
        Destroy(this.gameObject, 1f);
    }
}
