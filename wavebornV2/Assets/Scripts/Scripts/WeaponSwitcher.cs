using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private WeaponBase[] weapons;
    [SerializeField] private int startIndex = 0;

    public WeaponBase Current { get; private set; }

    private void Awake()
    {
        SelectIndex(startIndex);
    }

    public void SelectIndex(int idx)
    {
        if (weapons == null || weapons.Length == 0) return;
        idx = Mathf.Clamp(idx, 0, weapons.Length - 1);

        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == idx);

        Current = weapons[idx];

        // 🔊 звук взяття у руки (кожна зброя має свій)
        if (Current) Current.PlayEquipSound();
    }

    public void SelectNext(bool forward = true)
    {
        if (weapons == null || weapons.Length == 0) return;
        int cur = System.Array.IndexOf(weapons, Current);
        if (cur < 0) cur = 0;
        int next = (cur + (forward ? 1 : -1) + weapons.Length) % weapons.Length;
        SelectIndex(next);
    }
}
