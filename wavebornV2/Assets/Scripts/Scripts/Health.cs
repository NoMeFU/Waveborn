using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public enum StartHpMode { Full, Custom }

    [Header("Config")]
    [SerializeField, Min(1f)] private float maxHP = 100f;        // �������� HP
    [SerializeField] private StartHpMode startMode = StartHpMode.Full;
    [SerializeField, Min(0f)] private float startHP = 100f;      // �������� HP, ���� ����� Custom
    [SerializeField] private bool destroyOnDeath = true; // ������ �������� �������

    [Header("Runtime (read-only)")]
    [SerializeField] private float currentHP; // �������� � ��������� ��� ������

    // �������� API
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0f;

    // ��䳿
    public Action<float, float> OnChanged; // (current, max)
    public Action OnDied;
    public Action<float> OnDamaged;        // ������ �������
    public Action<float> OnHealed;         // ������ �������

    private void Awake()
    {
        currentHP = (startMode == StartHpMode.Full)
            ? maxHP
            : Mathf.Clamp(startHP, 0f, maxHP);

        OnChanged?.Invoke(currentHP, maxHP);
    }

    private void OnValidate()
    {
        // ������� ����� � �����
        maxHP = Mathf.Max(1f, maxHP);
        startHP = Mathf.Clamp(startHP, 0f, maxHP);

        // ���� ������� � �������� � ����� Edit, �������� �����������:
        if (!Application.isPlaying)
        {
            if (startMode == StartHpMode.Full) currentHP = maxHP;
            else currentHP = startHP;
        }
    }

    // ================== ���˲�Ͳ ������ ==================

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

    /// <summary>������ ������� HP (������� ������ ��� ����������/�������).</summary>
    public void SetCurrentHP(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, maxHP);
        if (Mathf.Approximately(clamped, currentHP)) return;

        bool wasAlive = IsAlive;
        currentHP = clamped;
        OnChanged?.Invoke(currentHP, maxHP);

        if (wasAlive && currentHP <= 0f) Die();
    }

    /// <summary>���������� ����� �������� HP. keepRatio=true � �������� ������� ��������� HP.</summary>
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

    /// <summary>�������� �������� HP; healToFull=true � ������ ����� ����������.</summary>
    public void AddMaxHP(float delta, bool healToFull = false)
    {
        SetMaxHP(maxHP + delta, keepRatio: !healToFull);
        if (healToFull)
        {
            currentHP = maxHP;
            OnChanged?.Invoke(currentHP, maxHP);
        }
    }

    /// <summary>������� (atPercent 0..1).</summary>
    public void Revive(float atPercent = 1f)
    {
        float p = Mathf.Clamp01(atPercent);
        currentHP = Mathf.Max(1f, maxHP * p);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>������ �����.</summary>
    public void Kill()
    {
        if (!IsAlive) return;
        currentHP = 0f;
        OnChanged?.Invoke(currentHP, maxHP);
        Die();
    }

    // ================== ����в�ͪ ==================

    private void Die()
    {
        OnDied?.Invoke();
        if (destroyOnDeath)
            Destroy(gameObject);
        // ���� �� Player � ������ destroyOnDeath � ������ OnDied � ����� �������� (UI �You Died�, ������� ����)
    }

    // ================== DEV-�������� (�� ����������) ==================
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
