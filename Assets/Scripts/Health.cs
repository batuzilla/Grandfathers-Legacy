using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float Current { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float, float> OnChanged;
    public event Action OnDied;

    void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        Current = Mathf.Max(0f, Current - amount);
        OnChanged?.Invoke(Current, maxHealth);
        if (Current <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        OnChanged?.Invoke(Current, maxHealth);
    }
}