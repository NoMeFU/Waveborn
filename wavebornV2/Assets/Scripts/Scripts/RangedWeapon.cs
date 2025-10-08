using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [Header("Firing")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 28f;
    [SerializeField] private int pellets = 1;
    [SerializeField] private float spreadDegrees = 0f;
    [SerializeField] private LayerMask hitMask;

    protected override void OnAttack()
    {
        if (!firePoint || !projectilePrefab) return;


        int count = Mathf.Max(1, pellets);
        for (int i = 0; i < count; i++)
        {
            Quaternion rot = firePoint.rotation;
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                float pitch = Random.Range(-spreadDegrees * 0.25f, spreadDegrees * 0.25f);
                rot = Quaternion.Euler(firePoint.eulerAngles + new Vector3(pitch, yaw, 0f));
            }

            var p = Instantiate(projectilePrefab, firePoint.position, rot);
            p.Init(damage, rot * Vector3.forward, projectileSpeed, hitMask);
        }
    }

    public override void AnimFire()
    {
    
    }
}
