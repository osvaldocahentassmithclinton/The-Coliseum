using UnityEngine;

public class KnightController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f; // 🔽 Gravidade personalizada

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravityScale; // 🧲 Aplica gravidade personalizada
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movimento lateral
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Flip do sprite
        if (moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;

        // Pulo
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.Play("Jump");
        }

        // Ataque
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            isAttacking = true;
            animator.Play("Attack1");
        }

        // Atualiza animações
        UpdateAnimations(moveInput);
    }

    void UpdateAnimations(float moveInput)
    {
        if (!isGrounded)
        {
            animator.Play("Fall");
        }
        else if (isAttacking)
        {
            // Ataque já foi iniciado
        }
        else if (moveInput != 0)
        {
            animator.Play("Run");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            isGrounded = false;
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}
