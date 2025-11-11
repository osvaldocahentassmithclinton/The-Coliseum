using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cutsceneCanvas;    
    public Image cutsceneImage;         
    public TMP_Text subtitleText;     
    public Button skipButton;          

    [Header("Cutscene Content")]
    public Sprite[] cutsceneSprites;   
    [TextArea] public string[] subtitles; 

    [Header("Typing Effect")]
    public float charDelay = 0.03f;     

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private const string PREF_KEY_FIRST_LAUNCH = "HasSeenCutscene";

    void Start()
    {
        
        skipButton.onClick.AddListener(OnSkipButtonClicked);

     
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
                if (!isTyping)
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
          
            isTyping = false;
        }
        else
        {
            
            currentIndex++;
            ShowCurrentPart();
        }
    }

    private void EndCutscene()
    {
        cutsceneCanvas.SetActive(false);
    }

  
    public void ReplayCutscene()
    {
        StartCutscene();
    }

 
    public static void ResetFlag()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_FIRST_LAUNCH);
        PlayerPrefs.Save();
    }
}
