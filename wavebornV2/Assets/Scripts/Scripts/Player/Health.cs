using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float baseMaxHP = 100f;
    [SerializeField] private float baseRegen = 1f;
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float regenRate;
    [SerializeField] private bool destroyOnDeath = true;

    public float BaseMaxHP => baseMaxHP;
    public float BaseRegen => baseRegen;
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public float RegenRate => regenRate;
    public bool IsAlive => currentHP > 0f;

    // Події
    public event Action<float, float> OnChanged; // (current, max)
    public event Action OnDied;
    public float currentHealth;

    private bool isDead = false;

    private void Awake()
    {
        maxHP = baseMaxHP;
        regenRate = baseRegen;
        currentHP = maxHP;
        isDead = false;
    }

    private void Update()
    {
        if (!isDead && regenRate > 0f && currentHP < maxHP)
        {
            currentHP += regenRate * Time.deltaTime;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            OnChanged?.Invoke(currentHP, maxHP);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        OnChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f && !isDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void SetMaxHP(float newMax)
    {
        maxHP = newMax;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void SetRegenRate(float newRate)
    {
        regenRate = newRate;
    }

    public void RestoreFullHP()
    {
        currentHP = maxHP;
        OnChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        isDead = true;
        OnDied?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public void Revive(float restorePercent = 1f)
    {
        isDead = false;
        currentHP = maxHP * Mathf.Clamp01(restorePercent);
        OnChanged?.Invoke(currentHP, maxHP);
    }
}
