using UnityEngine;
using System.Collections;

public class ElfController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Configurações de Movimento")]
    public float speed = 5f;
    public float jumpForce = 8f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float slideSpeed = 10f; // velocidade do impulso do slide
    public float slideDuration = 0.75f; // duração do slide

    private bool isGrounded = true;
    private bool isSliding = false;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        // Certifica-se de que os componentes necessários estão presentes
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Verifica se os componentes foram encontrados
        if (anim == null)
            Debug.LogError("Animator Component is missing on the ElfController GameObject.");
        if (rb == null)
            Debug.LogError("Rigidbody2D Component is missing on the ElfController GameObject.");
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxisRaw("Horizontal");

        // --- MOVIMENTO HORIZONTAL ---
        if (!isAttacking && !isSliding)
        {
            // Movimento normal com velocidade base
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }
        else if (isAttacking)
        {
            // Para o personagem horizontalmente durante ataque
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (isSliding)
        {
            // Durante o slide, a velocidade é controlada pela Coroutine SlideCoroutine
            // AQUI o movimento horizontal é mantido pelo SlideCoroutine até o final da duração.
            // A linha de código original: 
            // rb.linearVelocity = new Vector2(isSliding ? rb.linearVelocity.x : 0, rb.linearVelocity.y);
            // NÃO É NECESSÁRIA AQUI, pois a coroutine já define o valor exato, mas 
            // manteremos a estrutura da sua lógica original para evitar efeitos colaterais.
        }

        // --- ANIMAÇÃO DE CORRIDA ---
        anim.SetBool("isRunning", move != 0 && isGrounded && !isAttacking && !isSliding);

        // --- VIRAR SPRITE (não vira durante slide) ---
        if (move != 0 && !isSliding)
            GetComponent<SpriteRenderer>().flipX = move < 0;

        // --- PULO (bloqueado durante ataque ou slide) ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking && !isSliding)
        {
            Jump();
        }

        // --- SLIDE (inicia o impulso) ---
        // Input.GetKeyDown(KeyCode.LeftShift) é a tecla padrão de Shift esquerdo
        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isAttacking && !isSliding)
        {
            StartCoroutine(SlideCoroutine());
        }

        // --- ATAQUES (só no chão e sem ataque/slide em andamento) ---
        if (!isAttacking && isGrounded && !isSliding)
        {
            if (Input.GetKeyDown(KeyCode.Z)) StartCoroutine(Attack("Attack1"));
            else if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(Attack("Attack2"));
            else if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(Attack("Attack3"));
        }

        // --- ATUALIZAÇÕES DE ANIMAÇÃO ---
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Checa se está no chão com base no 'groundCheck' e 'groundLayer'
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    // Esta função era um método auxiliar que foi substituído pelo Coroutine, 
    // mas pode ser mantida para clareza
    void Slide(bool sliding)
    {
        anim.SetBool("isSliding", sliding);
        isSliding = sliding;
    }

    // Coroutine do slide com impulso e duração exata (0.75s)
    private IEnumerator SlideCoroutine()
    {
        isSliding = true;
        anim.SetBool("isSliding", true);

        float startTime = Time.time;
        // Determina a direção do slide com base para onde o sprite está virado
        float direction = GetComponent<SpriteRenderer>().flipX ? -1f : 1f;

        // Loop principal do impulso
        while (Time.time < startTime + slideDuration)
        {
            // Aplica a velocidade de slide no eixo X
            rb.linearVelocity = new Vector2(direction * slideSpeed, rb.linearVelocity.y);
            yield return null; // Espera o próximo frame
        }

        // Para o impulso horizontal no final do slide
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Reseta o estado
        isSliding = false;
        anim.SetBool("isSliding", false);
    }

    // Ataque controlado (duração baseada na duração da animação)
    private IEnumerator Attack(string attackName)
    {
        isAttacking = true;
        anim.SetTrigger(attackName);

        // A duração ideal deve ser o tamanho do clipe de animação
        // Atenção: Esta linha PODE retornar 0 se o Animator não estiver no estado ainda
        float attackDuration = anim.GetCurrentAnimatorStateInfo(0).length;

        // Uma abordagem mais robusta é usar um tempo fixo (e.g. 0.5f) ou
        // um evento de animação (Animation Event) no final do clipe.
        // Mantendo a sua implementação original:
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    // Métodos públicos para serem chamados por outros scripts (e.g., Hitbox/Collision)
    public void TakeHit()
    {
        anim.SetTrigger("TakeHit");
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true; // Impede que o Rigidbody seja movido pela física
        GetComponent<Collider2D>().enabled = false; // Desativa a colisão
    }

    // Desenha um Gizmo para visualizar o Ground Check no editor da Unity
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}