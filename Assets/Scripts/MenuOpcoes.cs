using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class MenuOpcoes : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject painelOpcoes;
    public Slider sliderBrilho;
    public Slider sliderVolumeMusica;
    public Image mascaraBrilho; 

    [Header("Referência ao Audio")]
    public AudioSource musicaDeFundo;

  
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
    

    void Start()
    {
        
        float brilhoSalvo = PlayerPrefs.GetFloat("Brilho", 1f);
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeMusica", 1f);

        sliderBrilho.value = brilhoSalvo;
        sliderVolumeMusica.value = volumeSalvo;

        AjustarBrilho(brilhoSalvo);
        AjustarVolumeMusica(volumeSalvo);

       
        sliderBrilho.onValueChanged.AddListener(AjustarBrilho);
        sliderVolumeMusica.onValueChanged.AddListener(AjustarVolumeMusica);

        painelOpcoes.SetActive(false);

      
     
        MusicPersistent.SetupIfNeeded(sharedMusic, otherMusic, sceneNameSharedA, sceneNameSharedB, otherSceneName, volumeSalvo);
    }

    public void AbrirFecharOpcoes()
    {
        painelOpcoes.SetActive(!painelOpcoes.activeSelf);
    }

    public void AjustarBrilho(float valor)
    {
       
        if (mascaraBrilho != null)
        {
            Color corAtual = mascaraBrilho.color;
            corAtual.a = 1f - valor; 
            mascaraBrilho.color = corAtual;
        }

        PlayerPrefs.SetFloat("Brilho", valor);
    }

    public void AjustarVolumeMusica(float volume)
    {
        
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

   
    public void VoltarAoMenuPrincipal()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("menu");
    }

   
    private class MusicPersistent : MonoBehaviour
    {
        public static MusicPersistent Instance;

        private AudioSource src;
        private AudioClip shared;
        private AudioClip other;
        private string sharedA;
        private string sharedB;
        private string otherScene;

       
        private string lastKey = "";
        private float lastTime = 0f;

      
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

            Instance.shared = sharedMusic;
            Instance.other = otherMusic;
            Instance.sharedA = sA;
            Instance.sharedB = sB;
            Instance.otherScene = sOther;
            Instance.SetVolume(initialVolume);

          
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
           
            if (src != null && src.isPlaying && src.clip != null)
            {
                lastTime = src.time;
            }

         
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
             
                desired = src.clip;
               
                key = lastKey;
            }

            if (desired == null)
            {
               
                if (src.isPlaying)
                {
                    src.Pause();
                    lastKey = "";
                }
                return;
            }

            
            if (src.clip == desired && src.isPlaying)
            {
                
                lastKey = key;
                return;
            }

          
            float startTime = 0f;
            if (key == "shared" && lastKey == "shared")
            {
              
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
