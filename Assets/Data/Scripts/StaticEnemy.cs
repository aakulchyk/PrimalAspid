using System;
using System.Collections;
using UnityEngine;


public class StaticEnemy : NpcBehavior
{
    [SerializeField] private bool respawnable;
    [SerializeField] private float respawnTimeout;

    void Start()
    {
        BaseInit();
    }

    public override void hurt(Vector2 force, Types.DamageType damageType = Types.DamageType.Spikes, int damage = 1) {
        base.hurt(force, damageType, damage);
    }

    protected override void die()
    {
        sounds.pitch = 1;
        sounds.volume = 0.8f;
        base.die();
        //Destroy(this.gameObject, 1f);
        StartCoroutine(RespawnAfterTimeout());
    }

    IEnumerator RespawnAfterTimeout()
    {
        yield return new WaitForSeconds(respawnTimeout);

        isDead = false;
        _hp = INITIAL_HP;
        anim.SetTrigger("respawn");
    }
}