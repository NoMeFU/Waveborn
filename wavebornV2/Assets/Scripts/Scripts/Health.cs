using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHP = 50f;

    // ќриг≥нальн≥
    public float MaxHP => maxHP;
    public float CurrentHP { get; private set; }

    // Alias п≥д HudController
    public float Max => MaxHP;
    public float Current => CurrentHP;

    public Action<float, float> OnChanged; // (cur, max)
    public Action OnDied;

    private void Awake()
    {
        CurrentHP = maxHP;
        OnChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(float dmg)
    {
        if (dmg <= 0f) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - dmg);
        OnChanged?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP <= 0f)
        {
            OnDied?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        OnChanged?.Invoke(CurrentHP, MaxHP);
    }
}
