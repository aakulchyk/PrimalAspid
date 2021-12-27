using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PcAttack : MonoBehaviour
{
    public GameObject dustFromWhipFxPrefab;
    private PlayerControl player = null;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Animate() {
        if (!player)
            player = GetComponentInParent<PlayerControl>();
            
        GetComponent<Animator>().SetTrigger("hit");
        
        if (player._isGrounded) {
            GameObject go = Instantiate(dustFromWhipFxPrefab);
            go.transform.position = transform.position + Vector3.down*player.pHeight;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = new Vector3(1, 1, 1);
            
            if (!player.faceRight) {
                go.GetComponent<SpriteRenderer>().flipX = true;
            }
            go.GetComponent<Animator>().SetTrigger("hit");
            Destroy(go, 2f);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag=="Enemy") {
            NpcBehavior behavior = other.gameObject.GetComponent<NpcBehavior>();
            Vector3 dir = (other.transform.position - transform.position).normalized;
            
            behavior.hurt(new Vector2(dir.x*40, dir.y*20f), Types.DamageType.PcHit);

            player.knockback(new Vector2(-dir.x*15f, -dir.y*8f));
        }
    }

    /*IEnumerator DeleteFxAfterDelay(GameObject go)
    {
        yield return new WaitForSeconds(1);
        Destroy(go, 2);
        
    }*/
}
