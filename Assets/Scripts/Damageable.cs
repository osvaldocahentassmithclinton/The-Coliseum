using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Damageable : MonoBehaviour
{
    public Action onDeath;
    public Action<float> onHit;

    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("Invulnerabilidade")]
    [Tooltip("Tempo em segundos que fica invulnerável após receber dano")]
    public float invulnerabilityDuration = 1.2f;

    [Tooltip("Faz o sprite piscar enquanto invulnerável")]
    public bool flashWhileInvulnerable = true;

    [Header("Referências")]
    public GameObject opponent;

    private Animator anim;
    private SpriteRenderer sr;
    private bool isInvulnerable = false;
    private Coroutine invulCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) Debug.LogWarning("Damageable: SpriteRenderer não encontrado!");
    }

    public void TakeDamage(float dmg)
    {
        if (IsDead) return;
        if (isInvulnerable) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"{gameObject.name} recebeu {dmg} de dano! Vida restante: {currentHealth}");

        onHit?.Invoke(dmg);

        if (invulCoroutine != null) StopCoroutine(invulCoroutine);
        invulCoroutine = StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));

        if (currentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"{gameObject.name} morreu!");
        onDeath?.Invoke();

        if (anim != null)
            anim.SetTrigger("Death");

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    // ===========================
    // COROUTINE DE INVULNERABILIDADE ATUALIZADA
    // ===========================
    private IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;

        if (flashWhileInvulnerable && sr != null)
        {
            Color original = sr.color;
            Color flashColor = Color.white;

            float timer = 0f;
            float flashInterval = 0.1f; // tempo entre piscadas

            while (timer < duration)
            {
                sr.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                sr.color = original;
                yield return new WaitForSeconds(flashInterval);

                timer += flashInterval * 2f;
            }

            sr.color = original;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        isInvulnerable = false;
        invulCoroutine = null;
    }

    public void SetInvulnerable(bool v)
    {
        if (v)
        {
            if (invulCoroutine != null) StopCoroutine(invulCoroutine);
            isInvulnerable = true;
        }
        else
        {
            isInvulnerable = false;
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(1f, value);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
}
