using UnityEngine;
using CharacterSystem;
using GamePlayer;
using NodeStage;
using BattleSystem;
using Utils;

namespace GameFramework
{
    public class InGameManager : MonoBehaviour
    {
        private Player player;
        private int stageRound;
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
        
        private Difficulty GetCurrentDifficulty()
        {
            return Difficulty.GetByStageRound(stageRound);
        }

        public void StartNextStage(StageType stageType, Character mainCharacter)
        {
            stageRound++;
            switch (stageType)
            {
                case StageType.Battle:
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.Boss:
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.Elite:
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                
            }
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