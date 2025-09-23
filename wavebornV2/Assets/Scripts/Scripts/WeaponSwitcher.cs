using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<WeaponBase> weapons = new();
    [SerializeField] private int startIndex = 0;
    [SerializeField, Min(1)] private int maxSlots = 3;

    [Header("Mount Point")]
    [SerializeField] private Transform weaponRoot;

    [Header("Drop")]
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private float dropForce = 4f;

    public WeaponBase Current { get; private set; }
    public int SlotCount => weapons.Count;

    private void Awake()
    {
        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == startIndex);

        if (weapons.Count > 0)
        {
            Current = weapons[Mathf.Clamp(startIndex, 0, weapons.Count - 1)];
            Current?.PlayEquipSound();
        }
    }

    public void SelectIndex(int idx)
    {
        if (weapons.Count == 0) return;
        idx = Mathf.Clamp(idx, 0, weapons.Count - 1);

        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

        Current = weapons[idx];
        Current?.PlayEquipSound();
    }

    public void SelectNext(bool forward = true)
    {
        if (weapons.Count == 0) return;
        int cur = Mathf.Max(0, weapons.IndexOf(Current));
        int next = (cur + (forward ? 1 : -1) + weapons.Count) % weapons.Count;
        SelectIndex(next);
    }

    // --------- НОВЕ: знайти перший інстанс зброї певного типу в інвентарі ---------
    public bool TryGetWeaponOfType<T>(out T result) where T : WeaponBase
    {
        foreach (var w in weapons)
        {
            if (!w) continue;
            var t = w as T;
            if (t != null) { result = t; return true; }
        }
        result = null;
        return false;
    }

    // ---------- ПІДБІР (працює як було) ----------
    public void AddWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (!weaponPrefab || !weaponRoot) return;

        if (weapons.Count >= maxSlots && Current != null)
        {
            SpawnPickupFor(Current);
            RemoveWeaponInstance(Current);
        }

        GameObject inst = Instantiate(weaponPrefab, weaponRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        var wb = inst.GetComponent<WeaponBase>() ?? inst.GetComponentInChildren<WeaponBase>();
        if (!wb) { Debug.LogWarning("Added weapon prefab has no WeaponBase."); Destroy(inst); return; }

        wb.SetPrefabReference(weaponPrefab);

        weapons.Add(wb);
        SelectIndex(weapons.Count - 1);
    }

    public void DropCurrent()
    {
        if (!Current) return;
        SpawnPickupFor(Current);
        RemoveWeaponInstance(Current);
        if (weapons.Count > 0) SelectIndex(0);
    }

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
        if (!prefab) { Debug.LogWarning("WeaponSwitcher: weapon has no WeaponPrefab reference."); return; }

        Vector3 spawn = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        if (Physics.Raycast(spawn + Vector3.up * 2f, Vector3.down, out var hit, 10f,
            LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            spawn = hit.point + Vector3.up * 0.05f;

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
