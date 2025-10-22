using UnityEngine;

namespace CarePackage.Main
{
    public class GameManager : MonoBehaviour
    {
        public PlayerState Player;
        
        public static GameManager Instance;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
        }
    }
}
