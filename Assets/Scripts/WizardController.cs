using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Damageable))]
public class WizardController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f;

    [Header("Combate")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private string hitboxChildName = "AttackHitbox";

    [Header("Defesa / Mana")]
    public float maxMana = 100f;
    public float manaDrainPerSecond = 25f;
    public float manaRegenPerSecond = 15f;
    public float shieldBreakCooldown = 10f; // tempo sem poder usar shield após quebrar

    [Tooltip("Se true, o estado de animação 'Shield' será pausado no último frame enquanto segurar.")]
    public bool freezeShieldAtEnd = true; // <-- nome consistente

    [Header("Referências (atribuir no Inspector)")]
    [SerializeField] private Slider manaSlider;        // opcional: arraste o Slider da UI
    [SerializeField] private GameObject shieldObject;  // filho que representa o escudo (com Collider2D e ShieldObject.cs)

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Damageable damageable;
    private PlayerInput playerInput;

    private float timeSinceAttack;
    private bool isGrounded;
    private bool isAttacking;
    private bool isShielding;

    private bool isDead;
    private int comboStep;
    private bool canShield = true;
    private bool shieldOnCooldown = false;
    private bool shieldFrozenAtEnd = false;

    // mana
    private float currentMana;

    // use same layer id check as before (preserve lógica existente)
    private int selfLayerID;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            Debug.LogWarning($"{name}: PlayerInput não encontrado. Fallback para Input.GetKey(...) (teclado).");

        selfLayerID = gameObject.layer;
        rb.gravityScale = gravityScale;

        if (hitbox == null)
        {
            Transform childTransform = transform.Find(hitboxChildName);
            if (childTransform != null)
                hitbox = childTransform.gameObject;
        }

        if (hitbox) hitbox.SetActive(false);

        damageable.onHit += OnTakeHit;
        damageable.onDeath += OnDeath;

        currentMana = maxMana;
        UpdateManaUI();

        // deixe shieldObject desativado por padrão (visual)
        if (shieldObject != null) shieldObject.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        // inputs
        float moveInput = playerInput != null ? playerInput.GetHorizontal() : Input.GetAxisRaw("Horizontal");
        bool wantsToJump = playerInput != null ? playerInput.GetJumpDown() : Input.GetKey(KeyCode.Space);
        bool wantsToAttack = playerInput != null ? playerInput.GetAction1Down() : Input.GetMouseButtonDown(0);

        // START do shield: usamos GetDodgeDown() do playerInput (não alterado)
        bool wantsToShieldDown = playerInput != null ? playerInput.GetDodgeDown() : Input.GetKeyDown(KeyCode.LeftShift);
        // HOLD/RELEASE: fallback ao teclado (LeftShift).
        // Se você usa InputManager custom, adicione GetDodge() e GetDodgeUp() no PlayerInput para substituir esses checks.
        bool wantsToShieldHold = Input.GetKey(KeyCode.LeftShift);
        bool wantsToShieldUp = Input.GetKeyUp(KeyCode.LeftShift);

        if (!isAttacking)
            timeSinceAttack += Time.deltaTime;

        // Attack
        if (wantsToAttack && isGrounded && !isShielding)
            Attack();

        // Movement: não atualiza movimento enquanto shielding (bloqueio desejado)
        if (!isAttacking && !isShielding)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Jump
        if (wantsToJump)
            Jump();

        // Iniciar shield
        if (wantsToShieldDown && isGrounded && !isAttacking && canShield && !shieldOnCooldown && currentMana > 0f)
        {
            StartShield();
        }

        // Enquanto shield ativo: detectar release pelo teclado (fallback)
        if (isShielding)
        {
            // se detectou release (teclado), para o shield
            if (wantsToShieldUp)
            {
                StopShield();
            }
            else
            {
                // Drena mana enquanto estiver segurando
                float drain = manaDrainPerSecond * Time.deltaTime;
                currentMana -= drain;
                if (currentMana <= 0f)
                {
                    currentMana = 0f;
                    StopShield(); // sem mana, para o shield
                }

                UpdateManaUI();

                // freeze no último frame do clipe Shield (se configurado)
                if (freezeShieldAtEnd && !shieldFrozenAtEnd)
                {
                    AnimatorStateInfo st = anim.GetCurrentAnimatorStateInfo(0);
                    if (st.IsName("Shield")) // ATENÇÃO: se o seu state tiver outro nome, mude aqui
                    {
                        if (st.normalizedTime >= 1f)
                        {
                            anim.speed = 0f;
                            shieldFrozenAtEnd = true;
                        }
                    }
                }
            }
        }
        else
        {
            // regen de mana quando NÃO estiver atacando nem shielding
            if (!isAttacking)
            {
                if (currentMana < maxMana)
                {
                    currentMana += manaRegenPerSecond * Time.deltaTime;
                    if (currentMana > maxMana) currentMana = maxMana;
                    UpdateManaUI();
                }
            }
        }

        // flip do sprite (só quando não estiver shield/attack, para manter bloqueio visual)
        if (moveInput != 0 && !isAttacking && !isShielding)
            sr.flipX = moveInput < 0;

        // Animator params (sempre atualizar)
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isRunning", moveInput != 0 && isGrounded && !isShielding && !isAttacking);
        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isShielding", isShielding);
    }

    void Jump()
    {
        if (isGrounded && !isShielding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }
    }

    void Attack()
    {
        if (isAttacking || isShielding) return;

        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        comboStep = (timeSinceAttack < 1f) ? comboStep + 1 : 1;
        if (comboStep > 2) comboStep = 1;

        anim.SetInteger("comboStep", comboStep);
        timeSinceAttack = 0f;

        RotateHitbox(hitbox);
    }

    private void RotateHitbox(GameObject hitboxObject)
    {
        if (hitboxObject == null) return;

        Vector3 scale = hitboxObject.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (sr.flipX ? -1 : 1);
        hitboxObject.transform.localScale = scale;
    }

    public void EndAttack() => isAttacking = false;

    public void EnableHitbox()
    {
        if (hitbox == null) return;

        Collider2D hbCol = hitbox.GetComponent<Collider2D>();
        DamageDealer dd = hitbox.GetComponent<DamageDealer>();
        float dmgValue = (dd != null) ? dd.damage : 10f;

        hitbox.SetActive(true);

        if (hbCol != null)
        {
            hbCol.enabled = true;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            Collider2D[] results = new Collider2D[10];
            int hits = hbCol.Overlap(filter, results);

            for (int i = 0; i < hits; i++)
            {
                Collider2D other = results[i];
                if (other == null || other.gameObject == gameObject || other.gameObject.layer == gameObject.layer) continue;

                Damageable tgt = other.GetComponent<Damageable>();
                if (tgt != null)
                    tgt.TakeDamage(dmgValue);
            }

            hbCol.enabled = false;
        }

        StartCoroutine(ResetHitbox(hitbox));
    }

    private IEnumerator ResetHitbox(GameObject hb)
    {
        yield return new WaitForSeconds(0.12f);
        if (hb != null)
        {
            Collider2D col = hb.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            hb.SetActive(false);
        }
    }

    public void DisableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }

    // Start shield
    private void StartShield()
    {
        if (!canShield || shieldOnCooldown || currentMana <= 0f || isShielding) return;

        isShielding = true;
        anim.speed = 1f;
        shieldFrozenAtEnd = false;

        // trigger/param para Animator
        anim.SetTrigger("shieldTrigger");
        anim.SetBool("isShielding", true);

        // invulnerável
        if (damageable != null)
            damageable.SetInvulnerable(true);

        // ativa shield object visual/colisor
        if (shieldObject != null)
            shieldObject.SetActive(true);
    }

    // Stop shield
    private void StopShield()
    {
        if (!isShielding) return;

        isShielding = false;
        anim.SetBool("isShielding", false);

        // restaurar animação
        anim.speed = 1f;
        shieldFrozenAtEnd = false;

        if (damageable != null)
            damageable.SetInvulnerable(false);

        // desativa shield object visual
        if (shieldObject != null)
            shieldObject.SetActive(false);
    }

    // Chamado pelo ShieldObject quando quebrar
    public void OnShieldBroken()
    {
        StopShield();
        if (!shieldOnCooldown)
            StartCoroutine(ShieldBreakCooldownCoroutine(shieldBreakCooldown));
    }

    private IEnumerator ShieldBreakCooldownCoroutine(float cooldown)
    {
        shieldOnCooldown = true;
        canShield = false;

        // desative visual local (shieldObject já foi desativado no Break)
        yield return new WaitForSeconds(cooldown);

        shieldOnCooldown = false;
        canShield = true;

        // quando reativar o shieldObject, o próprio ShieldObject reseta hits no OnEnable()
        if (shieldObject != null)
            shieldObject.SetActive(false); // deixamos desativado até o jogador pressionar
    }

    private void UpdateManaUI()
    {
        if (manaSlider != null)
            manaSlider.value = currentMana / maxMana;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = false;
    }

    private void OnTakeHit(float dmg)
    {
        if (isDead) return;
        anim.SetTrigger("TakeHit");
    }

    private void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        this.enabled = false;
    }
}
