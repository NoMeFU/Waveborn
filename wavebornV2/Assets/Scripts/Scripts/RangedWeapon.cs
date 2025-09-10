using UnityEngine;

public class RangedWeapon : WeaponBase
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 28f;
    [SerializeField] private int pellets = 1;
    [SerializeField] private float spreadDegrees = 0f;
    [SerializeField] private LayerMask hitMask;

    protected override void OnAttack()
    {
        if (!firePoint || !projectilePrefab) return;

        for (int i = 0; i < Mathf.Max(1, pellets); i++)
        {
            Quaternion spreadRot = firePoint.rotation;
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                float pitch = Random.Range(-spreadDegrees * 0.25f, spreadDegrees * 0.25f);
                spreadRot = Quaternion.Euler(firePoint.eulerAngles + new Vector3(pitch, yaw, 0f));
            }

            var p = Instantiate(projectilePrefab, firePoint.position, spreadRot);
            p.Init(damage, spreadRot * Vector3.forward, projectileSpeed, hitMask);
        }
    }
}
