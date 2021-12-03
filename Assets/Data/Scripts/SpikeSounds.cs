using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeSounds : MonoBehaviour
{
    public AudioClip clip_appear;
    public AudioClip clip_disappear;
    public AudioClip worm_appear;
    public AudioSource source; 
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playAppearSound()
    {
        source.PlayOneShot(clip_appear);
    }

    public void playDisappearSound()
    {
        source.PlayOneShot(clip_disappear);
    }

    

    public void playSpikeWormSound()
    {
        source.PlayOneShot(worm_appear);
    }
}
