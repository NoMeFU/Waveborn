using UnityEngine;
using System;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject weaponPrefab;   // оригінальний префаб (для дропа/пікапа)

    [Header("Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 4f;     // атак/сек
    private float cooldown;
    public float CooldownRemaining => cooldown;

    [Header("UI")]
    [SerializeField] private Sprite icon;               // іконка для HUD
    [SerializeField] private string displayName = "Weapon";

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource; // не дублювати у нащадках
    [SerializeField] private AudioClip equipClip;

    // ====== API для HUD/зовнішніх систем ======
    public Sprite Icon => icon;
    public virtual string DisplayName => string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
    public GameObject WeaponPrefab => weaponPrefab;

    public event Action<WeaponBase> Equipped;
    public event Action<WeaponBase> Unequipped;
    public event Action<WeaponBase> Attacked;

    protected virtual void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    /// <summary>Перевірка, чи можемо атакувати прямо зараз.</summary>
    protected virtual bool CanAttack()
    {
        return cooldown <= 0f;
    }

    /// <summary>Спроба атакувати. Повертає true, якщо атака відбулась.</summary>
    public bool TryAttack()
    {
        if (!CanAttack()) return false;

        cooldown = 1f / Mathf.Max(0.01f, fireRate);
        OnAttack();
        Attacked?.Invoke(this);
        return true;
    }

    /// <summary>Власне атака (постріл/удар). Реалізують нащадки.</summary>
    protected abstract void OnAttack();

    /// <summary>Відтворити звук екіпування (викликати з OnEquip або WeaponSwitcher).</summary>
    public void PlayEquipSound()
    {
        if (equipClip)
        {
            if (audioSource) audioSource.PlayOneShot(equipClip);
            else AudioSource.PlayClipAtPoint(equipClip, transform.position);
        }
    }

    /// <summary>WeaponSwitcher повідомляє, який це був префаб (для дропа).</summary>
    public void SetPrefabReference(GameObject prefab)
    {
        if (prefab) weaponPrefab = prefab;
    }

    // ====== Хуки для свічера ======
    public virtual void OnEquip()
    {
        Equipped?.Invoke(this);
        PlayEquipSound();
        // сюди ж можна додати вмикання прицілу, накладення анімацій тощо
    }

    public virtual void OnUnequip()
    {
        Unequipped?.Invoke(this);
        // сюди — зняття бафів/ефектів
    }
}
