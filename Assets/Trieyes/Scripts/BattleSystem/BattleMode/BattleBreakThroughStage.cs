using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 돌파형 전투 스테이지
    /// 기본 상속만 받는 구조로 생성
    /// </summary>

    public class BattleBreakThroughStage : BattleStage<BattleBreakThroughStage>
    {
        // 기본 상속만 받는 구조
        // 필요시 OnActivated() 또는 OnDeactivated()를 오버라이드하여 구현
        [Header("돌파형 전투 스테이지 파라미터")]
        private int breakThroughCount = 0;

        public void BreakThroughCountDown()
        {
            breakThroughCount--;
        }

        public int GetBreakThroughCount()
        {
            return breakThroughCount;
        }
        
        public override void Update()
        {
            if (!isActivated)
                return;
            
            if (breakThroughCount <= 0)
            {
                OnBattleClear();
            }
        }
        
        protected override void OnActivated()
        {
            base.OnActivated();

            // Difficulty에서 breakThroughCount를 가져오기
            breakThroughCount = difficulty.GetBreakThroughCount();
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }
    }
}
