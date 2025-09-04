using UnityEngine;
using CharacterSystem;
using GamePlayer;

namespace GameFramework
{
    public class InGameManager : MonoBehaviour
    {
        private Character mainCharacter;
        private Player player;
        public static InGameManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        public void OpenSelectCharacterPopup()
        {
            
        }
        
        public void OpenSelectStartRelicPopup()
        {
            
        }

        public void OpenSelectStartCardPopup()
        {
            
        }

        public void OpenSelectStagePopup()
        {
            
        }
    }
}