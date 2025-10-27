using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Dados de Dano")]
    public float damage = 10f;
    public string targetTag = "";

    [Tooltip("Se verdadeiro, o hitbox será destruído automaticamente após o tempo definido abaixo.")]
    public bool destroyAfterTime = false;
    public float lifeTime = 0.2f;

    private void Start()
    {
        if (destroyAfterTime)
            Destroy(gameObject, lifeTime);
    }

    // Nota:
    // Não implementamos lógica de dano aqui (OnTriggerEnter) porque o Damageable
    // faz OnTriggerEnter e aplica invulnerabilidade. Manter código simples evita conflitos.
}
