using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpcoesInGame : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject painelOpcoesInGame;
    public Slider sliderBrilhoInGame;
    public Slider sliderVolumeMusicaInGame;
    public Image mascaraBrilho;

    [Header("Referência ao Audio")]
    public AudioSource musicaDeFundo;

    private AudioSource persistentMusicSource; 

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


        GameObject persistent = GameObject.Find("PersistentMusic");
        if (persistent != null)
        {
            persistentMusicSource = persistent.GetComponent<AudioSource>();
            if (persistentMusicSource != null)
            {

                persistentMusicSource.volume = volumeSalvo;
            }
        }
        else
        {

            if (musicaDeFundo != null)
                musicaDeFundo.volume = volumeSalvo;
        }


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
        if (mascaraBrilho != null)
        {
            Color corAtual = mascaraBrilho.color;
            corAtual.a = 1f - valor;
            mascaraBrilho.color = corAtual;
        }

        PlayerPrefs.SetFloat("Brilho", valor);
    }

    public void AplicarVolume(float volume)
    {

        if (persistentMusicSource != null)
        {
            persistentMusicSource.volume = volume;
        }
        else if (musicaDeFundo != null)
        {
            musicaDeFundo.volume = volume;
        }


        PlayerPrefs.SetFloat("VolumeMusica", volume);
    }

    public void VoltarAoMenuPrincipal()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("menu");
    }
}
