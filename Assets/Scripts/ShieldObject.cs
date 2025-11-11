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
       
    }

    private void OnEnable()
    {

        hitsLeft = maxHits;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
     
        DamageDealer dd = other.GetComponent<DamageDealer>();
        if (dd != null)
        {
            TakeHit();
           
        }
        else if (other.CompareTag("EnemyAttack"))
        {
            TakeHit();
        }
    }

    public void TakeHit()
    {
        hitsLeft--;
      
        if (hitsLeft <= 0)
            BreakShield();
    }

    private void BreakShield()
    {
        
        if (owner != null)
            owner.OnShieldBroken();

      
        gameObject.SetActive(false);

      
    }
}
