using UnityEngine;
using System;
using System.Collections;

public class Damageable : MonoBehaviour
{
    public Action onDeath;
    public Action<float> onHit;

    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Invulnerabilidade")]
    public float invulnerabilityDuration = 1.2f; // 1.0 - 1.5s recomendado
    private bool isInvulnerable = false;

    [Header("Refer�ncias")]
    public GameObject opponent; // usado pelo GameManager (mantenha)

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Usamos OnTriggerEnter2D para detectar ataques quando a hitbox/proj�til entra em contato
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Procura DamageDealer (hitbox ou proj�til)
        DamageDealer dmgDealer = other.GetComponent<DamageDealer>();
        if (dmgDealer == null) return;

        // Friendly fire: ignora se mesma layer
        if (other.gameObject.layer == gameObject.layer) return;

        // Se estamos invulner�veis, n�o aplicamos dano agora
        if (isInvulnerable) return;

        // Aplica dano
        TakeDamage(dmgDealer.damage);

        // Para proj�teis/objetos que devam se desativar ao acertar, deixe que
        // o pr�prio script do proj�til (ex: ProjectileArrow) trate do colisor.
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        currentHealth -= dmg;
        Debug.Log($"{gameObject.name} recebeu {dmg} de dano! Vida restante: {currentHealth}");

        // Notifica listeners
        if (onHit != null) onHit(dmg);

        // Come�a invulnerabilidade + piscada
        StartCoroutine(InvulnerabilityCoroutine());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // piscagem simples: alterna alfa entre 1 e 0.25 v�rias vezes
        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            float blinkInterval = 0.12f;
            Color original = spriteRenderer.color;
            while (elapsed < invulnerabilityDuration)
            {
                // diminuir alpha
                spriteRenderer.color = new Color(original.r, original.g, original.b, 0.25f);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;

                // restaurar alpha
                spriteRenderer.color = new Color(original.r, original.g, original.b, 1f);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }
            // restaura cor original
            spriteRenderer.color = original;
        }
        else
        {
            // fallback: s� espera o tempo
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} morreu!");

        if (onDeath != null) onDeath();

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
}
