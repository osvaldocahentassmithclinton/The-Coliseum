using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterPrefab
    {
        public string characterName;
        public GameObject prefab;
    }

    public CharacterPrefab[] characterPrefabs;

    [Header("Spawn Points")]
    public Transform player1Spawn;
    public Transform player2Spawn;

    [Header("Layers de Colisão")]
    [Tooltip("ID numérico da Layer de Colisão do Player 1 (Ex: 8). Digite o número.")]
    public int player1Layer = 8;

    [Tooltip("ID numérico da Layer de Colisão do Player 2 (Ex: 9). Digite o número.")]
    public int player2Layer = 9;

    [Header("UI")]
    public HealthBar player1HealthBar; // arrastar referência no inspector
    public HealthBar player2HealthBar; // arrastar referência no inspector

    private GameObject player1;
    private GameObject player2;

    private bool gameEnded = false;

    private void Start()
    {
        string p1Name = CharacterSelectionManager.selectedCharacterP1;
        string p2Name = CharacterSelectionManager.selectedCharacterP2;

        if (string.IsNullOrEmpty(p1Name) || string.IsNullOrEmpty(p2Name))
        {
            Debug.LogError("Faltando seleção de personagem.");
            return;
        }

        // CHANGED: passamos o playerIndex para que o spawn configure PlayerInput.playerId corretamente.
        player1 = SpawnCharacter(p1Name, player1Spawn.position, false, player1Layer, 1); // CHANGED
        player2 = SpawnCharacter(p2Name, player2Spawn.position, true, player2Layer, 2);  // CHANGED

        if (player1 == null || player2 == null)
        {
            Debug.LogError("Falha ao instanciar personagens.");
            return;
        }

        player1.tag = "Player1";
        player2.tag = "Player2";

        var dmg1 = player1.GetComponent<Damageable>();
        var dmg2 = player2.GetComponent<Damageable>();
        if (dmg1 != null) dmg1.opponent = player2;
        if (dmg2 != null) dmg2.opponent = player1;

        // Vincula as HealthBars (se atribuídas no Inspector)
        if (player1HealthBar != null && dmg1 != null)
            player1HealthBar.target = dmg1;

        if (player2HealthBar != null && dmg2 != null)
            player2HealthBar.target = dmg2;

        // Inscreve eventos de morte para checar fim de jogo
        if (dmg1 != null) dmg1.onDeath += () => OnCharacterDeath(1);
        if (dmg2 != null) dmg2.onDeath += () => OnCharacterDeath(2);
    }

    // CHANGED: adicionamos parâmetro playerIndex (1 ou 2)
    private GameObject SpawnCharacter(string name, Vector3 position, bool flip, int layerID, int playerIndex) // CHANGED
    {
        foreach (var cp in characterPrefabs)
        {
            if (cp.characterName == name)
            {
                GameObject obj = Instantiate(cp.prefab, position, Quaternion.identity);

                // CHANGED: garante que o prefab terá um PlayerInput e configura o playerId dinamicamente
                PlayerInput pi = obj.GetComponent<PlayerInput>(); // CHANGED
                if (pi == null) pi = obj.AddComponent<PlayerInput>(); // CHANGED
                pi.playerId = playerIndex; // CHANGED

                SetLayerRecursively(obj, layerID);

                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.flipX = flip;

                return obj;
            }
        }

        Debug.LogError("Personagem não encontrado: " + name);
        return null;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void OnCharacterDeath(int playerIndex)
    {
        if (gameEnded) return;

        // Checa estado das duas entidades
        var dmg1 = player1 != null ? player1.GetComponent<Damageable>() : null;
        var dmg2 = player2 != null ? player2.GetComponent<Damageable>() : null;

        bool p1Dead = dmg1 == null ? true : dmg1.IsDead;
        bool p2Dead = dmg2 == null ? true : dmg2.IsDead;

        if (p1Dead && p2Dead)
        {
            EndGame("DRAW");
        }
        else if (p1Dead)
        {
            EndGame("P2 WINS");
        }
        else if (p2Dead)
        {
            EndGame("P1 WINS");
        }
    }

    private void EndGame(string message)
    {
        gameEnded = true;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowEndGameScreen(message);

        Debug.Log("Fim de jogo: " + message);
    }

    // Métodos públicos ligados aos botões (no UIManager/na cena)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("MainMenu"); // descomente e coloque o nome correto
    }

    public void GoToCharacterSelect()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("CharacterSelect"); // descomente e coloque o nome correto
    }
}
