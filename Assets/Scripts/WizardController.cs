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
    public float shieldBreakCooldown = 10f;

    [Tooltip("Se true, o state 'Shield' será pausado no último frame enquanto segurar.")]
    public bool freezeShieldAtEnd = true;

    [Header("Referências (Shield e Sliders)")]
    [SerializeField] private GameObject shieldObject;     // Shield child (visual + collider + ShieldObject.cs)
    [SerializeField] private string player1LayerName = "Player1"; // nome da layer que representa player 1
    [SerializeField] private string player2LayerName = "Player2"; // nome da layer que representa player 2
    [SerializeField] private Slider manaSlider_Player1;   // arraste o Slider do player 1 (UI)
    [SerializeField] private Slider manaSlider_Player2;   // arraste o Slider do player 2 (UI)

    // runtime
    private Slider activeManaSlider; // slider que será atualizado por este Wizard
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

    private float currentMana;
    private int selfLayerID;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            Debug.LogWarning($"{name}: PlayerInput não encontrado. Usando fallback teclado para hold/release.");

        selfLayerID = gameObject.layer;
        rb.gravityScale = gravityScale;

        // localizar hitbox se não tiver sido atribuída
        if (hitbox == null)
        {
            Transform childTransform = transform.Find(hitboxChildName);
            if (childTransform != null) hitbox = childTransform.gameObject;
        }
        if (hitbox) hitbox.SetActive(false);

        // identificar a layer do GameObject e ativar o slider correspondente
        string layerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"{name} está na layer: {layerName}");

        if (layerName == player1LayerName && manaSlider_Player1 != null)
        {
            activeManaSlider = manaSlider_Player1;
            // opcional: deixar a barra do outro invisível
            if (manaSlider_Player2 != null) manaSlider_Player2.gameObject.SetActive(false);
        }
        else if (layerName == player2LayerName && manaSlider_Player2 != null)
        {
            activeManaSlider = manaSlider_Player2;
            if (manaSlider_Player1 != null) manaSlider_Player1.gameObject.SetActive(false);
        }
        else
        {
            // fallback: se só um slider estiver atribuído usa ele; senão avisa
            if (manaSlider_Player1 != null && manaSlider_Player2 == null)
                activeManaSlider = manaSlider_Player1;
            else if (manaSlider_Player2 != null && manaSlider_Player1 == null)
                activeManaSlider = manaSlider_Player2;
            else
                Debug.LogWarning($"{name}: Nenhum slider de mana apropriado encontrado para layer '{layerName}'. Cheque as referências.");
        }

        damageable.onHit += OnTakeHit;
        damageable.onDeath += OnDeath;

        currentMana = maxMana;
        UpdateManaUI();

        if (shieldObject != null) shieldObject.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        float moveInput = playerInput != null ? playerInput.GetHorizontal() : Input.GetAxisRaw("Horizontal");
        bool wantsToJump = playerInput != null ? playerInput.GetJumpDown() : Input.GetKey(KeyCode.Space);
        bool wantsToAttack = playerInput != null ? playerInput.GetAction1Down() : Input.GetMouseButtonDown(0);

        // START do shield (pressionamento) - usa GetDodgeDown() do PlayerInput como você pediu
        bool wantsToShieldDown = playerInput != null ? playerInput.GetDodgeDown() : Input.GetKeyDown(KeyCode.LeftShift);

        // HOLD/RELEASE fallback para teclado (LeftShift). Se você usar InputManager, posso adaptar.
        bool wantsToShieldHold = Input.GetKey(KeyCode.LeftShift);
        bool wantsToShieldUp = Input.GetKeyUp(KeyCode.LeftShift);

        if (!isAttacking) timeSinceAttack += Time.deltaTime;

        // ataque
        if (wantsToAttack && isGrounded && !isShielding)
            Attack();

        // movimento bloqueado enquanto shielding (conforme seu pedido)
        if (!isAttacking && !isShielding)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (wantsToJump) Jump();

        // iniciar shield
        if (wantsToShieldDown && isGrounded && !isAttacking && canShield && !shieldOnCooldown && currentMana > 0f)
            StartShield();

        // lógica de hold: se estiver shielding, detecta release (fallback teclado)
        if (isShielding)
        {
            if (wantsToShieldUp)
            {
                StopShield();
            }
            else
            {
                // drenar mana enquanto segura
                float drain = manaDrainPerSecond * Time.deltaTime;
                currentMana -= drain;
                if (currentMana <= 0f)
                {
                    currentMana = 0f;
                    StopShield();
                }

                UpdateManaUI();

                // freeze no último frame do state "Shield"
                if (freezeShieldAtEnd && !shieldFrozenAtEnd)
                {
                    AnimatorStateInfo st = anim.GetCurrentAnimatorStateInfo(0);
                    if (st.IsName("Shield") && st.normalizedTime >= 1f)
                    {
                        anim.speed = 0f;
                        shieldFrozenAtEnd = true;
                    }
                }
            }
        }
        else
        {
            // regen de mana quando não atacando nem shielding
            if (!isAttacking && currentMana < maxMana)
            {
                currentMana += manaRegenPerSecond * Time.deltaTime;
                if (currentMana > maxMana) currentMana = maxMana;
                UpdateManaUI();
            }
        }

        if (moveInput != 0 && !isAttacking && !isShielding)
            sr.flipX = moveInput < 0;

        // Animator params
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
                if (tgt != null) tgt.TakeDamage(dmgValue);
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

    private void StartShield()
    {
        if (!canShield || shieldOnCooldown || currentMana <= 0f || isShielding) return;

        isShielding = true;
        anim.speed = 1f;
        shieldFrozenAtEnd = false;

        anim.SetTrigger("shieldTrigger");
        anim.SetBool("isShielding", true);

        if (damageable != null) damageable.SetInvulnerable(true);

        if (shieldObject != null) shieldObject.SetActive(true);
    }

    private void StopShield()
    {
        if (!isShielding) return;

        isShielding = false;
        anim.SetBool("isShielding", false);

        anim.speed = 1f;
        shieldFrozenAtEnd = false;

        if (damageable != null) damageable.SetInvulnerable(false);

        if (shieldObject != null) shieldObject.SetActive(false);
    }

    // chamado pelo ShieldObject quando quebrar
    public void OnShieldBroken()
    {
        StopShield();
        if (!shieldOnCooldown) StartCoroutine(ShieldBreakCooldownCoroutine(shieldBreakCooldown));
    }

    private IEnumerator ShieldBreakCooldownCoroutine(float cooldown)
    {
        shieldOnCooldown = true;
        canShield = false;
        yield return new WaitForSeconds(cooldown);
        shieldOnCooldown = false;
        canShield = true;
        if (shieldObject != null) shieldObject.SetActive(false);
    }

    private void UpdateManaUI()
    {
        if (activeManaSlider != null)
            activeManaSlider.value = currentMana / maxMana;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground")) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground")) isGrounded = false;
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
