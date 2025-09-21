using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    void Start()
    {
        string characterName = CharacterSelectionManager.selectedCharacter;

        if (!string.IsNullOrEmpty(characterName))
        {
            GameObject prefab = Resources.Load<GameObject>("Characters/" + characterName);
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Prefab não encontrado: " + characterName);
            }
        }
    }
}

