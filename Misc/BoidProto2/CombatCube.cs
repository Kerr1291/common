using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using nv;

public class CombatCube : MonoBehaviour {
    private enum CubeTeam { Red, Blue, Green, Purple, Studio };

    [Header("Force exerted on attacked targetsis scaled per point of damage dealt")]
    public float knockbackForce = 1f;

    [Range(0f, 1f)]
    public float knockbackResistance = 0f;
    public int hitPoints = 1;
    public int minAttack = 1;
    public int maxAttack = 1;
    public float attackCooldown = 1f;

    
    [Header("Spawns these game objects on death")]
    public List<GameObject> deathEffectPrefabs;

    public GameObject parent;
    private Rigidbody body;
    private CubeTeam teamId = CubeTeam.Studio;
    private float attackRemainingCooldown = 0f;

    void Start () {
        body = parent.gameObject.GetComponent<Rigidbody>();
        switch (parent.tag)
        {
            case "RedCube": teamId = CubeTeam.Red; break;
            case "GreenCube": teamId = CubeTeam.Green; break;
            case "BlueCube": teamId = CubeTeam.Blue; break;
            case "PurpleCube": teamId = CubeTeam.Purple; break;
            default: break;
        }	
	}

    void Update() {
        if (hitPoints <= 0)
            Die();
        else if (attackRemainingCooldown > 0f)
            attackRemainingCooldown -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) {
        if (attackRemainingCooldown > 0f)
            return;

        CombatCube target = other.GetComponent<CombatCube>();
        if (target == null || target.teamId == teamId)
            return;

        AttackTarget(target);
    }

    private void AttackTarget(CombatCube target) {
        int damage = (int)GameRNG.GaussianRandom(minAttack, maxAttack);
        target.DefendAttack(damage, this, knockbackForce);
    }

    private int DefendAttack(int damage, CombatCube attacker, float knockback = 0f) {
        hitPoints -= damage;
        if(knockback > 0f && knockbackResistance < 1f) {
            float force = (1f - knockbackResistance) * (knockback * damage);
            Vector3 trajectory = transform.position - attacker.transform.position;
            trajectory.y = trajectory.magnitude;    // becomes a 45 degree upward fling

            body.AddForce(trajectory.normalized * force, ForceMode.VelocityChange);
        }
        return hitPoints;
    }

    void Die() {
        Destroy(parent);
        for (int i = 0; i < deathEffectPrefabs.Count; ++i) {
            Instantiate(deathEffectPrefabs[i], transform.position, Quaternion.identity);
        }
    }
}
