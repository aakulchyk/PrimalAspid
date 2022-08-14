using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindArea : MonoBehaviour
{
    [Range(-180f, 180f)] public float defaultForceAngle;
    
    [Range(-1000f, 1000f)] public float defaultForceMagnitude;
    
    [Range(-500f, 500f)] public float defaultForceVariation;

    [Range(1, 100)] public int particleSpeedModifier;

    public float windageCoefficient;

    [SerializeField] private AreaEffector2D effectorForPlayer;

    [SerializeField] private AreaEffector2D effectorForRest;

    [SerializeField] private ParticleSystem particles;

    private AudioSource au;

    void Start()
    {
        au = GetComponent<AudioSource>();
        effectorForPlayer.forceAngle = defaultForceAngle;
        effectorForPlayer.forceMagnitude = defaultForceMagnitude;
        effectorForPlayer.forceVariation = defaultForceVariation;

        effectorForRest.forceAngle = defaultForceAngle;
        effectorForRest.forceMagnitude = defaultForceMagnitude;
        effectorForRest.forceVariation = defaultForceVariation;

        var velocity = particles.velocityOverLifetime;
        velocity.speedModifier = particleSpeedModifier;
    }

    void Update()
    {
        effectorForPlayer.forceAngle = defaultForceAngle;
        //effectorForPlayer.forceMagnitude = defaultForceMagnitude;
        effectorForPlayer.forceVariation = defaultForceVariation;

        effectorForRest.forceAngle = defaultForceAngle;
        effectorForRest.forceMagnitude = defaultForceMagnitude;
        effectorForRest.forceVariation = defaultForceVariation;

        
        var velocity = particles.velocityOverLifetime;
        var v = Utils.AngleToVector(effectorForPlayer.forceAngle);
        
        velocity.speedModifier = particleSpeedModifier;
        velocity.x = v.x;
        velocity.y = v.y - 0.1f;
        velocity.z = 0;

        var shape = particles.shape;
        shape.position = v * -20;

        //velocity.speedModifier = areaEffector.forceMagnitude/10;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            StartCoroutine(IncreaseVolume());
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.tag == "Player") {
            var pc = Utils.GetPlayer();
            effectorForPlayer.forceMagnitude
             = pc.WingsOpen() && !pc.IsDashing()
                ? defaultForceMagnitude * windageCoefficient
                : defaultForceMagnitude;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            StartCoroutine(DecreaseVolume());
        }
    }

    IEnumerator DecreaseVolume() {
        while (au.volume>0.1f) {
            au.volume = au.volume - 0.01f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator IncreaseVolume() {
        while (au.volume<1) {
            au.volume = au.volume + 0.01f;
            yield return new WaitForSeconds(0.02f);
        }
    }





}
