using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class Damageable : MonoBehaviour
{
    public Action onDeath;
    public Action<float> onHit;

    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Dano")]
    public float damageCooldown = 0.3f; // tempo entre hits
    private float lastDamageTime = -999f;

    [Header("Referências")]
    public GameObject opponent; // referência a outro personagem ou inimigo

    [Header("Vulnerabilidade pós-movimento")]
    public float vulnerableTimeAfterMove = 2f; // segundos que pode levar dano após se mover
    private float lastMoveTime = -999f;

    private Animator anim;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Atualiza lastMoveTime se o player se moveu
        var elf = GetComponent<ElfController>();
        if (elf != null && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)))
            lastMoveTime = Time.time;

        var knight = GetComponent<KnightController>();
        if (knight != null && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)))
            lastMoveTime = Time.time;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DamageDealer dmgDealer = other.GetComponent<DamageDealer>();
        if (dmgDealer == null || isDead) return;

        // Evita friendly fire
        if (other.gameObject.layer == gameObject.layer)
            return;

        // Aplica dano apenas se cooldown expirou
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        // Só leva dano se estiver dentro do tempo de vulnerabilidade após se mover
        if (Time.time - lastMoveTime > vulnerableTimeAfterMove)
            return;

        lastDamageTime = Time.time;
        TakeDamage(dmgDealer.damage);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        Debug.Log($"{gameObject.name} recebeu {dmg} de dano! Vida restante: {currentHealth}");

        onHit?.Invoke(dmg);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

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
}
