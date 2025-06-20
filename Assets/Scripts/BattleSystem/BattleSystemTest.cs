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
        void Start()
        {
            var characterID = 0;
            var stageRound = 12;
            
            Pawn character = CharacterFactory.Instance.Create(characterID);
            Difficulty difficulty = Difficulty.GetByStageRound(stageRound);
            BattleStage battleStage = BattleStageFactory.Instance.Create(character, difficulty);
        }
    }
}