using UnityEngine;

[CreateAssetMenu(fileName = "ControlsConfig", menuName = "Controls/ControlsConfig", order = 0)]
public class ControlsConfig : ScriptableObject
{
    public PlayerControls player1;
    public PlayerControls player2;

    [System.Serializable]
    public struct PlayerControls
    {
        public string name;

        public KeyCode left;
        public KeyCode right;
        public KeyCode jump;
        public KeyCode dodge;      // método de desvio
        public KeyCode action1;    // ataque 1 (atq/def)
        public KeyCode action2;    // ataque 2 (atq/def)
        public KeyCode action3;    // ataque 3 (atq/def)
    }
}
