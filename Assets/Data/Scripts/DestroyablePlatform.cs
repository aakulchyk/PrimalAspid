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

    public bool isCollapsing = false;
    public AudioClip clip_rumble;
    public AudioClip clip_crack;

    public void StartCollapsing()
    {
        //GetComponent<Animator>().enabled = true;
        isCollapsing = true;
        GetComponent<AudioSource>().enabled = true;
        StartCoroutine(CollapseAfterDelay(1));
    }

    IEnumerator CollapseAfterDelay(float sec) {
        Utils.GetPlayer().cameraEffects.Shake(0.3f, 500, 2 );
        //GetComponent<Animator>().SetTrigger("rumble");
        yield return new WaitForSeconds(sec);
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_crack);
        //GetComponent<Animator>().SetTrigger("collapse");
        OnCollapsed();
    }

    public void OnCollapsed() {
        Utils.GetPlayer().cameraEffects.Shake(0.4f, 1200, 0.3f);
        var joint = GetComponent<FixedJoint2D>();
        if (joint) {
            if (joint.connectedBody) {
                Utils.GetPlayerGrabber().endHangOnCeiling();
            }
            joint.enabled = false;
        }
        
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = null;
        }
        
        Destroy(this.gameObject, 0.5f);
    }

}
