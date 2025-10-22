using UnityEngine;

namespace CarePackage.Main
{
    public enum GameState
    {
        Gameplay,
        Dialogue,
        Paused,
    }
    public class GameManager : MonoBehaviour
    {
        public PlayerState Player;
        
        public static GameManager Instance;
        public GameState CurrentState { get; private set; } = GameState.Gameplay;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void SetGameState(GameState newState)
        {
            CurrentState = newState;
        }

        public bool IsDialogueActive => CurrentState == GameState.Dialogue;
    }
}
