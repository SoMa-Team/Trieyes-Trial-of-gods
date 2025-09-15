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
        
        private Difficulty GetCurrentDifficulty()
        {
            return Difficulty.GetByStageRound(stageRound);
        }

        public void StartNextStage(StageType stageType, Character mainCharacter)
        {
            switch (stageType)
            {
                case StageType.Battle:
                    stageRound++;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.Boss:
                    stageRound++;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.Elite:
                    stageRound++;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.StartCard:
                    StartCardStage.Instance.Activate(mainCharacter);
                    break;
                // case StageType.StartRelic:
                //     StartRelicStage.Instance.Activate(mainCharacter);
                //     break;
                case StageType.CampFire:
                    CampfireStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.CardEnhancement:
                    CardEnhancementStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.Shop:
                    ShopSceneManager.Instance.Activate(mainCharacter, GetCurrentDifficulty());
                    break;
                case StageType.BattleReward:
                    BattleStageFactory.Instance.Deactivate(BattleStage.now);
                    BattleRewardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.SkillReward:
                    SkillRewardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StickerEvent:
                    StickerStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.GameOver:
                    Time.timeScale = 0;
                    GameOverStage.Instance.Activate(mainCharacter);
                    break;
            }
        }
    }
}