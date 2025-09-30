using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public enum StartHpMode { Full, Custom }

    [Header("Config")]
    [SerializeField, Min(1f)] private float maxHP = 100f;        // максимум HP
    [SerializeField] private StartHpMode startMode = StartHpMode.Full;
    [SerializeField, Min(0f)] private float startHP = 100f;      // стартове HP, якщо режим Custom
    [SerializeField] private bool destroyOnDeath = true; // ворогів зазвичай знищуємо

    [Header("Runtime (read-only)")]
    [SerializeField] private float currentHP; // показуємо у інспекторі для дебагу

    // Публічний API
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0f;

    // Події
    public Action<float, float> OnChanged; // (current, max)
    public Action OnDied;
    public Action<float> OnDamaged;        // скільки отримав
    public Action<float> OnHealed;         // скільки зцілено

    private void Awake()
    {
        currentHP = (startMode == StartHpMode.Full)
            ? maxHP
            : Mathf.Clamp(startHP, 0f, maxHP);

        OnChanged?.Invoke(currentHP, maxHP);
    }

    private void OnValidate()
    {
        // тримаємо старт у межах
        maxHP = Mathf.Max(1f, maxHP);
        startHP = Mathf.Clamp(startHP, 0f, maxHP);

        // якщо редагуєш у редакторі в режимі Edit, підтримай узгодженість:
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

    /// <summary>Задати поточне HP (зручний девтул для інспектора/скриптів).</summary>
    public void SetCurrentHP(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxHP);
        if (Mathf.Approximately(clamped, currentHP)) return;

        bool wasAlive = IsAlive;
        currentHP = clamped;
        OnChanged?.Invoke(currentHP, maxHP);

        if (wasAlive && currentHP <= 0f) Die();
    }

    /// <summary>Встановити новий максимум HP. keepRatio=true — зберегти відсоток поточного HP.</summary>
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

    /// <summary>Збільшити максимум HP; healToFull=true — одразу повне відновлення.</summary>
    public void AddMaxHP(float delta, bool healToFull = false)
    {
        SetMaxHP(maxHP + delta, keepRatio: !healToFull);
        if (healToFull)
        {
            currentHP = maxHP;
            OnChanged?.Invoke(currentHP, maxHP);
        }
    }

    /// <summary>Оживити (atPercent 0..1).</summary>
    public void Revive(float atPercent = 1f)
    {
        float p = Mathf.Clamp01(atPercent);
        currentHP = Mathf.Max(1f, maxHP * p);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>Миттєво вбити.</summary>
    public void Kill()
    {
        if (!IsAlive) return;
        currentHP = 0f;
        OnChanged?.Invoke(currentHP, maxHP);
        Die();
    }

    // ================== ВНУТРІШНЄ ==================

    private void Die()
    {
        OnDied?.Invoke();
        if (destroyOnDeath)
            Destroy(gameObject);
        // Якщо це Player — вимкни destroyOnDeath і оброби OnDied у свому менеджері (UI «You Died», респавн тощо)
    }

    // ================== DEV-ШОРТКАТИ (не обов’язково) ==================
#if UNITY_EDITOR
    [ContextMenu("Dev/Damage 10")]
    private void DevDamage10() => TakeDamage(10f);

    [ContextMenu("Dev/Heal 10")]
    private void DevHeal10() => Heal(10f);

    [ContextMenu("Dev/Full Heal")]
    private void DevFullHeal() => SetCurrentHP(maxHP);

    [ContextMenu("Dev/Kill")]
    private void DevKill() => Kill();
#endif
}
