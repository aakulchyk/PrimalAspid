using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntlionBehavior : NpcBehavior
{

    public GameObject projectilePrefab;     // the prefab of our bullet

    MantisProjectile currentProjectile;
    public int _shootRadius = 10;
    private bool shootAnticipate = false;

    public AudioClip clip_shoot;
    public AudioClip clip_shriek;

    public AudioClip boss_theme;
    public AudioClip prev_theme;

    public AudioSource backgroundMusic;


    [SerializeField] public GameObject triggerPlatform;
    [SerializeField] public GameObject deathZone;

    public bool idle = false;


    enum States {
        Idle,
        Aggravate,
        Wait,
        AnticipateShoot,
        Shoot,
        Hurt,
        Die,
        Dead
    }

    private States state = States.Idle;


    void Start()
    {
        BaseInit();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("EnemyProjectile"), true);
    }

    IEnumerator DeleteProjectileAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        currentProjectile = null;
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) return;
         // interact with player

         switch (state) {
            case States.Idle:
                // TODO: optimize;
                if (triggerPlatform == null)
                    OnTriggered();
                break;
            case States.Dead:
                break;
            case States.Aggravate:
                break;
            case States.Wait:
                if (!shootAnticipate && !invulnerable) {
                        anim.SetTrigger("shoot");
                        state = States.AnticipateShoot;
                } break;

            case States.AnticipateShoot:
            break;
            case States.Shoot:
            break;
            case States.Hurt:
                // TODO: sound, animation, etc...
                break;
        }
    }

    public void OnTriggered() {
        state = States.Aggravate;
        sounds.PlayOneShot(clip_shriek);
        anim.SetTrigger("shriek");
        // set background music
        backgroundMusic.Stop();
        prev_theme = backgroundMusic.clip;
        backgroundMusic.clip = boss_theme;
        backgroundMusic.Play();
    }

    public void OnShriekFinished() {
        state = States.Wait;
    }

    public void OnHurtFinished() {
        state = States.Wait;
    }

    public void shoot() {
        if (state != States.AnticipateShoot) return;
        
        Debug.Log("Mantis: Shoot");

        GetComponent<AudioSource>().PlayOneShot(clip_shoot);

        GameObject go = Instantiate(projectilePrefab);

        go.transform.position = gameObject.transform.position +  Vector3.up*10;
        go.transform.rotation = Quaternion.identity;
        //go.transform.localScale = new Vector3(1, 1, 1);
        
        currentProjectile = go.GetComponent<MantisProjectile>();

        Vector3 direction = ((Utils.GetPlayerTransform().position + Vector3.up*2    ) - go.transform.position).normalized;
        
        currentProjectile.setImpulse(direction);
        state = States.Wait;
    }

    protected override bool checkAccessibility(Transform wp)
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D> ();
        Vector3 pos = transform.position;

        Vector3[] points = new Vector3[] {
            new Vector3(pos.x - col.size.x*4f, pos.y, 0),
            new Vector3(pos.x + col.size.x*4f, pos.y, 0),
            new Vector3(pos.x, pos.y - col.size.y*4.5f, 0),
            new Vector3(pos.x, pos.y + col.size.y*3f, 0)
        };

        bool accessible = true;
        foreach (var p in points) {
            float distM = Vector2.Distance(p, wp.position);
            Vector3 dir = wp.position - p;
            RaycastHit2D hit = Physics2D.Raycast(p, dir.normalized, distM);
            if (hit.collider != null && hit.collider.tag != "Player" && hit.collider != col) {
                Debug.DrawLine(p, wp.position, Color.red, 0.02f, false);
                accessible = false;
                continue;
            } else {
                Debug.DrawLine(p, wp.position, Color.green, 0.02f, false);
            }
        }
        
        return accessible;    
    }

    protected override void processCollision(Collision2D collision) {
       /* if (invulnerable) return;

        Collider2D collider = collision.collider;
        if (collider.tag == "Boulder") {
            if (collision.relativeVelocity.magnitude > 6) {
                hurt(Vector2.zero);
            }
        }

        if (collider.tag == "Spike")
        {
            if (currentProjectile && currentProjectile.gameObject == collider.gameObject) {
                return;
            }
            
            hurt(Vector2.zero);
        }*/
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes) {

        if (state == States.Dead) return;


        if (damageType == Types.DamageType.PcHit)
            return;

        Debug.Log("Antlion Hurt");

        StartCoroutine(Utils.FreezeFrameEffect());
        
        
        
        //Debug.Log("NPC Hurt");
        if (--_hp < 0) {
            die();
        } else {
            sounds.PlayOneShot(clip_hurt);
            anim.SetTrigger("hurt");
        }

        Bleed();

        state = States.Wait;
    }

    public void Bleed()
    {
        if (damageParticles) {
            damageParticles.Emit(10);
        }
    }

    protected override void die() {
        shootAnticipate = false;
        state = States.Dead;
        anim.SetTrigger("die");

        sounds.Stop();
        sounds.pitch = 0.8f;
        sounds.volume = 1f;
        sounds.PlayOneShot(clip_shriek);
    }

    private void OnDieFinished() {
        anim.SetBool("dead", true);
        sounds.PlayOneShot(clip_death);

        if (deathParticles) {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }

        // drop loot
        if (collectiblePrefab) {
            StartCoroutine(dropCollectibles());
        }
        
        Destroy(this.gameObject, 2f);

        backgroundMusic.Stop();
        backgroundMusic.clip = prev_theme;
        backgroundMusic.Play();

        deathZone.SetActive(false);
    }

    IEnumerator dropCollectibles() {
        yield return new WaitForSeconds(0.4F);

        for (int i = -1; i<2; i++) {
            GameObject go = Instantiate(collectiblePrefab);

            go.transform.position = gameObject.transform.position + new Vector3(i,1,0);
            go.transform.rotation = Quaternion.identity;
        }      
    }


    void OnDestroy()
    {
        if (currentProjectile) {
            currentProjectile.Delete();  
        } 
    }

    IEnumerator blinkInvulnerable() {
        Renderer r = GetComponent<Renderer>();
        invulnerable = true;
        for (int i=0; i<4; i++)
        {
            r.enabled = false;
            yield return new WaitForSeconds(0.1F);
            r.enabled = true;
            yield return new WaitForSeconds(0.1F);
        }
        invulnerable = false;
    }

  /*  IEnumerator OpenDoorAfterDelay(float time)
    {
        Debug.Log("Open Door!");
        if (controlledWall == null) yield break;
        yield return new WaitForSeconds(time);
        controlledWall.GetComponent<Animator>().SetTrigger("Opened");
        yield return new WaitForSeconds(0.5f);
        controlledWall.GetComponent<AudioSource>().enabled = true;
    }*/

}
