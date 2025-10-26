using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    // Este prefab será o ÚNICO que este script pode spawnar.
    [Header("Prefab do personagem a spawnar")]
    public GameObject characterPrefab;

    void Start()
    {
        if (characterPrefab != null)
        {
            // AQUI você spawna o personagem na posição deste GameObject
            Instantiate(characterPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Este erro aponta que o campo 'characterPrefab' está vazio no Inspector
            Debug.LogError("Nenhum prefab de personagem atribuído!");
        }
    }
}