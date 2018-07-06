using UnityEngine;
using System.Collections;

public class CubeCombatShooterLogic : CubeCombatLogic
{
    public float shotDisplayTime = .5f;
    public bool damageResetsAttackCooldown = false;
    public AudioSource shotSoundFx;
    public LineRenderer lineRenderer;
    private float shotDisplayRemainingTime = 0f;

    protected override void Update()
    {
        base.Update();

        if (shotDisplayRemainingTime > 0f)
        {
            shotDisplayRemainingTime -= Time.deltaTime;

            if (shotDisplayRemainingTime <= 0f)
                lineRenderer.enabled = false;
        }
    }

    override public void AttackTarget(CubeCombatStats targetStats)
    {
        base.AttackTarget(targetStats);
        lineRenderer.SetPosition(0, stats.parent.transform.position);
        lineRenderer.SetPosition(1, targetStats.parent.transform.position);
        lineRenderer.enabled = true;
        shotDisplayRemainingTime = shotDisplayTime;

        if (shotSoundFx)
            shotSoundFx.Play();
    }

    override public int DefendAttack(int damage, GameObject attacker)
    {
        if(damageResetsAttackCooldown)
            attackRemainingCooldown = attackCooldown;

        return base.DefendAttack(damage, attacker);
    }
}
