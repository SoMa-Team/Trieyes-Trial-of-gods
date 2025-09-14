using UnityEngine;
using CharacterSystem;
using GamePlayer;
using NodeStage;
using BattleSystem;
using Utils;
using UISystem;

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
        
        private Difficulty GetCurrentBattleDifficulty()
        {
            return Difficulty.GetByStageRound(stageRound);
        }
        
        private Difficulty GetCurrentBossDifficulty()
        {
            return Difficulty.GetByStageRound(stageRound, true);
        }

        public void StartNextStage(StageType stageType, Character mainCharacter)
        {
            stageRound++;
            switch (stageType)
            { 
                case StageType.Battle:
                {
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentBattleDifficulty());
                    break;
                }
                case StageType.Boss:
                case StageType.Elite:
                {
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentBossDifficulty());
                    break;
                }
                case StageType.StartCard:
                    StartCardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StartRelic:
                    StartRelicStage.Instance.Activate(mainCharacter);
                    break;
                // case StageType.CampFire:
                //     CampFireStage.Instance.Activate(mainCharacter);
                //     break;
                case StageType.CardEnhancement:
                    CardEnhancementStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.Shop:
                    ShopSceneManager.Instance.Activate(mainCharacter, GetCurrentBattleDifficulty());
                    break;
                case StageType.BattleReward:
                    BattleStageFactory.Instance.Deactivate(BattleStage.now);
                    BattleRewardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StickerEvent:
                    StickerStage.Instance.Activate(mainCharacter);
                    break;
            }
        }
    }
}