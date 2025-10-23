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

    private PlayerStats playerStats;

    protected override void Awake()
    {
        base.Awake();
        playerStats = FindObjectOfType<PlayerStats>();
    }

    protected override void OnAttack()
    {
        if (!firePoint || !projectilePrefab)
        {
            Debug.LogWarning($"⚠️ {name}: firePoint або projectilePrefab не заданий!");
            return;
        }

        int count = Mathf.Max(1, pellets);
        bool shotFired = false;

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = firePoint.rotation;

            // Розкид
            if (spreadDegrees > 0f)
            {
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                float pitch = Random.Range(-spreadDegrees * 0.25f, spreadDegrees * 0.25f);
                rot = Quaternion.Euler(firePoint.eulerAngles + new Vector3(pitch, yaw, 0f));
            }

            // Підрахунок фінального урону
            float finalDamage = damage;
            bool isCrit = false;

            if (playerStats)
            {
                isCrit = Random.value < playerStats.critChance / 100f;
                finalDamage = (damage + playerStats.damage) * (isCrit ? playerStats.critMultiplier : 1f);
            }

            // Створення кулі
            var p = Instantiate(projectilePrefab, firePoint.position, rot);
            if (p != null)
            {
                p.Init(finalDamage, rot * Vector3.forward, projectileSpeed, hitMask, gameObject);
                if (isCrit) p.MarkAsCrit();

                shotFired = true;

                // 🔹 Debug — покаже реальний урон кожного пострілу
                Debug.Log($"🔫 {name} постріл: {finalDamage:F1} dmg (crit: {isCrit})");
            }
        }

        if (shotFired)
        {
            PlayFireSound();
            AnimFire();
        }
    }
}
