using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    
    [SerializeField]
    private EnemyHealthUI healthUI;

    [Header("Events")]
    public UnityEvent onHit;
    public UnityEvent onDeath;

    private float _currentHealth;
    private bool _isDead;
    private EnemyAI _ai;

    public bool IsDead => _isDead;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _ai = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        _currentHealth  = Mathf.Max(_currentHealth, 0f);
        healthUI.UpdateHealth(_currentHealth,maxHealth);

        onHit.Invoke();
        _ai?.OnHit();

        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        onDeath.Invoke();
        _ai?.OnDeath();
    }

    // handy for a future UI health bar
    public float HealthPercent => _currentHealth / maxHealth;
}
