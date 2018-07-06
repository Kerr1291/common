using UnityEngine;
using System.Collections;

public class CubeCombatLogic : MonoBehaviour
{
    public float attackRange = 3f;
    [Header("Time between making attacks")]
    public float attackCooldown = 1f;
    [Header("Distance this cube will be sent flying when taking damage")]
    public float knockback = 150f;
    public CubeCombatStats stats;
    public Rigidbody body;

    // the combat stats are stored instead of the game object as the stats are also the interface to resolving combat with the game object
    protected CubeCombatStats enemyTarget;
    protected float attackRemainingCooldown = 0f;

    protected virtual void Update()
    {
        float dt = Time.deltaTime;
        if (attackRemainingCooldown > 0f)
        {
            attackRemainingCooldown -= dt;
            return;
        }

        if (enemyTarget == null)
            return;

        if ((enemyTarget.transform.position - gameObject.transform.position).magnitude < attackRange)
            AttackTarget(enemyTarget);
    }

    public virtual void AcquireTarget(GameObject acquired)
    {
        CubeCombatStats enemy = acquired.GetComponent<CubeCombatStats>();

        // don't acquire a new target if one already exists, it lacks stats, or is on the same team
        if (enemyTarget != null || (enemy == null || enemy.teamId == stats.teamId))
            return;

        enemyTarget = enemy;
    }

    // this is shitty, especially for ranged
    public virtual void LoseTarget(GameObject lost)
    {
        CubeCombatStats enemyStats = lost.GetComponent<CubeCombatStats>();
        if (enemyStats != enemyTarget)
            return;

        enemyTarget = null;
    }

    public virtual void AttackTarget(CubeCombatStats targetStats)
    {
        int damage = (int)RNG.GaussianRandom(stats.minAttack, stats.maxAttack);
        int enemyRemainingHp = targetStats.TakeDamage(damage, gameObject);
        if (enemyRemainingHp <= 0)
            enemyTarget = null;

        attackRemainingCooldown = attackCooldown;
    }

    // calculates damage after mitigation (if any) as well as any other results that go with being hit
    public virtual int DefendAttack(int damage, GameObject attacker)
    {
        if (knockback > 0f)
        {
            Vector3 trajectory = transform.position - attacker.transform.position;
            trajectory.y = trajectory.magnitude;    // becomes a 45 degree upward fling

            body.AddForce(trajectory.normalized * knockback, ForceMode.VelocityChange);
        }
        return damage;
    }
}
