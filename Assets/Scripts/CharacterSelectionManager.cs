using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    // Nome da cena do jogo
    [SerializeField] private string gameSceneName = "game";

    // Refer�ncia ao bot�o de confirmar
    [SerializeField] private Button confirmButton;

    // Guarda o personagem atualmente pr�-selecionado (ainda n�o confirmado)
    private string preSelectedCharacter;

    // Lista de bot�es de personagens
    [SerializeField] private List<CharacterButton> characterButtons;

    // Personagem selecionado final (static para acesso global)
    public static string selectedCharacter;

    private void Start()
    {
        // Desativa o bot�o confirmar at� que algo seja pr�-selecionado
        confirmButton.interactable = false;

        // Remove destaque de todos os personagens ao iniciar
        foreach (var btn in characterButtons)
        {
            btn.SetSelected(false);
            btn.SetManager(this); // para que cada bot�o chame esse manager
        }
    }

    // Chamada ao clicar em um personagem (pr�-sele��o)
    public void PreSelectCharacter(string characterName)
    {
        preSelectedCharacter = characterName;
        confirmButton.interactable = true;

        // Atualiza bordas visuais: ativa s� no bot�o pr�-selecionado
        foreach (var btn in characterButtons)
        {
            btn.SetSelected(btn.characterName == characterName);
        }

        Debug.Log("Pr�-selecionado: " + characterName);
    }

    // Chamada ao clicar no bot�o Confirmar
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
