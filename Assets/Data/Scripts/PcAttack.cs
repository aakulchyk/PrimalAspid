using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PcAttack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Animate() {
        GetComponent<Animator>().SetTrigger("hit");
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag=="Enemy") {
            NpcBehavior behavior = other.gameObject.GetComponent<NpcBehavior>();
            Vector3 dir = (other.transform.position - transform.position).normalized;
            
            behavior.hurt(new Vector2(dir.x*40, dir.y*20f), Types.DamageType.PcHit);

            GetComponentInParent<PlayerControl>().knockback(new Vector2(-dir.x*15f, -dir.y*8f));
        }
    }
}
