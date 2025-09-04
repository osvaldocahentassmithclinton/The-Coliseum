using UnityEngine;

public class MenuQuit : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit game!");
        Application.Quit();

        // No editor da Unity, para testar, voc� pode parar o play mode assim:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
