using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Necessário para carregar cenas
public class OpcoesInGame : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject painelOpcoesInGame;
    public Slider sliderBrilhoInGame;
    public Slider sliderVolumeMusicaInGame;
    public Image mascaraBrilho; // Imagem preta semi-transparente sobre a tela

    [Header("Referência ao Audio")]
    public AudioSource musicaDeFundo;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            painelOpcoesInGame.SetActive(!painelOpcoesInGame.activeSelf);
        }
    }

    void Start()
    {
        float brilhoSalvo = PlayerPrefs.GetFloat("Brilho", 1f);
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        sliderBrilhoInGame.value = brilhoSalvo;
        sliderVolumeMusicaInGame.value = volumeSalvo;

        AplicarBrilho(brilhoSalvo);
        AplicarVolume(volumeSalvo);

        sliderBrilhoInGame.onValueChanged.AddListener(AplicarBrilho);
        sliderVolumeMusicaInGame.onValueChanged.AddListener(AplicarVolume);

        painelOpcoesInGame.SetActive(false);
    }

    public void AbrirOpcoesInGame()
    {
        painelOpcoesInGame.SetActive(true);
    }

    public void FecharOpcoesInGame()
    {
        painelOpcoesInGame.SetActive(false);
    }

    public void AplicarBrilho(float valor)
    {
        // O valor de brilho vai de 0 (escuro) até 1 (sem escurecimento)
        if (mascaraBrilho != null)
        {
            Color corAtual = mascaraBrilho.color;
            corAtual.a = 1f - valor; // Inverter: mais brilho = menos alpha
            mascaraBrilho.color = corAtual;
        }

        PlayerPrefs.SetFloat("Brilho", valor);
    }

    public void AplicarVolume(float volume)
    {
        if (musicaDeFundo != null)
            musicaDeFundo.volume = volume;

        PlayerPrefs.SetFloat("VolumeMusica", volume);
    }

    public void VoltarAoMenuPrincipal()
    {
        // Você pode salvar configurações antes de sair, se necessário
        PlayerPrefs.Save();

        // Substitua "MenuPrincipal" pelo nome exato da sua cena de menu
        SceneManager.LoadScene("menu");
    }


}
