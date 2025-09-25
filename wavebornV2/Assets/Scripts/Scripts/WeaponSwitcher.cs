using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<WeaponBase> weapons = new();
    [SerializeField] private int startIndex = 0;
    [SerializeField, Min(1)] private int maxSlots = 3;
    [SerializeField] private bool autoReplaceWhenFull = true;   // якщо повний інвентар: true=замінити поточну, false=ігнор пікап
    [SerializeField] private bool preventDuplicates = true;     // заборонити дублікати того самого префаба

    [Header("Mount Point")]
    [SerializeField] private Transform weaponRoot;

    [Header("Drop")]
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private float dropForce = 4f;

    public WeaponBase Current { get; private set; }
    public int SlotCount => weapons.Count;

    // 🔔 HUD підписується сюди
    public event Action<WeaponBase> WeaponChanged;

    private void Awake()
    {
        maxSlots = Mathf.Clamp(maxSlots, 1, 3);

        if (weapons.Count > 0)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i]) weapons[i].gameObject.SetActive(i == startIndex);

            // Викликаємо вибір стартової — це гарантує OnEquip/звук/подію WeaponChanged
            SelectIndex(Mathf.Clamp(startIndex, 0, weapons.Count - 1));
        }
    }

    public void SelectIndex(int idx)
    {
        if (weapons.Count == 0) return;
        idx = Mathf.Clamp(idx, 0, weapons.Count - 1);

        var newW = weapons[idx];
        if (!newW) return;

        if (Current == newW)
        {
            // вже активна — просто гарантуємо видимість і повідомимо HUD (корисно після рестарту)
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

            WeaponChanged?.Invoke(Current);
            return;
        }

        if (Current) Current.OnUnequip();

        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

        Current = newW;

        if (Current)
        {
            Current.OnEquip();
            Current.PlayEquipSound();
        }

        WeaponChanged?.Invoke(Current); // 🔔 важливо для HUD
    }

    public void SelectNext(bool forward = true)
    {
        if (weapons.Count == 0) return;
        int cur = Mathf.Max(0, weapons.IndexOf(Current));
        int next = (cur + (forward ? 1 : -1) + weapons.Count) % weapons.Count;
        SelectIndex(next);
    }

    public bool AddWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (!weaponPrefab || !weaponRoot) return false;

        if (preventDuplicates && HasWeaponPrefab(weaponPrefab))
            return false;

        if (weapons.Count >= maxSlots)
        {
            if (!autoReplaceWhenFull) return false;
            if (Current != null)
            {
                SpawnPickupFor(Current);
                RemoveWeaponInstance(Current);
            }
        }

        GameObject inst = Instantiate(weaponPrefab, weaponRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        var wb = inst.GetComponent<WeaponBase>() ?? inst.GetComponentInChildren<WeaponBase>();
        if (!wb)
        {
            Debug.LogWarning("WeaponSwitcher: Added weapon prefab has no WeaponBase.");
            Destroy(inst);
            return false;
        }

        wb.SetPrefabReference(weaponPrefab);
        weapons.Add(wb);
        SelectIndex(weapons.Count - 1);
        return true;
    }

    public void DropCurrent()
    {
        if (!Current) return;
        SpawnPickupFor(Current);
        RemoveWeaponInstance(Current);

        if (weapons.Count > 0) SelectIndex(0);
        else Current = null;
    }

    public bool TryGetWeaponOfType<T>(out T result) where T : WeaponBase
    {
        foreach (var w in weapons)
        {
            if (!w) continue;
            if (w is T t) { result = t; return true; }
        }
        result = null;
        return false;
    }

    public bool HasWeaponPrefab(GameObject prefab)
    {
        if (!prefab) return false;
        foreach (var w in weapons)
        {
            if (!w) continue;
            if (w.WeaponPrefab == prefab) return true;
        }
        return false;
    }

    // ==== helpers ====
    private void RemoveWeaponInstance(WeaponBase wb)
    {
        int idx = weapons.IndexOf(wb);
        if (idx >= 0)
        {
            Destroy(weapons[idx].gameObject);
            weapons.RemoveAt(idx);
        }
        if (Current == wb) Current = null;
    }

    private void SpawnPickupFor(WeaponBase wb)
    {
        if (!weaponPickupPrefab || !wb) return;

        GameObject prefab = wb.WeaponPrefab;
        if (!prefab)
        {
            Debug.LogWarning("WeaponSwitcher: weapon has no WeaponPrefab reference.");
            return;
        }

        Vector3 spawn = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        if (Physics.Raycast(spawn + Vector3.up * 2f, Vector3.down, out var hit, 10f,
            LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
        {
            spawn = hit.point + Vector3.up * 0.05f;
        }

        GameObject pickup = Instantiate(weaponPickupPrefab, spawn, Quaternion.identity);

        if (pickup.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
        {
            Vector3 impulse = (transform.forward + Vector3.up * 0.3f).normalized * dropForce;
            rb.AddForce(impulse, ForceMode.VelocityChange);
        }

        var wp = pickup.GetComponent<WeaponPickup>();
        if (wp) wp.Setup(prefab);
    }
}
