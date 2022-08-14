using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{

    private bool _cameFromRight;
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private Transform[] cameFromTrigger;
    // Start is called before the first frame update
    bool fallCooldown = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other) {
        //GetComponent<AudioSource>().PlayOneShot(clip_splash);

        if (other.tag == "Player") {
            if (fallCooldown)
                return;
            Debug.Log("Player Fall");
            var pos = spawnPoint[0].position;
            //pos.x = save.px;
            //pos.y = save.py;
            Utils.GetPlayer().hurt(Vector2.zero, Types.DamageType.DeathZone);
            Game.SharedInstance.DarkenScreen();
            Utils.GetPlayer().transform.parent.position = new Vector3(pos.x, pos.y, pos.z);
            fallCooldown = true;
            StartCoroutine(restoreFallCooldownShortly());
        }
    }

    IEnumerator restoreFallCooldownShortly()
    {
        yield return new WaitForSeconds(0.5F);
        fallCooldown = false;
        Game.SharedInstance.LightenScreenAsync();
    }
}
