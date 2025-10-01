using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Target")]
    [SerializeField] private Transform target; // Player

    [Header("Navigation")]
    [SerializeField] private float repathInterval = 0.2f; // як часто оновлювати ціль
    [SerializeField] private float faceTurnSpeed = 720f;  // град/с, коли стоїмо й б’ємо
    [SerializeField] private bool agentControlsRotation = true; // лишаємо true для стабільності

    private NavMeshAgent agent;
    private float cd;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        // Базові безпечні значення агента
        agent.stoppingDistance = Mathf.Max(0.05f, attackRange * 0.9f);
        agent.updateRotation = agentControlsRotation;
        agent.updatePosition = true;

        // Переконаємось, що не нульові параметри
        if (agent.speed <= 0.01f) agent.speed = 3.5f;
        if (agent.acceleration <= 0.01f) agent.acceleration = 8f;
        if (agent.angularSpeed <= 0.01f) agent.angularSpeed = 720f;
        if (agent.baseOffset < -1f || agent.baseOffset > 2f) agent.baseOffset = 0f;
    }

    private void OnEnable()
    {
        EnsureOnNavMesh();
        repathTimer = 0f;
    }

    private void Update()
    {
        if (!target || !agent || !agent.enabled) return;
        if (!agent.isOnNavMesh)
        {
            EnsureOnNavMesh();
            if (!agent.isOnNavMesh) return;
        }

        cd -= Time.deltaTime;
        repathTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, target.position);

        // 1) Переслідування за межами радіуса атаки
        if (dist > attackRange)
        {
            // Рухаємось до цілі; не зупиняємось
            agent.isStopped = false;

            // Оновлюємо ціль не кожен кадр (щоб не смикалось)
            if (repathTimer <= 0f || agent.remainingDistance <= agent.stoppingDistance || agent.pathPending)
            {
                agent.SetDestination(target.position);
                repathTimer = repathInterval;
            }
        }
        else
        {
            // 2) У радіусі атаки — зупиняємось і атакуємо по кулдауну
            agent.isStopped = true;

            if (!agentControlsRotation)
            {
                // М’яко повертаємося до цілі, якщо ми керуємо обертанням
                Vector3 dir = target.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion look = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, look, faceTurnSpeed * Time.deltaTime);
                }
            }

            if (cd <= 0f)
            {
                if (target.TryGetComponent<Health>(out var hp))
                    hp.TakeDamage(damage);

                cd = attackCooldown;
            }
        }
    }

    private void EnsureOnNavMesh()
    {
        // якщо агент не на NavMesh — знаходимо найближчу точку і телепортуємо
        if (agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
        {
            // Warp безпечніше ніж просто позицію ставити
            if (!agent.Warp(hit.position))
            {
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
