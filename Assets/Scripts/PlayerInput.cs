using UnityEngine;

/// <summary>
/// Componente simples para anexar aos prefabs/players. 
/// Define qual playerId (1 ou 2) e fornece wrappers de input para os controllers.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Tooltip("Sete 1 para jogador 1, 2 para jogador 2")]
    public int playerId = 1;

    private void Reset()
    {
        playerId = 1;
    }

    public float GetHorizontal()
    {
        if (InputManager.Instance == null) return Input.GetAxisRaw("Horizontal");
        return InputManager.Instance.GetHorizontal(playerId);
    }

    public bool GetJumpDown()
    {
        if (InputManager.Instance == null) return Input.GetKeyDown(KeyCode.Space);
        return InputManager.Instance.GetJumpDown(playerId);
    }

    public bool GetDodgeDown()
    {
        if (InputManager.Instance == null) return Input.GetKeyDown(KeyCode.LeftShift);
        return InputManager.Instance.GetDodgeDown(playerId);
    }

    public bool GetAction1Down()
    {
        if (InputManager.Instance == null) return Input.GetMouseButtonDown(0);
        return InputManager.Instance.GetAction1Down(playerId);
    }

    public bool GetAction2Down()
    {
        if (InputManager.Instance == null) return Input.GetKeyDown(KeyCode.X);
        return InputManager.Instance.GetAction2Down(playerId);
    }

    public bool GetAction3Down()
    {
        if (InputManager.Instance == null) return Input.GetKeyDown(KeyCode.C);
        return InputManager.Instance.GetAction3Down(playerId);
    }
}
