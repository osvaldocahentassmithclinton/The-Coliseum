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
    public float slideSpeed = 10f;
    public float slideDuration = 0.75f;

    private bool isGrounded = true;
    private bool isSliding = false;
    private bool isDead = false;
    private bool isAttacking = false;

    [Header("Ataque e Hitbox")]
    public GameObject attack1Hitbox;
    public GameObject attack3Hitbox;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint; 
    public float projectileSpeed = 10f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            Debug.LogError("Animator Component is missing on the ElfController GameObject.");
        if (rb == null)
            Debug.LogError("Rigidbody2D Component is missing on the ElfController GameObject.");

        if (attack1Hitbox != null)
            attack1Hitbox.SetActive(false);
        if (attack3Hitbox != null)
            attack3Hitbox.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxisRaw("Horizontal");

        if (!isAttacking && !isSliding)
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }
        else if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        anim.SetBool("isRunning", move != 0 && isGrounded && !isAttacking && !isSliding);

        // Virar e spawnpoint
        if (move != 0 && !isSliding)
        {
            bool flip = move < 0;
            GetComponent<SpriteRenderer>().flipX = flip;

            // Faz o spawnpoint do projétil mudar de lado junto com o elfo
            if (projectileSpawnPoint != null)
            {
                Vector3 localPos = projectileSpawnPoint.localPosition;
                localPos.x = Mathf.Abs(localPos.x) * (flip ? -1 : 1);
                projectileSpawnPoint.localPosition = localPos;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking && !isSliding)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isAttacking && !isSliding)
        {
            StartCoroutine(SlideCoroutine());
        }

        // Ataques
        if (!isAttacking && isGrounded && !isSliding)
        {
            if (Input.GetKeyDown(KeyCode.Z)) StartCoroutine(Attack("Attack1"));
            else if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(Attack("Attack2"));
            else if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(Attack("Attack3"));
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    void Slide(bool sliding)
    {
        anim.SetBool("isSliding", sliding);
        isSliding = sliding;
    }

    private IEnumerator SlideCoroutine()
    {
        isSliding = true;
        anim.SetBool("isSliding", true);

        float startTime = Time.time;
        float direction = GetComponent<SpriteRenderer>().flipX ? -1f : 1f;

        while (Time.time < startTime + slideDuration)
        {
            rb.linearVelocity = new Vector2(direction * slideSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        isSliding = false;
        anim.SetBool("isSliding", false);
    }

    private IEnumerator Attack(string attackName)
    {
        // Vai pergar o nome do ataque e ativar a condição
        isAttacking = true;
        anim.SetTrigger(attackName);

        // Ataque 2 do projétil
        if (attackName == "Attack2")
        {
            yield return new WaitForSeconds(0.15f);
            ShootProjectile();
        }

        float attackDuration = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    // Ativar hitboxes, vai ser no animator
    public void EnableHitbox(string hitboxName)
    {
        if (hitboxName == "Attack1" && attack1Hitbox != null)
            attack1Hitbox.SetActive(true);
        else if (hitboxName == "Attack3" && attack3Hitbox != null)
            attack3Hitbox.SetActive(true);
    }

    // Desativar hitboxes
    public void DisableHitbox(string hitboxName)
    {
        if (hitboxName == "Attack1" && attack1Hitbox != null)
            attack1Hitbox.SetActive(false);
        else if (hitboxName == "Attack3" && attack3Hitbox != null)
            attack3Hitbox.SetActive(false);
    }

    // Disparar projetil
    private void ShootProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        // Instancia a flecha no spawnpoint
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // Direção do disparo
        float direction = GetComponent<SpriteRenderer>().flipX ? -1f : 1f;

        // Envia direção para o script do projétil
        ProjectileArrow arrow = projectile.GetComponent<ProjectileArrow>();
        if (arrow != null)
            arrow.SetDirection(direction);
    }

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
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
