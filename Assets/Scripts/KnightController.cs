using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

public class KnightController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float jumpForce = 25f;
    public float gravityScale = 5f;
    public float life = 30f;  

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public GameObject hitbox;

    private float timezin;
    private float verticalVelocity;
    private bool isGrounded = false;
    public bool isAttacking = false;
    private bool isJumping = false;
    private int comboStep = 0;



    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
            animator.SetBool("isGrounded", false);
        }
    }
    void Attack()
    {
        if (isAttacking) return; // impede ataque se já está no meio de outro

        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);

        // verifica se está dentro da janela de 1 segundo para continuar combo
        if (timezin < 1.0f)
        {
            comboStep++;
        }
        else
        {
            comboStep = 1; // reinicia combo
        }

        if (comboStep > 2)
        {
            comboStep = 1;
        }

        animator.SetBool("isAttacking", true);
        animator.SetInteger("comboStep", comboStep);

        timezin = 0f; // zera contador de tempo
        Debug.Log("Atacando passo " + comboStep);
    }

    // chamado via Animation Event no último frame da animação
    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        Debug.Log("Fim do ataque");

        // se já passou de 1 segundo, reseta combo
        if (timezin > 1.0f)
        {
            comboStep = 0;
            animator.SetInteger("comboStep", 0);
        }
    }

    public void EnableHitbox()
    {
        hitbox.SetActive(true);
        Debug.Log("Hitbox ativada");
    }

    public void DisableHitbox()
    {
        hitbox.SetActive(false);
        Debug.Log("Hitbox desativada");
    }





    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravityScale;
    }

    


void Update()
    {
        Jump();
        // Impede qualquer movimento durante o ataque

       
       


        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movimento lateral
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Flip do sprite
        if (moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;

        


        // Ataque com botão esquerdo do mouse
        if (!isAttacking)
            timezin += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            Attack();
        }
       
        
        
       // if () {
       //     timezin = Time.deltaTime;
      //      if (timezin > 1.5) { 
          
        //    }
      //  }
        

        // Atualiza parâmetros do Animator
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isRunning", moveInput != 0 && isGrounded);
        animator.SetBool("isJumping", !isGrounded);
        verticalVelocity = rb.linearVelocity.y;
        animator.SetBool("isFalling", verticalVelocity < 0);
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
        animator.SetInteger("comboStep", comboStep);
        


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


    
    
}
    
