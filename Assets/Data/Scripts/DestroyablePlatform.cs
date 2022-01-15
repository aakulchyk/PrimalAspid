using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyablePlatform : MonoBehaviour
{
    protected AudioSource sounds;
    private Animator anim;
    private Renderer _renderer;
    //private BoxCollider2D col = GetComponent<BoxCollider2D>();

    public bool isCollapsing = false;
    public AudioClip clip_rumble;
    public AudioClip clip_crack;

    public void StartCollapsing()
    {
        isCollapsing = true;
        GetComponent<AudioSource>().enabled = true;
        StartCoroutine(CollapseAfterDelay(1));
    }

    IEnumerator CollapseAfterDelay(float sec) {
        Utils.GetPlayer().cameraEffects.Shake(100, 1);
        GetComponent<Animator>().SetTrigger("rumble");
        yield return new WaitForSeconds(sec);
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(clip_crack);
        GetComponent<Animator>().SetTrigger("collapse");
    }

    public void OnCollapsed() {
        Utils.GetPlayer().cameraEffects.Shake(1000, 0.3f);
        var joint = GetComponent<FixedJoint2D>();
        if (joint) {
            if (joint.connectedBody) {
                Utils.GetPlayerGrabber().endHangOnCeiling();
            }
        }
        Destroy(this.gameObject, 0.01f);
    }

}
