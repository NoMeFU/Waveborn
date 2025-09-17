using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject weaponPrefab; // <- ОРИГІНАЛЬНИЙ префаб цієї зброї (прив'яжи на самому префабі)

    [Header("Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 4f;   // атак/сек
    private float cooldown;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] private AudioClip equipClip;

    protected virtual void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    public void TryAttack()
    {
        if (cooldown > 0f) return;
        cooldown = 1f / Mathf.Max(0.01f, fireRate);
        OnAttack();
    }

    public void PlayEquipSound()
    {
        if (equipClip && audioSource) audioSource.PlayOneShot(equipClip);
    }

    // Викличе WeaponSwitcher після інстансу з пікапа — щоб знати, що дропати потім
    public void SetPrefabReference(GameObject prefab)
    {
        if (prefab != null) weaponPrefab = prefab;
    }

    public GameObject WeaponPrefab => weaponPrefab;

    protected abstract void OnAttack();
}
