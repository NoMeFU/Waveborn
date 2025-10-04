using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyXPReward : MonoBehaviour
{
    [Header("XP Reward")]
    [SerializeField] private int xpReward = 50;
    [SerializeField] private bool isEnemy = true; 
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (!isEnemy) return;

        // Знайти Player (по тегу)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player && player.TryGetComponent<PlayerExperience>(out var xp))
        {
            xp.AddXP(xpReward);
            Debug.Log($"💀 Enemy {name} defeated! +{xpReward} XP");
        }
    }
}
