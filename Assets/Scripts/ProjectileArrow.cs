using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(DamageDealer))]
public class ProjectileArrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Collider2D projectileCollider;

    private float direction = 1f;
    private bool hasHit = false;
    private bool isFading = false;

    private GameObject shooter; 

    [Header("Configurações da Flecha")]
    public float speed = 10f;
    public float lifeTime = 3f;
    public float stickDuration = 3f;
    public float fadeDuration = 1.5f;

    [Header("Camadas de Colisão")]
    public LayerMask wallLayer;

    private DamageDealer dmgDealer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        projectileCollider = GetComponent<Collider2D>();
        dmgDealer = GetComponent<DamageDealer>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

  
    public void SetDirection(float dir, GameObject owner)
    {
        direction = dir;
        shooter = owner;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir < 0 ? -1 : 1);
        transform.localScale = scale;

        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    void FixedUpdate()
    {
        if (!hasHit && !isFading)
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFading || hasHit) return;

        
        if (shooter != null && other.gameObject == shooter) return;

       
        Damageable target = other.GetComponent<Damageable>();
        if (target != null)
        {
            
            if (other.gameObject.layer == gameObject.layer) return;

            
            if (dmgDealer != null)
                target.TakeDamage(dmgDealer.damage);

            hasHit = true;

            
            if (projectileCollider != null)
                projectileCollider.enabled = false;

            StartCoroutine(HandleEnemyHit());
            return;
        }

      
        int layer = other.gameObject.layer;
        if (((1 << layer) & wallLayer) != 0)
        {
            hasHit = true;
            StartCoroutine(HandleWallCollision());
        }
    }

    private IEnumerator HandleWallCollision()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
        }

        if (anim != null) anim.SetTrigger("Stick");

        yield return new WaitForSeconds(stickDuration);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator HandleEnemyHit()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
        }

        if (anim != null) anim.SetTrigger("Hit");

        yield return StartCoroutine(FadeOut(0.1f));
    }

    private IEnumerator FadeOut(float duration = 0f)
    {
        if (isFading) yield break;
        isFading = true;

        float fadeTime = (duration > 0) ? duration : fadeDuration;
        if (rb != null) rb.simulated = false;

        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
