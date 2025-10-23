using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [Header("Основні параметри")]
    public float damage = 10f;
    public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public float regen = 0f;
    public float shieldDuration = 0f;

    [Header("Критичні удари")]
    [Range(0f, 100f)] public float critChance = 10f;
    [Range(1f, 5f)] public float critMultiplier = 1.5f;

    [Header("Інше")]
    public float fireRate = 1f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.SetMaxHP(maxHealth);
            health.SetRegenRate(regen);
        }
    }

    public void AddDamage(float amount) => damage += amount;
    public void AddSpeed(float amount) => moveSpeed += amount;
    public void AddHealth(float amount)
    {
        maxHealth += amount;
        if (health != null)
            health.SetMaxHP(maxHealth);
    }
    public void AddRegen(float amount)
    {
        regen += amount;
        if (health != null)
            health.SetRegenRate(regen);
    }
    public void AddShieldDuration(float amount) => shieldDuration += amount;
    public void AddCritChance(float amount) => critChance += amount;
    public void AddCritMultiplier(float amount) => critMultiplier += amount;

    public void AddFireRate(float amount)
    {
        fireRate += amount;
        fireRate = Mathf.Clamp(fireRate, 0.1f, 10f);
    }
}
