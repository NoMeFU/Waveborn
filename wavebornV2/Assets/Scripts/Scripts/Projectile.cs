using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    private float damage;
    private Vector3 direction;
    private float speed;
    private LayerMask hitMask;
    private bool isCrit;
    private GameObject ownerRoot;
    private string ownerTag;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject critEffect;
    [SerializeField] private float lifeTime = 5f;

    private Collider myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider == null)
            myCollider = GetComponentInChildren<Collider>();

        myCollider.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(float dmg, Vector3 dir, float spd, LayerMask mask, GameObject shooter = null)
    {
        damage = dmg;
        direction = dir.normalized;
        speed = spd;
        hitMask = mask;

        if (shooter != null)
        {
            ownerRoot = shooter.transform.root.gameObject;
            ownerTag = ownerRoot.tag;

            Collider[] ownerCols = ownerRoot.GetComponentsInChildren<Collider>();
            foreach (var c in ownerCols)
            {
                if (c != null && myCollider != null)
                    Physics.IgnoreCollision(myCollider, c, true);
            }
        }
    }

    public void MarkAsCrit()
    {
        isCrit = true;
        if (critEffect)
        {
            var vfx = Instantiate(critEffect, transform.position, transform.rotation, transform);
            Destroy(vfx, 1.5f);
        }
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        // 🔸 ігноруємо власника
        if (ownerRoot && other.transform.root.gameObject == ownerRoot)
            return;

        // 🔸 якщо попали у ворога або об'єкт з Health
        if (other.TryGetComponent<Health>(out var hp))
        {
            if (ownerRoot == null || other.transform.root != ownerRoot.transform)
            {
                float oldHp = hp.CurrentHP;
                hp.TakeDamage(damage);
                float newHp = hp.CurrentHP;

                // 🧩 ДЕБАГ у консоль
                string critText = isCrit ? "<color=yellow>[CRIT]</color>" : "";
                Debug.Log(
                    $"<color=#FF5555>☄️ HIT → {other.name}</color> {critText}\n" +
                    $"<b>Damage:</b> {damage}\n" +
                    $"<b>HP:</b> {oldHp:0.0} → {newHp:0.0} / {hp.MaxHP:0.0}"
                );

                if (hitEffect)
                    Instantiate(hitEffect, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
            return;
        }

        // 🔸 якщо попали у стіну чи інший об'єкт
        if (((1 << other.gameObject.layer) & hitMask) != 0)
        {
            if (hitEffect)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
