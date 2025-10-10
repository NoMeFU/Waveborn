using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<WeaponBase> weapons = new();
    [SerializeField] private int startIndex = 0;
    [SerializeField, Min(1)] private int maxSlots = 3;
    [SerializeField] private bool autoReplaceWhenFull = true;
    [SerializeField] private bool preventDuplicates = true;

    [Header("Mount Point")]
    [SerializeField] private Transform weaponRoot;

    [Header("Drop")]
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private float dropForce = 4f;

    [Header("Animator Link")]
    [SerializeField] private Animator animator;
    private static readonly int WeaponTypeParam = Animator.StringToHash("WeaponType");

    public WeaponBase Current { get; private set; }
    public int SlotCount => weapons.Count;
    public event Action<WeaponBase> WeaponChanged;

    private void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        maxSlots = Mathf.Clamp(maxSlots, 1, 3);

        // Деактивуємо всі, активуємо тільки стартову
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i])
                weapons[i].gameObject.SetActive(i == startIndex);
        }

        // Вибір початкової зброї
        if (weapons.Count > 0)
        {
            SelectIndex(Mathf.Clamp(startIndex, 0, weapons.Count - 1));
        }
        else
        {
            Current = null;
            WeaponChanged?.Invoke(null);
            UpdateAnimatorWeaponType();
        }
    }

    public void SelectIndex(int idx)
    {
        if (weapons.Count == 0)
        {
            Current = null;
            WeaponChanged?.Invoke(null);
            UpdateAnimatorWeaponType();
            return;
        }

        idx = Mathf.Clamp(idx, 0, weapons.Count - 1);
        WeaponBase newW = weapons[idx];

        if (!newW)
        {
            Current = null;
            WeaponChanged?.Invoke(null);
            UpdateAnimatorWeaponType();
            return;
        }

        if (Current == newW)
        {
            for (int i = 0; i < weapons.Count; i++)
                if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

            WeaponChanged?.Invoke(Current);
            UpdateAnimatorWeaponType();
            return;
        }

        // Вимкнути попередню
        if (Current)
        {
            Current.OnUnequip();
            Current.gameObject.SetActive(false);
        }

        // Активувати нову
        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

        Current = newW;

        if (Current)
        {
            Current.OnEquip();
            Current.PlayEquipSound();
        }

        WeaponChanged?.Invoke(Current);
        UpdateAnimatorWeaponType();
    }

    public void SelectNext(bool forward = true)
    {
        if (weapons.Count == 0)
        {
            Current = null;
            WeaponChanged?.Invoke(null);
            UpdateAnimatorWeaponType();
            return;
        }

        int cur = Mathf.Max(0, weapons.IndexOf(Current));
        int next = (cur + (forward ? 1 : -1) + weapons.Count) % weapons.Count;
        SelectIndex(next);
    }

    public bool AddWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (!weaponPrefab || !weaponRoot) return false;
        if (preventDuplicates && HasWeaponPrefab(weaponPrefab)) return false;

        // Якщо повний інвентар — дропаємо поточну
        if (weapons.Count >= maxSlots)
        {
            if (!autoReplaceWhenFull) return false;

            if (Current)
            {
                SpawnPickupFor(Current);
                RemoveWeaponInstance(Current);
            }
        }

        // Створення нової зброї
        GameObject inst = Instantiate(weaponPrefab, weaponRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        WeaponBase wb = inst.GetComponent<WeaponBase>() ?? inst.GetComponentInChildren<WeaponBase>();
        if (!wb)
        {
            Debug.LogWarning("WeaponSwitcher: Added weapon prefab has no WeaponBase.");
            Destroy(inst);
            return false;
        }

        wb.SetPrefabReference(weaponPrefab);
        weapons.Add(wb);

        // Активуємо її одразу
        SelectIndex(weapons.Count - 1);
        return true;
    }

    public void DropCurrent()
    {
        if (!Current) return;

        SpawnPickupFor(Current);
        RemoveWeaponInstance(Current);

        if (weapons.Count > 0)
        {
            SelectIndex(0);
        }
        else
        {
            Current = null;
            WeaponChanged?.Invoke(null);
            UpdateAnimatorWeaponType();
        }
    }

    public bool TryGetWeaponOfType<T>(out T result) where T : WeaponBase
    {
        foreach (var w in weapons)
        {
            if (w is T t)
            {
                result = t;
                return true;
            }
        }
        result = null;
        return false;
    }

    public bool HasWeaponPrefab(GameObject prefab)
    {
        if (!prefab) return false;
        foreach (var w in weapons)
        {
            if (w && w.WeaponPrefab == prefab)
                return true;
        }
        return false;
    }

    private void RemoveWeaponInstance(WeaponBase wb)
    {
        int idx = weapons.IndexOf(wb);
        if (idx >= 0)
        {
            Destroy(weapons[idx].gameObject);
            weapons.RemoveAt(idx);
        }
        if (Current == wb)
            Current = null;
    }

    private void SpawnPickupFor(WeaponBase wb)
    {
        if (!weaponPickupPrefab || !wb) return;

        GameObject prefab = wb.WeaponPrefab;
        if (!prefab) return;

        Vector3 spawn = transform.position + transform.forward + Vector3.up;
        if (Physics.Raycast(spawn + Vector3.up * 2f, Vector3.down, out var hit, 10f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
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

    private void UpdateAnimatorWeaponType()
    {
        if (!animator) return;
        int typeValue = Current ? (int)Current.Type : 0;
        animator.SetInteger(WeaponTypeParam, typeValue);
    }
}
