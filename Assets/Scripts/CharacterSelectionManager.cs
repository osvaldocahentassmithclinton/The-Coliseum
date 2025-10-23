using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "game";

    [SerializeField] private Button confirmButton;

    private string preSelectedCharacter;

    [SerializeField] private List<CharacterButton> characterButtons;

    public static string selectedCharacter;

    private void Start()
    {
        confirmButton.interactable = false;

        foreach (var btn in characterButtons)
        {
            btn.SetSelected(false);
            btn.SetManager(this);
        }
    }

    // Ao clicar em um dos personagens
    public void PreSelectCharacter(string characterName)
    {
        preSelectedCharacter = characterName;
        confirmButton.interactable = true;

        // Botar bordas na seleção
        foreach (var btn in characterButtons)
        {
            btn.SetSelected(btn.characterName == characterName);
        }

        Debug.Log("Pré-selecionado: " + characterName);
    }

    public void ConfirmSelection()
    {
        if (!string.IsNullOrEmpty(preSelectedCharacter))
        {
            selectedCharacter = preSelectedCharacter;
            Debug.Log("Personagem confirmado: " + selectedCharacter);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
