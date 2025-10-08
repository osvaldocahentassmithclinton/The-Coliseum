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

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float timezin;
    private float verticalVelocity;
    private bool isGrounded = false;
    public bool isAttacking = false;
    private bool isJumping = false;
    private int comboStep = 0;

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
    void Attack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
        Debug.Log("Aiaiaia");

        if (timezin < 1.5f)
        {
            comboStep++;
            Debug.Log(timezin);
        }
           
        else
        {
            comboStep = 1;
        }
       
        animator.SetBool("isAttacking", true);

        if (comboStep > 2)
        {
            comboStep = 1;
            Debug.Log("Segundo");
        }

        timezin = 0f;
        StartCoroutine(EndAttackAfterDelay(0.7f));
        Debug.Log("Aquiaa");

    }

    IEnumerator EndAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        animator.SetBool("isAttacking", false); 
        Debug.Log("Golpe final");
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
    
