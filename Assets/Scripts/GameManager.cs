using UnityEngine;
using System.Collections;
using System;

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


    private GameObject player1;
    private GameObject player2;

    // (Assumindo que CharacterSelectionManager.selectedCharacterP1 existe de outra cena)
    // Se não existir, use constantes para teste:
    // public static string selectedCharacterP1 = "Knight"; 
    // public static string selectedCharacterP2 = "Elf";

    private void Start()
    {
        // Exemplo: Substitua pela sua lógica de seleção, se necessário.
        string p1Name = CharacterSelectionManager.selectedCharacterP1;
        string p2Name = CharacterSelectionManager.selectedCharacterP2;

        if (string.IsNullOrEmpty(p1Name) || string.IsNullOrEmpty(p2Name))
        {
            Debug.LogError("Faltando seleção de personagem.");
            return;
        }

        // Chama o SpawnCharacter passando o ID da Layer correta
        player1 = SpawnCharacter(p1Name, player1Spawn.position, false, player1Layer);
        player2 = SpawnCharacter(p2Name, player2Spawn.position, true, player2Layer);

        if (player1 == null || player2 == null)
        {
            Debug.LogError("Falha ao instanciar personagens.");
            return;
        }

        // Define Tags e Opponent
        player1.tag = "Player1";
        player2.tag = "Player2";

        var dmg1 = player1.GetComponent<Damageable>();
        var dmg2 = player2.GetComponent<Damageable>();
        if (dmg1 != null) dmg1.opponent = player2;
        if (dmg2 != null) dmg2.opponent = player1;
    }

    private GameObject SpawnCharacter(string name, Vector3 position, bool flip, int layerID)
    {
        foreach (var cp in characterPrefabs)
        {
            if (cp.characterName == name)
            {
                GameObject obj = Instantiate(cp.prefab, position, Quaternion.identity);

                // Aplica a Layer a todos os filhos do personagem (IMPORTANTE para hitboxes e projéteis)
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

    // Método auxiliar para aplicar a Layer recursivamente a todos os filhos
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
}