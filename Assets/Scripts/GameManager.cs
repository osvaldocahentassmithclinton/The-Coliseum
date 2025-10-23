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

    public Transform spawnPoint; 

    private void Start()
    {
        string selected = CharacterSelectionManager.selectedCharacter;

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
