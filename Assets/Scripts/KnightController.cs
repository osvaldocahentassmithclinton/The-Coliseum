using System;
using UnityEngine;
using System.Collections;

public class KnightController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f;
    public float life = 30f;
    public float rollForce = 10f;      // Força do impulso do rolamento
    public float rollDuration = 0.6f;  // Duração da rolagem

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public GameObject hitbox;

    private float timezin;
    private float verticalVelocity;
    private bool isGrounded = false;
    public bool isAttacking = false;
    private bool isJumping = false;
    private bool isRolling = false; // novo
    private bool canTakeDamage = true;
    private int comboStep = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        if (isRolling) return; // trava tudo durante o roll

        Jump();

        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movimento lateral (somente se não estiver atacando)
        if (!isAttacking)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Flip do sprite
        if (moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;

        // Ataque com botão esquerdo do mouse
        if (!isAttacking)
            timezin += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && isGrounded && !isRolling)
        {
            Attack();
        }

        // Rolamento (Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isAttacking && !isJumping)
        {
            StartCoroutine(Roll());
        }

        // Atualiza parâmetros do Animator
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRunning", moveInput != 0 && isGrounded && !isRolling);
        animator.SetBool("isJumping", !isGrounded);
        verticalVelocity = rb.linearVelocity.y;
        animator.SetBool("isFalling", verticalVelocity < 0);
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
        animator.SetInteger("comboStep", comboStep);
    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded && !isRolling)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
            animator.SetBool("isGrounded", false);
            isJumping = true;
        }
    }

    void Attack()
    {
        if (isAttacking || isRolling) return;

        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);

        if (timezin < 1.0f)
        {
            comboStep++;
        }
        else
        {
            comboStep = 1;
        }

        if (comboStep > 2)
            comboStep = 1;

        animator.SetBool("isAttacking", true);
        animator.SetInteger("comboStep", comboStep);
        timezin = 0f;

        Debug.Log("Atacando passo " + comboStep);
    }

    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        Debug.Log("Fim do ataque");

        if (timezin > 1.0f)
        {
            comboStep = 0;
            animator.SetInteger("comboStep", 0);
        }
    }

    public void EnableHitbox()
    {
        hitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        hitbox.SetActive(false);
    }

    // ---- ROLAMENTO ----
    private IEnumerator Roll()
    {
        isRolling = true;
        canTakeDamage = false;
        animator.SetBool("isRolling", true);

        float direction = spriteRenderer.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(direction * rollForce, 0);

        // Espera até a animação acabar ou até o tempo máximo
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        EndRoll();
    }


    public void EndRoll()
    {
        isRolling = false;
        canTakeDamage = true;
        animator.SetBool("isRolling", false);
        Debug.Log("Fim do rolamento");
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = false;
    }
}
