using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    [ReadOnly] [SerializeField] private int currentHealth;

    public Action OnPlayerHurt;
    public Action OnPlayerDied;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnPlayerHurt?.Invoke();
        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }
}
