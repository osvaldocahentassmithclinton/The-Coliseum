using UnityEngine;
using System.Collections;

public class ProjectileArrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private float direction = 1f;
    private bool hasHitWall = false;
    private bool isFading = false;

    [Header("Configurações da Flecha")]
    public float speed = 10f;
    public float lifeTime = 3f;
    public float stickDuration = 3f;
    public float fadeDuration = 1.5f;

    [Header("Camadas de Colisão")]
    public LayerMask wallLayer;
    public LayerMask playerLayer;
    public LayerMask enemyLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Destroi depois de um tempo, se não colidir
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(float dir)
    {
        direction = dir;

        // Inverte o sprite conforme direção
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir < 0 ? -1 : 1);
        transform.localScale = scale;

        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    void FixedUpdate()
    {
        // Continua andando até colidir com parede
        if (!hasHitWall && !isFading)
        {
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFading) return; // já tá morrendo

        int layer = other.gameObject.layer;

        // Parede (tilemap, chão, etc)
        if (((1 << layer) & wallLayer) != 0)
        {
            StartCoroutine(HandleWallCollision());
        }
        // Inimigo (ou player, dependendo da lógica)
        else if (((1 << layer) & enemyLayer) != 0)
        {
            StartCoroutine(HandleEnemyHit());
        }
    }

    private IEnumerator HandleWallCollision()
    {
        hasHitWall = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true; // trava a física
        rb.simulated = false;

        if (anim != null)
            anim.SetTrigger("Stick");

        yield return new WaitForSeconds(stickDuration);
        yield return StartCoroutine(FadeOut());

        Destroy(gameObject);
    }

    private IEnumerator HandleEnemyHit()
    {
        // Continua andando pra frente, mas começa o fade
        StartCoroutine(FadeOut());
        yield break;
    }

    private IEnumerator FadeOut()
    {
        if (isFading) yield break;
        isFading = true;

        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
