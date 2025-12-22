using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    /* ===================== COMBAT ===================== */
    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float attackRange = 1.2f;

    /* ===================== TARGET ===================== */
    [Header("Target")]
    [SerializeField] private Transform target; // Player

    /* ===================== NAVIGATION ===================== */
    [Header("Navigation")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float angularSpeed = 120f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float faceTurnSpeed = 720f;
    [SerializeField] private bool agentControlsRotation = true;

    /* ===================== ANIMATION ===================== */
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runParameterName = "IsRunning";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private float deathDelay = 3f;

    /* ===================== AUDIO ===================== */
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private float runSoundVolume = 0.3f;

    /* ===================== PRIVATE ===================== */
    private NavMeshAgent agent;
    private float attackTimer;
    private float repathTimer;
    private bool isDead;
    private bool isRunning;

    // Animator hashes
    private int runHash;
    private int attackHash;
    private int deathHash;
    private int speedHash;

    /* ===================== UNITY ===================== */

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (animator)
        {
            runHash = Animator.StringToHash(runParameterName);
            attackHash = Animator.StringToHash(attackTriggerName);
            deathHash = Animator.StringToHash(deathTriggerName);
            speedHash = Animator.StringToHash(speedParameterName);
        }

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (!target)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = attackRange * 0.9f;
        agent.updateRotation = agentControlsRotation;
    }

    private void OnEnable()
    {
        EnsureOnNavMesh();
        repathTimer = 0f;
    }

    private void Update()
    {
        if (isDead || !target || !agent.enabled) return;

        if (!agent.isOnNavMesh)
        {
            EnsureOnNavMesh();
            return;
        }

        attackTimer -= Time.deltaTime;
        repathTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            ChaseTarget();
        }
        else
        {
            AttackTarget();
        }

        UpdateSpeedParameter();
    }

    /* ===================== LOGIC ===================== */

    private void ChaseTarget()
    {
        agent.isStopped = false;

        if (repathTimer <= 0f)
        {
            agent.SetDestination(target.position);
            repathTimer = repathInterval;
        }

        SetRunning(true);
    }

    private void AttackTarget()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        SetRunning(false);

        RotateToTarget();

        if (attackTimer <= 0f)
        {
            animator.SetTrigger(attackHash);

            if (target.TryGetComponent(out Health hp))
                hp.TakeDamage(damage);

            PlaySound(attackSound);
            attackTimer = attackCooldown;
        }
    }

    private void RotateToTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            look,
            faceTurnSpeed * Time.deltaTime
        );
    }

    private void SetRunning(bool running)
    {
        if (!animator) return;

        animator.SetBool(runHash, running);

        if (running && !isRunning)
            PlayRunSound();
        else if (!running && isRunning)
            StopRunSound();

        isRunning = running;
    }

    private void UpdateSpeedParameter()
    {
        if (!animator) return;

        float speed01 = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(speedHash, speed01);
    }

    /* ===================== DEATH ===================== */

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        agent.isStopped = true;
        agent.enabled = false;

        SetRunning(false);
        animator.SetTrigger(deathHash);
        PlaySound(deathSound);

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        Destroy(gameObject, deathDelay);
    }

    /* ===================== NAVMESH ===================== */

    private void EnsureOnNavMesh()
    {
        if (agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name} not on NavMesh");
        }
    }

    /* ===================== AUDIO ===================== */

    private void PlaySound(AudioClip clip)
    {
        if (!audioSource || !clip) return;
        audioSource.PlayOneShot(clip);
    }

    private void PlayRunSound()
    {
        if (!runSound) return;

        audioSource.clip = runSound;
        audioSource.loop = true;
        audioSource.volume = runSoundVolume;
        audioSource.Play();
    }

    private void StopRunSound()
    {
        if (audioSource.isPlaying && audioSource.clip == runSound)
            audioSource.Stop();
    }

    /* ===================== GIZMOS ===================== */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
