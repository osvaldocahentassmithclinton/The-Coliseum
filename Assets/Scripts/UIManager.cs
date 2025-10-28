using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager Instance;

    [Header("Referências das Barras de Vida")]
    public Slider player1HealthBar;
    public Slider player2HealthBar;

    [Header("Mensagens de Vitória")]
    public GameObject endGamePanel;
    public TMP_Text winnerText;

    [Header("Nomes das Cenas")]
    public string characterSelectionScene = "CharacterSelection";
    public string mainMenuScene = "MainMenu";

    private Damageable player1;
    private Damageable player2;

    private void Awake()
    {
        // Configura singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        // tenta encontrar os players automaticamente
        GameObject p1Obj = GameObject.FindGameObjectWithTag("Player1");
        GameObject p2Obj = GameObject.FindGameObjectWithTag("Player2");

        if (p1Obj != null)
            player1 = p1Obj.GetComponent<Damageable>();
        if (p2Obj != null)
            player2 = p2Obj.GetComponent<Damageable>();

        // inicializa sliders
        if (player1 != null && player1HealthBar != null)
        {
            player1HealthBar.maxValue = player1.maxHealth;
            player1HealthBar.value = player1.CurrentHealth;
        }
        if (player2 != null && player2HealthBar != null)
        {
            player2HealthBar.maxValue = player2.maxHealth;
            player2HealthBar.value = player2.CurrentHealth;
        }
    }

    void Update()
    {
        if (player1 == null || player2 == null) return;

        // atualiza vida em tempo real
        if (player1HealthBar != null)
            player1HealthBar.value = player1.CurrentHealth;
        if (player2HealthBar != null)
            player2HealthBar.value = player2.CurrentHealth;

        // checa fim de jogo (opcional, caso queira que o UIManager também detecte)
        if ((player1.IsDead || player2.IsDead) && endGamePanel != null && !endGamePanel.activeSelf)
        {
            string message;
            if (player1.IsDead && player2.IsDead)
                message = "DRAW";
            else if (player1.IsDead)
                message = "P2 WINS";
            else
                message = "P1 WINS";

            ShowEndGameScreen(message);
        }
    }

    /// <summary>
    /// Chama a tela de fim de jogo e mostra a mensagem passada
    /// </summary>
    public void ShowEndGameScreen(string message)
    {
        if (endGamePanel == null || endGamePanel.activeSelf) return;

        endGamePanel.SetActive(true);

        if (winnerText != null)
            winnerText.text = message;

        Time.timeScale = 0f; // pausa o jogo
    }

    // chamado pelos botões do UI
    public void ReturnToCharacterSelection()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(characterSelectionScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
