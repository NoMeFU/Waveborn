using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 4f; // постр./сек
    private float cooldown;

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


    protected abstract void OnAttack();
}
