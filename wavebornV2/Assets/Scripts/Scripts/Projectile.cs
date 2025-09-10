using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.5f;
    [SerializeField] private float radius = 0.12f;
    [SerializeField] private int pierce = 1;

    private float damage;
    private Vector3 dir;
    private float speed;
    private float timer;
    private LayerMask mask;

    public void Init(float dmg, Vector3 direction, float spd, LayerMask hitMask)
    {
        damage = dmg;
        dir = direction.normalized;
        speed = spd;
        mask = hitMask;
        timer = lifeTime;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        Vector3 next = transform.position + dir * step;

        Collider[] hits = Physics.OverlapSphere(next, radius, mask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage);
                pierce--;
                if (pierce <= 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position = next;
        transform.forward = dir;
        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }
}
