using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // Importante para carregar cenas

public class MenuOpcoes : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject painelOpcoes;
    public Slider sliderBrilho;
    public Slider sliderVolumeMusica;
    public Image mascaraBrilho; // <<< Imagem preta semi-transparente sobre a tela

    [Header("Referência ao Audio")]
    public AudioSource musicaDeFundo;

    void Start()
    {
        // Carrega configurações salvas
        float brilhoSalvo = PlayerPrefs.GetFloat("Brilho", 1f);
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        sliderBrilho.value = brilhoSalvo;
        sliderVolumeMusica.value = volumeSalvo;

        AjustarBrilho(brilhoSalvo);
        AjustarVolumeMusica(volumeSalvo);

        // Adiciona listeners
        sliderBrilho.onValueChanged.AddListener(AjustarBrilho);
        sliderVolumeMusica.onValueChanged.AddListener(AjustarVolumeMusica);

        painelOpcoes.SetActive(false);
    }

    public void AbrirFecharOpcoes()
    {
        painelOpcoes.SetActive(!painelOpcoes.activeSelf);
    }

    public void AjustarBrilho(float valor)
    {
        // Simula o brilho ajustando o alpha da imagem preta
        if (mascaraBrilho != null)
        {
            Color corAtual = mascaraBrilho.color;
            corAtual.a = 1f - valor; // brilho 1 => alpha 0 (transparente), brilho 0 => alpha 1 (preto total)
            mascaraBrilho.color = corAtual;
        }

        PlayerPrefs.SetFloat("Brilho", valor);
    }

    public void AjustarVolumeMusica(float volume)
    {
        if (musicaDeFundo != null)
        {
            musicaDeFundo.volume = volume;
        }
        PlayerPrefs.SetFloat("VolumeMusica", volume);
    }

    public void FecharOpcoes()
    {
        painelOpcoes.SetActive(false);
    }

    // NOVO: Método para voltar ao menu principal
    public void VoltarAoMenuPrincipal()
    {
        PlayerPrefs.Save(); // Salva as prefs antes de sair
        SceneManager.LoadScene("menu"); // Substitua pelo nome da sua cena de menu
    }
}
