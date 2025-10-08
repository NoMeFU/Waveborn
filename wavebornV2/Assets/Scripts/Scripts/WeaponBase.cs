using UnityEngine;
using System;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 4f; // атак/сек
    private float cooldown;
    public float CooldownRemaining => cooldown;

    [Header("UI")]
    [SerializeField] private Sprite icon;
    [SerializeField] private string displayName = "Weapon";

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] private AudioClip equipClip;
    [SerializeField] protected AudioClip fireClip; 

    public event Action<WeaponBase> Equipped;
    public event Action<WeaponBase> Unequipped;
    public event Action<WeaponBase> Attacked;

    public Sprite Icon => icon;
    public virtual string DisplayName => string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
    public GameObject WeaponPrefab => weaponPrefab;

    protected virtual void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    protected virtual bool CanAttack() => cooldown <= 0f;

    public bool TryAttack()
    {
        if (!CanAttack()) return false;

        cooldown = 1f / Mathf.Max(0.01f, fireRate);
        OnAttack();
        Attacked?.Invoke(this);

        PlayFireSound();
        AnimFire();

        return true;
    }

    protected abstract void OnAttack();

    public virtual void AnimFire() { }

    public void PlayEquipSound()
    {
        if (equipClip)
        {
            if (audioSource) audioSource.PlayOneShot(equipClip);
            else AudioSource.PlayClipAtPoint(equipClip, transform.position);
        }
    }

    protected void PlayFireSound()
    {
        if (fireClip)
        {
            if (audioSource) audioSource.PlayOneShot(fireClip);
            else AudioSource.PlayClipAtPoint(fireClip, transform.position);
        }
    }

    public void SetPrefabReference(GameObject prefab)
    {
        if (prefab) weaponPrefab = prefab;
    }

    public virtual void OnEquip()
    {
        Equipped?.Invoke(this);
        PlayEquipSound();
    }

    public virtual void OnUnequip()
    {
        Unequipped?.Invoke(this);
    }
}
