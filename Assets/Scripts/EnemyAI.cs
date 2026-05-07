using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Dark Souls–style enemy AI.
/// States: Idle → Chase → Attack → Dead
///
/// SETUP REQUIRED IN UNITY EDITOR:
///   1. Window > AI > Navigation > Bake the NavMesh on your scene geometry.
///   2. Add a NavMeshAgent component to the golem GameObject and tune:
///        Radius, Height, Speed, Stopping Distance to match the model scale.
///   3. Assign the golem's Animator and ensure it has these parameters:
///        "Speed"  (Float)   — 0 = idle, 1 = walking
///        "Attack" (Trigger) — plays the attack animation
///        "Hit"    (Trigger) — plays the hit-react animation
///        "Death"  (Trigger) — plays the death animation
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float loseAggroRadius = 22f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Combat")]
    [SerializeField] private float attackRange    = 2.5f;
    [SerializeField] private float attackCooldown = 2.2f;
    [SerializeField] private float attackDamage   = 20f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 6f;

    [Header("References")]
    [SerializeField] private Animator animator;

    // ── Animator hashes ──────────────────────────────────────────────
    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash    = Animator.StringToHash("Hit");
    private static readonly int DeathHash  = Animator.StringToHash("Death");

    // ── State ────────────────────────────────────────────────────────
    private enum State { Idle, Chase, Attack, Dead }
    private State _state = State.Idle;

    private NavMeshAgent _agent;
    private Transform    _player;
    private float        _attackTimer;
    private float        _distToPlayer;

    // ─────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = chaseSpeed;

        // auto-find animator on self or children if not assigned
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        _attackTimer -= Time.deltaTime;

        CachePlayerDistance();

        switch (_state)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────
    #region State Updates

    private void UpdateIdle()
    {
        SetAnimSpeed(0f);

        if (PlayerInRange(detectionRadius))
            EnterChase();
    }

    private void UpdateChase()
    {
        if (!PlayerInRange(loseAggroRadius))
        {
            EnterIdle();
            return;
        }

        if (PlayerInRange(attackRange))
        {
            EnterAttack();
            return;
        }

        // move toward player
        _agent.SetDestination(_player.position);
        _agent.isStopped = false;
        SetAnimSpeed(1f);

        // smooth rotation to face player while chasing
        FacePlayer();
    }

    private void UpdateAttack()
    {
        _agent.isStopped = true;
        SetAnimSpeed(0f);
        FacePlayer();

        // if player backed out of range go back to chasing
        if (!PlayerInRange(attackRange + 1f))
        {
            EnterChase();
            return;
        }

        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            animator?.SetTrigger(AttackHash);
        }
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────
    #region State Transitions

    private void EnterIdle()
    {
        _state = State.Idle;
        _agent.isStopped = true;
        SetAnimSpeed(0f);
    }

    private void EnterChase()
    {
        _state = State.Chase;
        _agent.isStopped = false;
    }

    private void EnterAttack()
    {
        _state = State.Attack;
        _agent.isStopped = true;
    }

    // called by EnemyHealth when a hit is received
    public void OnHit()
    {
        if (_state == State.Dead) return;
        animator?.SetTrigger(HitHash);

        // if the enemy wasn't chasing yet, aggro on hit
        if (_state == State.Idle)
            EnterChase();
    }

    // called by EnemyHealth when health reaches zero
    public void OnDeath()
    {
        _state = State.Dead;
        _agent.isStopped = true;
        _agent.enabled   = false;
        animator?.SetTrigger(DeathHash);

        // disable collider so the corpse doesn't block arrows
        foreach (var col in GetComponents<Collider>())
            col.enabled = false;
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────
    #region Helpers

    private void CachePlayerDistance()
    {
        if (_player == null) return;
        _distToPlayer = Vector3.Distance(transform.position, _player.position);
    }

    private bool PlayerInRange(float radius)
    {
        // try to find player the first time
        if (_player == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, playerLayer);
            if (hits.Length > 0)
            {
                _player = hits[0].transform;
                return true;
            }
            return false;
        }

        return _distToPlayer <= radius;
    }

    private void FacePlayer()
    {
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }

    private void SetAnimSpeed(float speed)
    {
        animator?.SetFloat(SpeedHash, speed);
    }

    #endregion
    // ─────────────────────────────────────────────────────────────────
    #region Editor Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, loseAggroRadius);
    }

    #endregion
}
