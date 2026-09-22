using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour
{
    private static readonly List<EnemyHealth> ActiveEnemiesInternal = new List<EnemyHealth>();

    [SerializeField] private int maxHealth = 30;
    [SerializeField] private bool isBoss;

    private int currentHealth;
    private Renderer cachedRenderer;
    private Color baseColor;

    public static IReadOnlyList<EnemyHealth> ActiveEnemies
    {
        get { return ActiveEnemiesInternal; }
    }

    public bool IsBoss
    {
        get { return isBoss; }
    }

    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    public float MaxHealth
    {
        get { return maxHealth; }
    }

    public void Initialize(int health, bool boss)
    {
        maxHealth = health;
        currentHealth = health;
        isBoss = boss;
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (cachedRenderer != null)
        {
            baseColor = cachedRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        if (!ActiveEnemiesInternal.Contains(this))
        {
            ActiveEnemiesInternal.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveEnemiesInternal.Remove(this);
    }

    public void ApplyDamage(int damage, SkillType sourceSkill)
    {
        currentHealth -= damage;
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = Color.white;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.08f);
        }

        SimpleEventBus.RaiseSkillHit(sourceSkill, gameObject);

        if (isBoss)
        {
            SimpleEventBus.RaiseBossHealthChanged(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void RestoreColor()
    {
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = baseColor;
        }
    }

    private void Die()
    {
        SimpleEventBus.RaiseEnemyKilled(this);
        Destroy(gameObject);
    }
}
