using UnityEngine;

public class GrenadeWeapon : WeaponBase
{
    [Header("Grenade Setup")]
    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private int maxGrenades = 3;
    [SerializeField] private int currentGrenades = 3;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip throwClip;

    protected override void Awake()
    {
        base.Awake();
        if (!throwOrigin) throwOrigin = transform;
    }

    protected override void OnAttack()
    {
        if (currentGrenades <= 0 || !grenadePrefab) return;

        if (throwClip && audioSource) audioSource.PlayOneShot(throwClip);

        var g = Instantiate(grenadePrefab, throwOrigin.position, Quaternion.identity);
        if (g.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = transform.forward + Vector3.up * 0.2f;
            rb.AddForce(dir.normalized * throwForce + Vector3.up * upwardForce, ForceMode.VelocityChange);
        }

        currentGrenades--;

        if (currentGrenades <= 0)
        {
            gameObject.SetActive(false);
            var switcher = GetComponentInParent<WeaponSwitcher>();
            if (switcher) switcher.SelectNext(true);
        }
    }

    public void AddGrenades(int amount)
    {
        currentGrenades = Mathf.Clamp(currentGrenades + amount, 0, maxGrenades);
        if (currentGrenades > 0 && !gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public int Count => currentGrenades;
    public int Max => maxGrenades;
}
