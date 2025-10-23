using UnityEngine;
using System.Collections;

public class ProjectileArrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Animator anim;

    private float direction = 1f;
    private bool hasCollided = false;

    [Header("Configurações da Flecha")]
    public float speed = 10f;
    public float lifeTime = 3f;
    public float stickDuration = 5f;
    public float fadeDuration = 1.5f;

    [Header("Camadas de Colisão")]
    public LayerMask wallLayer;
    public LayerMask playerLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(float dir)
    {
        direction = dir;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir < 0 ? -1 : 1);
        transform.localScale = scale;

        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return;

        int layer = other.gameObject.layer;

        if (((1 << layer) & wallLayer) != 0)
        {
            StartCoroutine(HandleWallCollision());
        }
        else if (((1 << layer) & playerLayer) != 0)
        {
            StartCoroutine(HandlePlayerCollision());
        }
    }

    private IEnumerator HandleWallCollision()
    {
        hasCollided = true;

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;

        if (anim != null)
            anim.SetTrigger("Stick");

        yield return new WaitForSeconds(stickDuration);

        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }

    private IEnumerator HandlePlayerCollision()
    {
        hasCollided = true;

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }

    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null)
            yield break;

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
    }
}
