using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Damageable))]
public class ElfController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Damageable damageable;
    private SpriteRenderer sr;
    private int selfLayerID;

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
    private bool isAttacking = false;
    private bool isDead = false;

    [Header("Ataques e Projétil")]
    public GameObject attack1Hitbox;
    public GameObject attack3Hitbox;
    public Transform attack1Pivot;
    public Transform attack3Pivot;
    private Vector3 attack1PivotInitialScale;
    private Vector3 attack3PivotInitialScale;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 10f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
        sr = GetComponent<SpriteRenderer>();

        selfLayerID = gameObject.layer;

        if (attack1Hitbox == null)
            attack1Hitbox = FindChildByName("Attack1_Hitbox");
        if (attack3Hitbox == null)
            attack3Hitbox = FindChildByName("Attack3_Hitbox");

        if (attack1Hitbox) attack1Hitbox.SetActive(false);
        if (attack3Hitbox) attack3Hitbox.SetActive(false);

        if (attack1Pivot != null) attack1PivotInitialScale = attack1Pivot.localScale;
        if (attack3Pivot != null) attack3PivotInitialScale = attack3Pivot.localScale;

        damageable.onDeath += OnDeath;
        damageable.onHit += OnTakeHit;
    }

    private GameObject FindChildByName(string childName)
    {
        Transform childTransform = transform.Find(childName);
        return childTransform != null ? childTransform.gameObject : null;
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxisRaw("Horizontal");

        // Movimento
        if (!isAttacking && !isSliding)
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        else if (isAttacking)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        anim.SetBool("isRunning", move != 0 && isGrounded && !isAttacking && !isSliding);

        if (move != 0 && !isSliding)
        {
            bool flip = move < 0;
            sr.flipX = flip;

            if (projectileSpawnPoint != null)
            {
                float localX = Mathf.Abs(projectileSpawnPoint.localPosition.x) * (flip ? -1 : 1);
                projectileSpawnPoint.localPosition = new Vector3(localX, projectileSpawnPoint.localPosition.y, projectileSpawnPoint.localPosition.z);
            }

            if (attack1Pivot != null)
                attack1Pivot.localScale = new Vector3(attack1PivotInitialScale.x * (flip ? -1 : 1),
                                                      attack1PivotInitialScale.y,
                                                      attack1PivotInitialScale.z);

            if (attack3Pivot != null)
                attack3Pivot.localScale = new Vector3(attack3PivotInitialScale.x * (flip ? -1 : 1),
                                                      attack3PivotInitialScale.y,
                                                      attack3PivotInitialScale.z);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking && !isSliding)
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isAttacking && !isSliding)
            StartCoroutine(SlideCoroutine());

        if (!isAttacking && isGrounded && !isSliding)
        {
            if (Input.GetKeyDown(KeyCode.Z)) Attack("Attack1");
            else if (Input.GetKeyDown(KeyCode.X)) Attack("Attack2"); // <-- NÃO chama ShootProjectile() aqui
            else if (Input.GetKeyDown(KeyCode.C)) Attack("Attack3");
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    private IEnumerator SlideCoroutine()
    {
        isSliding = true;
        anim.SetBool("isSliding", true);

        float startTime = Time.time;
        float direction = sr.flipX ? -1f : 1f;

        while (Time.time < startTime + slideDuration)
        {
            rb.linearVelocity = new Vector2(direction * slideSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isSliding = false;
        anim.SetBool("isSliding", false);
    }

    void Attack(string attackName)
    {
        isAttacking = true;
        anim.SetTrigger(attackName);

        // OBS: ShootProjectile deve ser chamado por Animation Event (ex: "ShootProjectile")
    }

    public void EndAttack() => isAttacking = false;

    // ============================
    // EnableHitbox agora aplica dano imediatamente via OverlapCollider
    // ============================
    public void EnableHitbox(string hitboxName)
    {
        GameObject hb = null;
        if (hitboxName == "Attack1" && attack1Hitbox != null) hb = attack1Hitbox;
        else if (hitboxName == "Attack3" && attack3Hitbox != null) hb = attack3Hitbox;

        if (hb == null) return;

        Collider2D hbCol = hb.GetComponent<Collider2D>();
        DamageDealer dd = hb.GetComponent<DamageDealer>();
        float dmgValue = (dd != null) ? dd.damage : 10f;

        // Ativa visualmente a hitbox
        hb.SetActive(true);

        if (hbCol != null)
        {
            // Temporariamente garante que o collider esteja ativo para OverlapCollider
            hbCol.enabled = true;

            // Faz a checagem imediata de colisões com a hitbox
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            Collider2D[] results = new Collider2D[10];
            int hits = hbCol.Overlap(filter, results);

            for (int i = 0; i < hits; i++)
            {
                Collider2D other = results[i];
                if (other == null) continue;
                if (other.gameObject == gameObject) continue; // ignora a si mesmo
                if (other.gameObject.layer == gameObject.layer) continue; // friendly fire ignore

                Damageable tgt = other.GetComponent<Damageable>();
                if (tgt != null)
                {
                    tgt.TakeDamage(dmgValue);
                }
            }

            // desativa o collider para evitar que o sistema de física gere eventos duplicados
            hbCol.enabled = false;
        }

        // E fecha a hitbox após um curto tempo (caso alguma animação precise)
        StartCoroutine(ResetHitbox(hb));
    }

    private IEnumerator ResetHitbox(GameObject hitbox)
    {
        yield return new WaitForSeconds(0.12f);
        if (hitbox != null)
        {
            // garante collider desligado e objeto desativado
            Collider2D col = hitbox.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            hitbox.SetActive(false);
        }
    }

    public void DisableHitbox(string hitboxName)
    {
        if (hitboxName == "Attack1" && attack1Hitbox != null) attack1Hitbox.SetActive(false);
        else if (hitboxName == "Attack3" && attack3Hitbox != null) attack3Hitbox.SetActive(false);
    }

    public void ShootProjectile()
    {
        if (!projectilePrefab || !projectileSpawnPoint)
        {
            Debug.LogError("Projectile Prefab ou Spawn Point não atribuído!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        float direction = sr.flipX ? -1f : 1f;

        projectile.layer = selfLayerID;

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
            projectileRb.linearVelocity = new Vector2(direction * projectileSpeed, 0);

        ProjectileArrow arrow = projectile.GetComponent<ProjectileArrow>();
        if (arrow != null)
            arrow.SetDirection(direction, gameObject);
    }

    private void OnTakeHit(float dmg)
    {
        anim.SetTrigger("TakeHit");
    }

    private void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
