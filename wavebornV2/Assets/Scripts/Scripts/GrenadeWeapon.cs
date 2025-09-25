using UnityEngine;
using System;

public class GrenadeWeapon : WeaponBase, IGrenadeInfo
{
    [Header("Grenade Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 16f;

    [Header("Ammo")]
    [SerializeField] private int maxGrenades = 3;
    [SerializeField] private int startGrenades = 3;

    public int CurrentGrenades { get; private set; }
    public int MaxGrenades => maxGrenades;
    public event Action<int, int> GrenadeCountChanged;

    protected override void Awake()
    {
        base.Awake();
        CurrentGrenades = Mathf.Clamp(startGrenades, 0, maxGrenades);
        GrenadeCountChanged?.Invoke(CurrentGrenades, MaxGrenades);
    }

    protected override void OnAttack()
    {
        if (CurrentGrenades <= 0) return;
        if (!grenadePrefab || !throwPoint) return;

        // кинули гранату
        var g = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        if (g.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = throwPoint.forward * throwForce;

        CurrentGrenades = Mathf.Max(0, CurrentGrenades - 1);
        GrenadeCountChanged?.Invoke(CurrentGrenades, MaxGrenades);
    }

    // додати здобич/підбір
    public void AddGrenades(int amount)
    {
        if (amount <= 0) return;
        CurrentGrenades = Mathf.Clamp(CurrentGrenades + amount, 0, MaxGrenades);
        GrenadeCountChanged?.Invoke(CurrentGrenades, MaxGrenades);
    }
}
