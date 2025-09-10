using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private Transform target; // Player

    private NavMeshAgent agent;
    private float cd;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        // Безпечні дефолти
        agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.9f);
        agent.updateRotation = true;   // ворог сам крутиться за напрямком
        agent.updatePosition = true;
    }

    private void OnEnable()
    {
        EnsureOnNavMesh();
    }

    private void Update()
    {
        if (!target) return;

        if (!agent.enabled)
        {
            Debug.LogWarning($"{name}: NavMeshAgent disabled");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            // Спробуємо разово виправити
            EnsureOnNavMesh();
            if (!agent.isOnNavMesh) return;
        }

        // Рух до цілі
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Атака
        cd -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange && cd <= 0f)
        {
            if (target.TryGetComponent<Health>(out var hp))
                hp.TakeDamage(damage);
            cd = attackCooldown;

            // Розвернутися в бік гравця, щоб не “бив спиною”
            Vector3 to = (target.position - transform.position);
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(to);
        }
    }

    private void EnsureOnNavMesh()
    {
        if (agent.isOnNavMesh) return;

        // Знайти найближчу точку навмешу поруч і телепортувати
        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
        {
            bool warped = agent.Warp(hit.position);
            if (!warped)
            {
                // як запасний варіант — тимчасово відключити/включити агент
                agent.enabled = false;
                transform.position = hit.position;
                agent.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning($"{name}: No NavMesh nearby — place enemy on baked mesh.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
