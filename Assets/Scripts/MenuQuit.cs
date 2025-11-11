using UnityEngine;

public class MenuQuit : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit game!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
