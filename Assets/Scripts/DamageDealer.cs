using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Dados de Dano")]
    public float damage = 10f;
    public string targetTag = "";

    [Tooltip("Se verdadeiro, o hitbox será destruído automaticamente após o tempo definido abaixo.")]
    public bool destroyAfterTime = false;
    public float lifeTime = 0.2f;

    // A lógica de dano único agora reside em Damageable (via cooldown) ou na animação (desativando a hitbox).
    // hasDealtDamage é redundante e pode causar falhas, pois quem aplica o dano é o Damageable.

    private void Start()
    {
        if (destroyAfterTime)
            Destroy(gameObject, lifeTime);
    }

    // CORREÇÃO: Removida a lógica de dano aqui para que o DANO seja 
    // exclusivamente manipulado pelo Damageable via OnTriggerEnter2D.
    // Desta forma, o Knight/Elf deve DESATIVAR A HITBOX através de um 
    // Animation Event imediatamente após a aplicação do dano.
    // Não é necessário o OnTriggerEnter2D neste script se o Damageable já o faz.
    // Mantemos, no entanto, a Tag para filtros futuros, se necessário.

    // Opcional: Se quiser manter uma verificação de Tag aqui para projéteis que não tem Damageable:
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            return;
        
        // Se for um projétil, o dano será tratado pelo Damageable
        // Se não for um projétil (hitbox melee), o Damageable que irá reagir.
    }
    */
}