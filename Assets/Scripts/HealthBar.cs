using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    public Damageable target; // atribuído pelo GameManager no Start
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning($"HealthBar on {gameObject.name} não tem target atribuído.");
            return;
        }

        slider.maxValue = target.maxHealth;
        slider.value = target.CurrentHealth;
        slider.interactable = false;
    }

    void Update()
    {
        if (target == null) return;

        // Atualiza valor da barra
        slider.maxValue = target.maxHealth; // em caso de buffs
        slider.value = Mathf.Clamp(target.CurrentHealth, 0f, target.maxHealth);
    }
}
