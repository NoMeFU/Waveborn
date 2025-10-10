using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public enum StartHpMode { Full, Custom }

    [Header("Config")]
    [SerializeField, Min(1f)] private float maxHP = 100f;
    [SerializeField] private StartHpMode startMode = StartHpMode.Full;
    [SerializeField, Min(0f)] private float startHP = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    [Header("Shield Integration")]
    [SerializeField] private ShieldController shieldController;

    [Header("Runtime (read-only)")]
    [SerializeField] private float currentHP;

    // Публічний API
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0f;
    public bool HasShield => shieldController && shieldController.IsActive;

    // Події
    public Action<float, float> OnChanged;
    public Action OnDied;
    public Action<float> OnDamaged;
    public Action<float> OnHealed;
    public Action<float> OnShieldBlocked; // нова подія для заблокованого урону

    private void Awake()
    {
        currentHP = (startMode == StartHpMode.Full)
            ? maxHP
            : Mathf.Clamp(startHP, 0f, maxHP);

        // Автопошук ShieldController
        if (!shieldController)
        {
            shieldController = GetComponent<ShieldController>();
        }

        OnChanged?.Invoke(currentHP, maxHP);
    }

    private void OnValidate()
    {
        maxHP = Mathf.Max(1f, maxHP);
        startHP = Mathf.Clamp(startHP, 0f, maxHP);

        if (!Application.isPlaying)
        {
            if (startMode == StartHpMode.Full) currentHP = maxHP;
            else currentHP = startHP;
        }
    }

    // ================== ПУБЛІЧНІ МЕТОДИ ==================

    public void TakeDamage(float dmg)
    {
        if (!IsAlive || dmg <= 0f) return;

        if (shieldController && shieldController.TryBlockDamage(dmg))
        {
            OnShieldBlocked?.Invoke(dmg);
            Debug.Log($"<color=cyan>🛡️ {gameObject.name} заблокував {dmg} урону щитом!</color>");
            return; 
        }

        float before = currentHP;
        currentHP = Mathf.Max(0f, currentHP - dmg);

        OnDamaged?.Invoke(Mathf.Max(0f, before - currentHP));
        OnChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f && before > 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f) return;

        float before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);

        float healed = currentHP - before;
        if (healed > 0f) OnHealed?.Invoke(healed);

        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void SetCurrentHP(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxHP);
        if (Mathf.Approximately(clamped, currentHP)) return;

        bool wasAlive = IsAlive;
        currentHP = clamped;
        OnChanged?.Invoke(currentHP, maxHP);

        if (wasAlive && currentHP <= 0f) Die();
    }

    public void SetMaxHP(float newMax, bool keepRatio = true)
    {
        newMax = Mathf.Max(1f, newMax);

        if (keepRatio && maxHP > 0f)
        {
            float ratio = currentHP / maxHP;
            maxHP = newMax;
            currentHP = Mathf.Clamp(ratio * maxHP, 0f, maxHP);
        }
        else
        {
            maxHP = newMax;
            currentHP = Mathf.Min(currentHP, maxHP);
        }

        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void AddMaxHP(float delta, bool healToFull = false)
    {
        SetMaxHP(maxHP + delta, keepRatio: !healToFull);
        if (healToFull)
        {
            currentHP = maxHP;
            OnChanged?.Invoke(currentHP, maxHP);
        }
    }

    public void Revive(float atPercent = 1f)
    {
        float p = Mathf.Clamp01(atPercent);
        currentHP = Mathf.Max(1f, maxHP * p);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void Kill()
    {
        if (!IsAlive) return;
        currentHP = 0f;
        OnChanged?.Invoke(currentHP, maxHP);
        Die();
    }

    public ShieldController GetShield() => shieldController;

    // ================== ВНУТРІШНЄ ==================

    private void Die()
    {
        OnDied?.Invoke();
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}