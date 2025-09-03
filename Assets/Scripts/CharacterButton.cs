using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public string characterName;

    [SerializeField] private GameObject selectionBorder;

    private Button button;

    private CharacterSelectionManager manager;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        SetSelected(false);
    }

    public void SetManager(CharacterSelectionManager mgr)
    {
        manager = mgr;
    }

    private void OnClick()
    {
        if (manager != null)
        {
            manager.PreSelectCharacter(characterName);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(selected);
        }
    }
}
