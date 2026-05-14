using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;

    private float currentHealth;

    [SerializeField] private PlayerHealthUI healthUI;
    [SerializeField] private Animator _animator;
    [SerializeField] private DamageOverlay damageOverlay;
    
    private static readonly int IsDamage = Animator.StringToHash("IsDamage");

    private void Start()
    {
        currentHealth = maxHealth;

        healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        _animator.SetTrigger(IsDamage);
        damageOverlay.ShowDamage();
        HitStop.Instance.Stop(0.03f);
        currentHealth -= damage;
        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);

        healthUI.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
    }
}