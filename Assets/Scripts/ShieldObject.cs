using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldObject : MonoBehaviour
{
    public int maxHits = 3;
    private int hitsLeft;
    private WizardController owner;

    void Start()
    {
        hitsLeft = maxHits;
        owner = GetComponentInParent<WizardController>();
        // geralmente começamos com o objeto desativado e o WizardController ativa quando necessário
        // gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // resetar contagem quando ativado
        hitsLeft = maxHits;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Adaptar conforme seu sistema: checamos por DamageDealer ou tag "EnemyAttack"
        DamageDealer dd = other.GetComponent<DamageDealer>();
        if (dd != null)
        {
            TakeHit();
            // opcional: impedir que o ataque também a?ete o jogador (dependendo do design)
        }
        else if (other.CompareTag("EnemyAttack"))
        {
            TakeHit();
        }
    }

    public void TakeHit()
    {
        hitsLeft--;
        // opcional: efeito visual/som aqui
        if (hitsLeft <= 0)
            BreakShield();
    }

    private void BreakShield()
    {
        // notificamos o owner
        if (owner != null)
            owner.OnShieldBroken();

        // desativamos o objeto de shield (visual/colisor)
        gameObject.SetActive(false);

        // hitsLeft será resetado no OnEnable quando o shield for ativado novamente
    }
}
