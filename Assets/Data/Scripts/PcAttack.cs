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

    public void Animate() {
        if (!player)
            player = GetComponentInParent<PlayerControl>();
            
        GetComponent<Animator>().SetTrigger("hit");
        
        if (player._isGrounded) {
            GameObject go = Instantiate(dustFromWhipFxPrefab);
            go.transform.position = transform.position + Vector3.down*player.pHeight/1.7f;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = new Vector3(1, 1, 1);
            
            if (!player.faceRight) {
                go.GetComponent<SpriteRenderer>().flipX = true;
            }
            go.GetComponent<Animator>().SetTrigger("hit");
            Destroy(go, 1.5f);

            
        }
    }

    public void ShakeCam()
    {
        if (player._isGrounded)
            player.cameraEffects.Shake(0.2f, 400, 0.2f);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag=="Enemy") {
            NpcBehavior behavior = other.gameObject.GetComponent<NpcBehavior>();
            Vector3 dir = (other.transform.position - player.transform.position).normalized;
            behavior.hurt(new Vector2(dir.x*50, 1), Types.DamageType.PcHit);
            player.cameraEffects.Shake(0.3f, 1000, 0.4f);
            player.knockback(new Vector2(-dir.x*15f, 0));
            PlayerStats.PartlyRestoreStamina(10);
        } else if (other.tag=="Breakable") {
            Breakable breakable = other.gameObject.GetComponent<Breakable>();
            breakable.hurt(1);
            player.cameraEffects.Shake(0.3f, 1000, 0.4f);
            Vector3 dir = (other.transform.position - player.transform.position).normalized;
            player.knockback(new Vector2(-dir.x*20f, 0));
        }
    }
}
