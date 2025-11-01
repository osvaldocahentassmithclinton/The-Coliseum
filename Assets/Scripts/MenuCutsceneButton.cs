using UnityEngine;

public class MenuShowCutscene : MonoBehaviour
{
    public CutsceneManager cutsceneManager;

    public void OnShowCutsceneClicked()
    {
        if (cutsceneManager != null)
            cutsceneManager.StartCutscene();
    }
}
