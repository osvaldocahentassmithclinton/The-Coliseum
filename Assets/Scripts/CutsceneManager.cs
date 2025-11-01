using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cutsceneCanvas;    // painel principal (CutsceneCanvas)
    public Image cutsceneImage;          // imagem da parte atual
    public TMP_Text subtitleText;        // legenda (TMP_Text)
    public Button skipButton;            // botão pular/avançar

    [Header("Cutscene Content")]
    public Sprite[] cutsceneSprites;     // sprites da cutscene
    [TextArea] public string[] subtitles; // legendas correspondentes

    [Header("Typing Effect")]
    public float charDelay = 0.03f;      // tempo entre letras

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private const string PREF_KEY_FIRST_LAUNCH = "HasSeenCutscene";

    void Start()
    {
        // botão de pular (na cutscene)
        skipButton.onClick.AddListener(OnSkipButtonClicked);

        // exibe apenas na primeira vez
        if (!PlayerPrefs.HasKey(PREF_KEY_FIRST_LAUNCH))
        {
            PlayerPrefs.SetInt(PREF_KEY_FIRST_LAUNCH, 1);
            PlayerPrefs.Save();
            StartCutscene();
        }
        else
        {
            cutsceneCanvas.SetActive(false);
        }
    }

    public void StartCutscene()
    {
        currentIndex = 0;
        cutsceneCanvas.SetActive(true);
        ShowCurrentPart();
    }

    private void ShowCurrentPart()
    {
        if (currentIndex >= cutsceneSprites.Length)
        {
            EndCutscene();
            return;
        }

        cutsceneImage.sprite = cutsceneSprites[currentIndex];
        string text = subtitles.Length > currentIndex ? subtitles[currentIndex] : "";

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSubtitle(text));
    }

    private IEnumerator TypeSubtitle(string text)
    {
        isTyping = true;
        subtitleText.text = "";

        foreach (char c in text)
        {
            subtitleText.text += c;
            float timer = 0f;
            while (timer < charDelay)
            {
                if (!isTyping) // se o jogador clicar pra pular
                {
                    subtitleText.text = text;
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        isTyping = false;
    }

    private void OnSkipButtonClicked()
    {
        if (isTyping)
        {
            // termina a digitação instantaneamente
            isTyping = false;
        }
        else
        {
            // avança pra próxima parte
            currentIndex++;
            ShowCurrentPart();
        }
    }

    private void EndCutscene()
    {
        cutsceneCanvas.SetActive(false);
    }

    // usado pelo botão do menu
    public void ReplayCutscene()
    {
        StartCutscene();
    }

    // opcional: limpar a flag da primeira vez
    public static void ResetFlag()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_FIRST_LAUNCH);
        PlayerPrefs.Save();
    }
}
