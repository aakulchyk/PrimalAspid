using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyablePlatform : MonoBehaviour
{
    protected AudioSource sounds;
    private Animator anim;
    private Renderer _renderer;
    //private BoxCollider2D col = GetComponent<BoxCollider2D>();
    [SerializeField] private GameObject deathParticles;

    public bool needsExtraEffort;
    public bool isShaking = false;
    public AudioClip clip_rumble;
    public AudioClip clip_crack;

    public void StartCollapsing()
    {
        StartCoroutine(CollapseAfterDelay());
    }

    public void JustShake()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake() {
        isShaking = true;
        GetComponent<AudioSource>().PlayOneShot(clip_rumble);
        Utils.GetPlayer().cameraEffects.Shake(0.3f, 500, 2 );
        for (int i=0; i<6;i++) {
            transform.position += Vector3.up * 0.2f;
            yield return new WaitForSeconds(0.1f);
            transform.position += Vector3.down * 0.2f;
            yield return new WaitForSeconds(0.1f);
        }
        GetComponent<AudioSource>().Stop();
        isShaking = false;
    }
    IEnumerator CollapseAfterDelay() {
        yield return Shake();
        OnCollapsed();
    }

    public void OnCollapsed() {
        GetComponent<AudioSource>().PlayOneShot(clip_crack);
        Utils.GetPlayer().cameraEffects.Shake(0.4f, 1200, 0.3f);
        var joint = GetComponent<FixedJoint2D>();
        if (joint) {
            if (joint.connectedBody) {
                Utils.GetPlayerGrabber().endHangOnCeiling();
            }
            joint.enabled = false;
        }
        
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>())
            GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = null;
        }
        
        Destroy(this.gameObject, 1f);
    }

}
