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
            switch (stageType)
            {
                case StageType.StartCard:
                    StartCardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StartRelic:
                    StartRelicStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.Battle:
                    stageRound++;
                    Player.Instance.bossStageLeftCount--;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentBattleDifficulty());
                    break;
                case StageType.Boss:
                    stageRound++;
                    Player.Instance.bossStageLeftCount = 3;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentBossDifficulty());
                    break;
                case StageType.Elite:
                    stageRound++;
                    Player.Instance.bossStageLeftCount--;
                    BattleStageFactory.Instance.Create(mainCharacter, GetCurrentBossDifficulty());
                    break;
                case StageType.CampFire:
                    Player.Instance.bossStageLeftCount--;
                    CampfireStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.CardEnhancement:
                    Player.Instance.bossStageLeftCount--;
                    CardEnhancementStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.Shop:
                    Player.Instance.bossStageLeftCount--;
                    ShopSceneManager.Instance.Activate(mainCharacter, GetCurrentBattleDifficulty());
                    break;
                case StageType.BattleReward:
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