using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterPrefab
    {
        public string characterName;
        public GameObject prefab;
    }

    public CharacterPrefab[] characterPrefabs;

    public Transform spawnPoint; // Arraste no Inspector o Transform que marca o local do spawn

    private void Start()
    {
        string selected = CharacterSelectionManager.selectedCharacter;

        // Busca o prefab correspondente ao personagem selecionado
        foreach (var cp in characterPrefabs)
        {
            if (cp.characterName == selected)
            {
                Instantiate(cp.prefab, spawnPoint.position, spawnPoint.rotation);
                return;
            }
        }

        Debug.LogError("Personagem selecionado não encontrado! Spawn cancelado.");
    }
}
