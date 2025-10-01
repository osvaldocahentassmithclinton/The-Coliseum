using UnityEngine;

public class KnightController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float verticalVelocity;
    private bool isGrounded = false;
    private bool isAttacking = false;
    private bool isJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravityScale;
    }

     void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
            animator.SetBool("isGrounded", false);
        }
    }
    void Attack() {
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
    }


    void Update()
    {
        Attack();
        Jump();
        // Impede qualquer movimento durante o ataque

        if (isJumping == true && verticalVelocity < 0) { 
        
        }
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movimento lateral
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Flip do sprite
        if (moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;

        // Pulo
        

        // Ataque com botão esquerdo do mouse
        if (Input.GetMouseButtonDown(0) && !isAttacking && isGrounded)
        {
            isAttacking = true;
            animator.SetBool("isAttacking", true);
        }
        if (isJumping == true && isGrounded) { 
        }
        EndAttack();
        

        // Atualiza parâmetros do Animator
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRunning", moveInput != 0 && isGrounded);
        animator.SetBool("isJumping", !isGrounded);
        verticalVelocity = rb.linearVelocity.y;
        animator.SetBool("isFalling", verticalVelocity < 0);
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);


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

    // Chamado via Animation Event no final da animação de ataque


    public void EndAttack()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            isAttacking = false;
            animator.SetBool("isAttacking", false);
            if(isAttacking == false)
            {
                Debug.Log("Ataque finalizando");
            }
        }
    }
    
}
    
