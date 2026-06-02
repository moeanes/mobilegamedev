using UnityEngine;

// Enemy hit points. On death it tells the LevelManager (so the level can count kills)
// and removes itself.
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;
    private bool isDead;

    public bool IsDead => isDead;

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

        LevelManager.Instance?.NotifyEnemyKilled(this);
        Destroy(gameObject);
    }
}
