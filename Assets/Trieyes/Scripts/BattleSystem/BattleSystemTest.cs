using AttackSystem;
using CharacterSystem;
using UnityEngine;
using Utils;

namespace BattleSystem
{
    /// <summary>
    /// 전투 시스템의 테스트를 위한 임시 클래스
    /// 구현 완료 후 삭제 예정입니다.
    /// </summary>
    public class BattleSystemTest: MonoBehaviour
    {
        public BattleStage battleStage;
        public AttackData attackData;
        
        public Pawn character;
        // public Difficulty difficulty;

        void Start()
        {
            var characterID = 0;
            var stageRound = 12;

            Pawn character = this.character;
            character.basicAttack = attackData;
            Difficulty difficulty = Difficulty.GetByStageRound(stageRound);
            BattleStage battleStage = BattleStageFactory.Instance.Create(character, difficulty);
        }
    }
}