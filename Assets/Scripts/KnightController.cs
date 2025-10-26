using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Damageable))]
public class KnightController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f;
    public float rollForce = 10f;
    public float rollDuration = 0.6f;

    [Header("Combate")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private string hitboxChildName = "AttackHitbox";

    [Header("Dano pós-movimento")]
    public float activeDamageDuration = 2f;
    private float lastMoveTime;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Damageable damageable;

    private float timeSinceAttack;
    private bool isGrounded;
    private bool isAttacking;
    private bool isRolling;
    private bool isDead;
    private int comboStep;

    private int selfLayerID;
    private bool hasHitThisActivation = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();

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
    }

    void Update()
    {
        if (isDead || isRolling) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        bool wantsToJump = Input.GetKey(KeyCode.Space);
        bool wantsToAttack = Input.GetMouseButtonDown(0);
        bool wantsToRoll = Input.GetKeyDown(KeyCode.LeftShift);

        if (!isAttacking) timeSinceAttack += Time.deltaTime;

        // Atualiza lastMoveTime se houver movimento
        if (moveInput != 0 || wantsToJump || wantsToRoll)
            lastMoveTime = Time.time;

        if (wantsToAttack && isGrounded && !isRolling) Attack();

        if (!isAttacking) rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        if (wantsToJump) Jump();
        if (wantsToRoll && isGrounded && !isAttacking) StartCoroutine(Roll());
        if (moveInput != 0 && !isAttacking) sr.flipX = moveInput < 0;

        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isRunning", moveInput != 0 && isGrounded && !isRolling && !isAttacking);
        anim.SetBool("isJumping", !isGrounded);
    }

    void Jump()
    {
        if (isGrounded && !isRolling)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }
    }

    void Attack()
    {
        if (isAttacking || isRolling) return;
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        comboStep = (timeSinceAttack < 1f) ? comboStep + 1 : 1;
        if (comboStep > 2) comboStep = 1;
        anim.SetInteger("comboStep", comboStep);
        timeSinceAttack = 0f;

        RotateHitbox(hitbox);
    }

    private void RotateHitbox(GameObject hb)
    {
        if (hb == null) return;
        Vector3 scale = hb.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (sr.flipX ? -1 : 1);
        hb.transform.localScale = scale;
    }

    public void EndAttack() => isAttacking = false;

    public void EnableHitbox()
    {
        if (hitbox != null)
        {
            hasHitThisActivation = false;
            hitbox.SetActive(true);
            StartCoroutine(ResetHitbox(hitbox));
        }
    }

    private IEnumerator ResetHitbox(GameObject hb)
    {
        yield return new WaitForSeconds(0.1f);
        hb.SetActive(false);
    }

    public void DisableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }

    private IEnumerator Roll()
    {
        isRolling = true;
        anim.SetBool("isRolling", true);

        float dir = sr.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(dir * rollForce, 0);

        yield return new WaitForSeconds(rollDuration);

        isRolling = false;
        anim.SetBool("isRolling", false);
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

    public bool CanTakeDamage() => Time.time - lastMoveTime <= activeDamageDuration;
}
