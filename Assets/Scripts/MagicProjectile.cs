using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 20f;
    public float lifetime = 3f;


    private Vector2 direction;
    private bool hasHit = false;

    public void Initialize(Vector2 dir, GameObject caster)
    {
        direction = dir.normalized;
        owner = caster;
        Destroy(gameObject, lifetime);

        // Flipar o sprite se estiver indo para a esquerda
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = direction.x < 0;
    }



    void Update()
    {
        if (!hasHit)
            transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.gameObject == owner) return; // Ignora o próprio mago

        Damageable dmg = other.GetComponent<Damageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
        }

        hasHit = true;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Impact");
    }

    // Chamado por Animation Event no final da animação de impacto
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    private GameObject owner;

    
}