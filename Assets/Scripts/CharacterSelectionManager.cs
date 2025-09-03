using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    // Nome da cena do jogo
    [SerializeField] private string gameSceneName = "game";

    // Referência ao botão de confirmar
    [SerializeField] private Button confirmButton;

    // Guarda o personagem atualmente pré-selecionado (ainda não confirmado)
    private string preSelectedCharacter;

    // Lista de botões de personagens
    [SerializeField] private List<CharacterButton> characterButtons;

    // Personagem selecionado final (static para acesso global)
    public static string selectedCharacter;

    private void Start()
    {
        // Desativa o botão confirmar até que algo seja pré-selecionado
        confirmButton.interactable = false;

        // Remove destaque de todos os personagens ao iniciar
        foreach (var btn in characterButtons)
        {
            btn.SetSelected(false);
            btn.SetManager(this); // para que cada botão chame esse manager
        }
    }

    // Chamada ao clicar em um personagem (pré-seleção)
    public void PreSelectCharacter(string characterName)
    {
        preSelectedCharacter = characterName;
        confirmButton.interactable = true;

        // Atualiza bordas visuais: ativa só no botão pré-selecionado
        foreach (var btn in characterButtons)
        {
            btn.SetSelected(btn.characterName == characterName);
        }

        Debug.Log("Pré-selecionado: " + characterName);
    }

    // Chamada ao clicar no botão Confirmar
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
