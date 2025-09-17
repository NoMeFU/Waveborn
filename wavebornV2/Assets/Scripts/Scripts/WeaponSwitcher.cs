using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private List<WeaponBase> weapons = new();
    [SerializeField] private int startIndex = 0;
    [SerializeField] private Transform weaponRoot;           // точка кріплення (WeaponRig)
    [Header("Drop")]
    [SerializeField] private GameObject weaponPickupPrefab;  // префаб пікапа
    [SerializeField] private float dropForce = 4f;

    public WeaponBase Current { get; private set; }

    private void Awake()
    {
        // Активуємо лише стартову
        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == startIndex);

        if (weapons.Count > 0)
        {
            Current = weapons[startIndex];
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

    // ---------- ПІДБІР: додаємо зброю з префаба ----------
    public void AddWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (!weaponPrefab || !weaponRoot) return;

        GameObject inst = Instantiate(weaponPrefab, weaponRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        var wb = inst.GetComponent<WeaponBase>() ?? inst.GetComponentInChildren<WeaponBase>();
        if (!wb) { Debug.LogWarning("Added weapon prefab has no WeaponBase."); Destroy(inst); return; }

        wb.SetPrefabReference(weaponPrefab); // запам'ятати джерело
        weapons.Add(wb);
        SelectIndex(weapons.Count - 1);
    }

    // ---------- СКИДАННЯ: робимо пікап і видаляємо з рук ----------
    public void DropCurrent()
    {
        if (!Current) return;
        if (!weaponPickupPrefab) { Debug.LogWarning("WeaponSwitcher: weaponPickupPrefab is not set."); return; }

        GameObject prefab = Current.WeaponPrefab;
        if (!prefab)
        {
            Debug.LogWarning("WeaponSwitcher: Current weapon has no WeaponPrefab reference.");
            return;
        }

        // позиція дропу: перед гравцем, прилипнути до землі якщо є шар Ground
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

        // прибираємо зброю з рук/списку
        int idx = weapons.IndexOf(Current);
        Destroy(Current.gameObject);
        weapons.RemoveAt(idx);

        if (weapons.Count > 0) SelectIndex(Mathf.Clamp(idx, 0, weapons.Count - 1));
        else Current = null;
    }
}
