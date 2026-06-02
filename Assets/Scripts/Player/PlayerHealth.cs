using System;
using UnityEngine;

// Player hit points with a short invulnerability window after each hit.
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invulnerabilityTime = 0.6f;

    private int currentHealth;
    private float lastDamageTime = -999f;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<int, int> HealthChanged;
    public event Action Damaged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead || Time.time < lastDamageTime + invulnerabilityTime)
        {
            return;
        }

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke();

        if (currentHealth == 0)
        {
            isDead = true;
            GameManager.Instance?.OnPlayerDied();
        }
    }
}
