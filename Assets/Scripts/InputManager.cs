using System;
using UnityEngine;

/// <summary>
/// Singleton que lê o ControlsConfig e expõe métodos para checar entradas por jogador.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Tooltip("Arraste aqui o ScriptableObject ControlsConfig criado")]
    public ControlsConfig config;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (config == null)
            Debug.LogWarning("InputManager: ControlsConfig não atribuído no inspector.");
    }

    // retorna -1/0/1 baseado nas teclas definidas
    public float GetHorizontal(int player)
    {
        var ctrlOpt = GetControls(player);
        if (!ctrlOpt.HasValue) return 0f;

        var ctrl = ctrlOpt.Value;
        bool left = Input.GetKey(ctrl.left);
        bool right = Input.GetKey(ctrl.right);
        if (left && !right) return -1f;
        if (right && !left) return 1f;
        return 0f;
    }

    // genéricos para botões (usa selector para escolher a KeyCode do struct)
    public bool GetButtonDown(int player, Func<ControlsConfig.PlayerControls, KeyCode> selector)
    {
        var ctrlOpt = GetControls(player);
        if (!ctrlOpt.HasValue) return false;
        return Input.GetKeyDown(selector(ctrlOpt.Value));
    }

    public bool GetButton(int player, Func<ControlsConfig.PlayerControls, KeyCode> selector)
    {
        var ctrlOpt = GetControls(player);
        if (!ctrlOpt.HasValue) return false;
        return Input.GetKey(selector(ctrlOpt.Value));
    }

    public bool GetButtonUp(int player, Func<ControlsConfig.PlayerControls, KeyCode> selector)
    {
        var ctrlOpt = GetControls(player);
        if (!ctrlOpt.HasValue) return false;
        return Input.GetKeyUp(selector(ctrlOpt.Value));
    }

    // helpers
    public bool GetJumpDown(int player) => GetButtonDown(player, c => c.jump);
    public bool GetDodgeDown(int player) => GetButtonDown(player, c => c.dodge);
    public bool GetAction1Down(int player) => GetButtonDown(player, c => c.action1);
    public bool GetAction2Down(int player) => GetButtonDown(player, c => c.action2);
    public bool GetAction3Down(int player) => GetButtonDown(player, c => c.action3);

    private Nullable<ControlsConfig.PlayerControls> GetControls(int player)
    {
        if (config == null) return null;
        return player == 1 ? (Nullable<ControlsConfig.PlayerControls>)config.player1
                            : (Nullable<ControlsConfig.PlayerControls>)config.player2;
    }
}
