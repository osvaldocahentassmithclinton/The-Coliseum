using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
   
    [Header("Prefab do personagem a spawnar")]
    public GameObject characterPrefab;

    void Start()
    {
        if (characterPrefab != null)
        {
           
            Instantiate(characterPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            
            Debug.LogError("Nenhum prefab de personagem atribuído!");
        }
    }
}