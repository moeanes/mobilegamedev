using System;
using UnityEngine;

// Enemy hit points. On death it tells the LevelManager (so the level can count kills)
// and removes itself. The boss reuses this component (so the doctor's bullets damage it
// the same way), but sets reportToLevelManager = false and listens to Died — so its death
// ends the level as a win instead of counting as one more kill.
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public bool reportToLevelManager = true;

    private int currentHealth;
    private bool isDead;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        if (reportToLevelManager)
        {
            LevelManager.Instance?.NotifyEnemyKilled(this);
        }

        Died?.Invoke();
        Destroy(gameObject);
    }
}
