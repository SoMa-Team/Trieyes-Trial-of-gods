using UnityEngine;
using CharacterSystem;
using GamePlayer;
using NodeStage;
using BattleSystem;

namespace GameFramework
{
    public class InGameManager : MonoBehaviour
    {
        private Player player;

        private Difficulty currentDifficulty = new Difficulty();
        public static InGameManager Instance { get; private set; }
        
        // ===== 스테이지 간 노드 개수 관리 =====
        [Header("스테이지 간 노드 개수 관리")]

        private const int _startLevelNodeCount = 5;
        private int _stageNodeCount;
        public int bossStageLeftCount = 0;

        private int _nextRoundMinNodeCount = 1;
        private int _nextRoundMaxNodeCount = 2;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            _stageNodeCount = _startLevelNodeCount;
            bossStageLeftCount = _stageNodeCount;

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        
        public Difficulty GetCurrentDifficulty()
        {
            return currentDifficulty;
        }

        public void ResetStageNodeCount()
        {
            _stageNodeCount = _startLevelNodeCount;
            bossStageLeftCount = _stageNodeCount;
        }

        /// <summary>
        /// 다음 스테이지 노드 개수를 설정합니다.
        /// </summary>
        public void SetNextStageNodeCount()
        {
            _stageNodeCount += Random.Range(_nextRoundMinNodeCount, _nextRoundMaxNodeCount + 1);
            bossStageLeftCount = _stageNodeCount;
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
                case StageType.BattleTimerStage:
                    BattleStageFactory.Instance.Create(mainCharacter, BattleMode.Timer);
                    break;
                case StageType.BattleBreakThroughStage:
                    BattleStageFactory.Instance.Create(mainCharacter, BattleMode.BreakThrough);
                    break;
                case StageType.BattleEscapeStage:
                    BattleStageFactory.Instance.Create(mainCharacter, BattleMode.Escape);
                    break;
                case StageType.Boss:
                    BattleStageFactory.Instance.Create(mainCharacter, BattleMode.Boss);
                    break;
                case StageType.Elite:
                    BattleStageFactory.Instance.Create(mainCharacter, BattleMode.BreakThrough);
                    break;
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
                    BattleRewardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.SkillReward:
                    SkillRewardStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StatEvent:
                    StatEventStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.StickerEvent:
                    StickerStage.Instance.Activate(mainCharacter);
                    break;
                case StageType.GameOver:
                    GameOverStage.Instance.Activate(mainCharacter);
                    break;
                default:
                    Debug.LogError($"StageType {stageType} is not supported");
                    break;
            }
        }
    }
}