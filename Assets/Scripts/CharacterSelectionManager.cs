using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "realgame";
    [SerializeField] private Button confirmButton;

    [SerializeField] private List<CharacterButton> characterButtons;

    public static string selectedCharacterP1;
    public static string selectedCharacterP2;

    private int currentPlayer = 1;
    private string preSelectedCharacter;

    private void Start()
    {
        confirmButton.interactable = false;

        foreach (var btn in characterButtons)
        {
            btn.SetSelected(false);
            btn.SetManager(this);
        }
    }

    public void PreSelectCharacter(string name)
    {
        preSelectedCharacter = name;
        confirmButton.interactable = true;

        foreach (var btn in characterButtons)
            btn.SetSelected(btn.characterName == name);
    }

    public void ConfirmSelection()
    {
        if (string.IsNullOrEmpty(preSelectedCharacter)) return;

        if (currentPlayer == 1)
        {
            selectedCharacterP1 = preSelectedCharacter;
            currentPlayer = 2;
            confirmButton.interactable = false;
            preSelectedCharacter = null;
            Debug.Log("Jogador 1 escolheu: " + selectedCharacterP1);
        }
        else
        {
            selectedCharacterP2 = preSelectedCharacter;
            Debug.Log("Jogador 2 escolheu: " + selectedCharacterP2);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
