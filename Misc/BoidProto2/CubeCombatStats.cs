using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TeamId { Red, Blue, Green, Purple };

public class CubeCombatStats : MonoBehaviour
{
    public int hitPoints = 100;
    public int minAttack = 100;
    public int maxAttack = 100;
    public float speedModifier = 1f;
    public TeamId teamId = TeamId.Red;

    public CubeCombatLogic combatLogic;
    public GameObject parent;

    [Header("These effects will not clean themselves up unless you tell them to")]
    public List<GameObject> deathEffectPrefabs;

    void Die()
    {
        Destroy(parent);
        for (int i = 0; i < deathEffectPrefabs.Count; ++i)
        {
            Instantiate(deathEffectPrefabs[i], transform.position, Quaternion.identity);
        }
    }

    // when an enemy is detected, combat logic acquires the target and decides how to proceed
    void OnTriggerEnter(Collider other)
    {
        if (combatLogic == null)
            return;

        combatLogic.AcquireTarget(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (combatLogic == null)
            return;

        combatLogic.LoseTarget(other.gameObject);
    }

    // returns health remaining
    public int TakeDamage(int damage, GameObject attacker)
    {
        hitPoints -= combatLogic.DefendAttack(damage, attacker);

        if (hitPoints <= 0)
            Die();

        return hitPoints;
    }

    public static Color ColorFromTeamId(TeamId team)
    {
        switch (team)
        {
            case TeamId.Red: return new Color(1f, 0f, 0f);
            case TeamId.Green: return new Color(0f, 1f, 0f);
            case TeamId.Blue: return new Color(0f, 0f, 1f);
            case TeamId.Purple: return new Color(0f, 1f, 1f);
            default: return new Color(0f, 0f, 0f);
        }
    }
}
