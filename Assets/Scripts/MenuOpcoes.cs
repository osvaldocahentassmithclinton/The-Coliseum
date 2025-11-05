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

    // ========== Novos campos para controle de música entre cenas ==========
    [Header("Configuração de músicas por cena (mínimo necessário)")]
    [Tooltip("Música usada nas duas cenas que devem compartilhar o mesmo tempo de reprodução")]
    public AudioClip sharedMusic;

    [Tooltip("Nomes das 2 cenas que compartilham 'sharedMusic' (digite exatamente como aparece na Build Settings)")]
    public string sceneNameSharedA;
    public string sceneNameSharedB;

    [Tooltip("Cena que tocará outra música (digite exatamente como aparece na Build Settings)")]
    public string otherSceneName;
    [Tooltip("Música que toca na cena 'otherSceneName'")]
    public AudioClip otherMusic;
    // =====================================================================

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

        // Inicializa o player de música persistente (se ainda não existir)
        // Passa as configurações que o usuário preencheu no Inspector.
        MusicPersistent.SetupIfNeeded(sharedMusic, otherMusic, sceneNameSharedA, sceneNameSharedB, otherSceneName, volumeSalvo);
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
        // Se houver um MusicPersistent, encaminha pra ele (persistente entre cenas)
        if (MusicPersistent.Instance != null)
        {
            MusicPersistent.Instance.SetVolume(volume);
        }
        else
        {
            if (musicaDeFundo != null)
            {
                musicaDeFundo.volume = volume;
            }
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

    // ===========================
    // CLASSE INTERNA: gerencia música persistente entre cenas
    // ===========================
    private class MusicPersistent : MonoBehaviour
    {
        public static MusicPersistent Instance;

        private AudioSource src;
        private AudioClip shared;
        private AudioClip other;
        private string sharedA;
        private string sharedB;
        private string otherScene;

        // para preservar tempo quando se troca entre sharedA e sharedB
        private string lastKey = "";
        private float lastTime = 0f;

        // Setup chamado pela MenuOpcoes.Start para criar/atualizar o singleton
        public static void SetupIfNeeded(AudioClip sharedMusic, AudioClip otherMusic, string sA, string sB, string sOther, float initialVolume)
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("PersistentMusic");
                Instance = go.AddComponent<MusicPersistent>();
                DontDestroyOnLoad(go);
                Instance.src = go.AddComponent<AudioSource>();
                Instance.src.loop = true;
                Instance.src.playOnAwake = false;
                SceneManager.sceneLoaded += Instance.OnSceneLoaded;
            }

            // aplica configs
            Instance.shared = sharedMusic;
            Instance.other = otherMusic;
            Instance.sharedA = sA;
            Instance.sharedB = sB;
            Instance.otherScene = sOther;
            Instance.SetVolume(initialVolume);

            // decide o que tocar agora (baseado na cena ativa)
            Scene current = SceneManager.GetActiveScene();
            Instance.HandleSceneChange(current.name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleSceneChange(scene.name);
        }

        private void HandleSceneChange(string sceneName)
        {
            // guarda tempo atual antes de qualquer troca
            if (src != null && src.isPlaying && src.clip != null)
            {
                lastTime = src.time;
            }

            // escolhe qual clip tocar
            AudioClip desired = null;
            string key = "";

            if (!string.IsNullOrEmpty(sharedA) && (sceneName == sharedA || sceneName == sharedB))
            {
                desired = shared;
                key = "shared";
            }
            else if (!string.IsNullOrEmpty(otherScene) && sceneName == otherScene)
            {
                desired = other;
                key = "other";
            }
            else
            {
                // cena não configurada: se quiser, mantemos a música atual.
                // Aqui, vamos manter o que já estava tocando (não parar).
                desired = src.clip;
                // key permanece como lastKey para preservar comportamento
                key = lastKey;
            }

            if (desired == null)
            {
                // nada configurado para tocar aqui; deixa como está (ou para)
                if (src.isPlaying)
                {
                    src.Pause();
                    lastKey = "";
                }
                return;
            }

            // se já está tocando esse clip, não reinicia
            if (src.clip == desired && src.isPlaying)
            {
                // nada a fazer, mas atualiza lastKey
                lastKey = key;
                return;
            }

            // se trocando entre as duplas que compartilham, preserva tempo
            float startTime = 0f;
            if (key == "shared" && lastKey == "shared")
            {
                // mantemos lastTime (o tempo que gravamos antes)
                startTime = lastTime;
            }
            else
            {
                startTime = 0f;
            }

            src.clip = desired;
            src.time = Mathf.Clamp(startTime, 0f, (src.clip != null ? src.clip.length : 0f));
            src.loop = true;
            src.Play();

            lastKey = key;
        }

        public void SetVolume(float vol)
        {
            if (src != null) src.volume = vol;
        }
    }
}
