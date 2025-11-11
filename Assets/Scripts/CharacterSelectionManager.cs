using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; 

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "realgame";
    [SerializeField] private Button confirmButton;

    [SerializeField] private List<CharacterButton> characterButtons;

    
    [SerializeField] private TMP_Text selectingPlayerText;

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

        
        if (selectingPlayerText != null)
        {
            selectingPlayerText.gameObject.SetActive(true);
            selectingPlayerText.text = "P1";
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

            
            if (selectingPlayerText != null)
            {
                selectingPlayerText.gameObject.SetActive(true);
                selectingPlayerText.text = "P2";
            }

            
            foreach (var btn in characterButtons)
                btn.SetSelected(false);
        }
        else
        {
            selectedCharacterP2 = preSelectedCharacter;
            Debug.Log("Jogador 2 escolheu: " + selectedCharacterP2);

            if (selectingPlayerText != null)
                selectingPlayerText.gameObject.SetActive(false);

            SceneManager.LoadScene(gameSceneName);
        }
    }

    

    public void RandomSelectForCurrentPlayer()
    {
        RandomSelectForPlayer(currentPlayer);
    }

    public void RandomSelectForPlayer(int player)
    {
        if (characterButtons == null || characterButtons.Count == 0) return;

        player = Mathf.Clamp(player, 1, 2);

        int randomIndex = Random.Range(0, characterButtons.Count);
        string randomName = characterButtons[randomIndex].characterName;

        if (selectingPlayerText != null)
        {
            selectingPlayerText.gameObject.SetActive(true);
            selectingPlayerText.text = (player == 1) ? "P1" : "P2";
        }

        PreSelectCharacter(randomName);
    }

    public void StartPlayerSelection(int player)
    {
        currentPlayer = Mathf.Clamp(player, 1, 2);
        if (selectingPlayerText != null)
        {
            selectingPlayerText.gameObject.SetActive(true);
            selectingPlayerText.text = (currentPlayer == 1) ? "P1" : "P2";
        }

        preSelectedCharacter = null;
        confirmButton.interactable = false;
        foreach (var btn in characterButtons)
            btn.SetSelected(false);
    }
}
