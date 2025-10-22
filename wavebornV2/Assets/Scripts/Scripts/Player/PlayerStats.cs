using UnityEngine;

/// <summary>
/// Простий клас для зберігання характеристик гравця
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float damage = 10f;
    public float shieldDuration = 3f;

    [Header("Current Runtime Stats")]
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void AddHealth(float value)
    {
        maxHealth += value;
        currentHealth = maxHealth;
        Debug.Log($"❤️ Нове здоров'я: {maxHealth}");
    }

    public void AddDamage(float value)
    {
        damage += value;
        Debug.Log($"⚔️ Новий урон: {damage}");
    }

    public void AddSpeed(float value)
    {
        moveSpeed += value;
        Debug.Log($"🏃‍♂️ Нова швидкість: {moveSpeed}");
    }

    public void AddShieldDuration(float value)
    {
        shieldDuration += value;
        Debug.Log($"🛡️ Новий час щита: {shieldDuration}");
    }
}
