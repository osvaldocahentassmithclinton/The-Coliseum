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
    public bool freezeShieldAtEnd = true;

    [Header("Magia")]
    public GameObject magicPrefab;
    public Transform magicSpawnPoint;
    public float magicManaCost = 30f;

    [Header("Controles por jogador")]
    public KeyCode player1ShieldKey = KeyCode.LeftShift;
    public KeyCode player2ShieldKey = KeyCode.Keypad0;

    [Header("Referências automáticas")]
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private string player1LayerName = "Player1";
    [SerializeField] private string player2LayerName = "Player2";

    private Slider activeManaSlider;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Damageable damageable;
    private PlayerInput playerInput;

    private float currentMana;
    private float timeSinceAttack;
    private bool isGrounded;
    private bool isAttacking;
    private bool isShielding;
    private bool isDead;
    private int comboStep;

    private bool canShield = true;
    private bool shieldOnCooldown = false;
    private bool shieldFrozenAtEnd = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
        playerInput = GetComponent<PlayerInput>();

        rb.gravityScale = gravityScale;

        // Detectar layer
        string layerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"{name} está na layer: {layerName}");

        // Procurar sliders na cena
        Slider slider1 = GameObject.Find("ManaBar_Player1")?.GetComponent<Slider>();
        Slider slider2 = GameObject.Find("ManaBar_Player2")?.GetComponent<Slider>();

        if (layerName == player1LayerName && slider1 != null)
        {
            activeManaSlider = slider1;
            if (slider2 != null) slider2.gameObject.SetActive(false);
        }
        else if (layerName == player2LayerName && slider2 != null)
        {
            activeManaSlider = slider2;
            if (slider1 != null) slider1.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{name}: Nenhum slider de mana encontrado para layer '{layerName}'.");
        }

        // Configuração da hitbox
        if (hitbox == null)
        {
            Transform childTransform = transform.Find(hitboxChildName);
            if (childTransform != null) hitbox = childTransform.gameObject;
        }
        if (hitbox) hitbox.SetActive(false);

        damageable.onHit += OnTakeHit;
        damageable.onDeath += OnDeath;

        currentMana = maxMana;
        UpdateManaUI();

        if (shieldObject != null)
            shieldObject.SetActive(false);
    }

    // 🔹 Funções de tecla do escudo (Player1 vs Player2)
    private KeyCode GetShieldKey()
    {
        string layerName = LayerMask.LayerToName(gameObject.layer);
        return (layerName == player2LayerName) ? player2ShieldKey : player1ShieldKey;
    }

    private bool WantsDodgeDown() => Input.GetKeyDown(GetShieldKey());
    private bool WantsDodgeHold() => Input.GetKey(GetShieldKey());
    private bool WantsDodgeUp() => Input.GetKeyUp(GetShieldKey());

    void Update()
    {
        if (isDead) return;

        float moveInput = playerInput != null ? playerInput.GetHorizontal() : Input.GetAxisRaw("Horizontal");
        bool wantsToJump = playerInput != null ? playerInput.GetJumpDown() : Input.GetKey(KeyCode.Space);
        bool wantsToAttack = playerInput != null ? playerInput.GetAction1Down() : Input.GetMouseButtonDown(0);
        bool wantsToCastMagic = playerInput != null ? playerInput.GetAction2Down() : Input.GetKeyDown(KeyCode.X);

        if (wantsToCastMagic && currentMana >= magicManaCost && !isShielding && !isAttacking && isGrounded)
            CastMagic();
        bool wantsToShieldDown = WantsDodgeDown();
        bool wantsToShieldHold = WantsDodgeHold();
        bool wantsToShieldUp = WantsDodgeUp();

        if (!isAttacking)
            timeSinceAttack += Time.deltaTime;

        if (wantsToAttack && isGrounded && !isShielding)
            Attack();

        if (!isAttacking && !isShielding)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (wantsToJump)
            Jump();

        // Iniciar shield
        if (wantsToShieldDown && isGrounded && !isAttacking && canShield && !shieldOnCooldown && currentMana > 0f)
            StartShield();

        // Segurar / soltar shield
        if (isShielding)
        {
            if (!wantsToShieldHold || wantsToShieldUp)
            {
                StopShield();
            }
            else
            {
                float drain = manaDrainPerSecond * Time.deltaTime;
                currentMana -= drain;

                if (currentMana <= 0f)
                {
                    currentMana = 0f;
                    StopShield();
                }

                UpdateManaUI();

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
            if (!isAttacking && currentMana < maxMana)
            {
                currentMana += manaRegenPerSecond * Time.deltaTime;
                if (currentMana > maxMana) currentMana = maxMana;
                UpdateManaUI();
            }
        }

        if (moveInput != 0 && !isAttacking && !isShielding)
            sr.flipX = moveInput < 0;

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
        yield return new WaitForSeconds(cooldown);
        shieldOnCooldown = false;
        canShield = true;
        if (shieldObject != null)
            shieldObject.SetActive(false);
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
    private void CastMagic()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetTrigger("CastMagic");

        currentMana -= magicManaCost;
        UpdateManaUI();

        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;
        GameObject magic = Instantiate(magicPrefab, magicSpawnPoint.position, Quaternion.identity);
        magic.GetComponent<MagicProjectile>().Initialize(direction, gameObject);

    }
    public void SpawnMagicProjectile()
    {
        if (currentMana < magicManaCost) return;

        currentMana -= magicManaCost;
        UpdateManaUI();

        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;
        GameObject magic = Instantiate(magicPrefab, magicSpawnPoint.position, Quaternion.identity);
        magic.GetComponent<MagicProjectile>().Initialize(direction, gameObject);

    }

}
