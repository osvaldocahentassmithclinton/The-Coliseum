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
        public KeyCode dodge;      
        public KeyCode action1;    
        public KeyCode action2;    
        public KeyCode action3;    
    }
}
