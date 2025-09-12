using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 4f;
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
        if (equipClip && audioSource)
            audioSource.PlayOneShot(equipClip);
    }

    protected abstract void OnAttack();
}
