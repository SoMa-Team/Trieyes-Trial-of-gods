using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 탈출형 전투 스테이지
    /// 기본 상속만 받는 구조로 생성
    /// </summary>

    public class BattleEscapeStage : BattleStage<BattleEscapeStage>
    {
        // 기본 상속만 받는 구조
        // 필요시 OnActivated() 또는 OnDeactivated()를 오버라이드하여 구현
        public override void Update()
        {
            if (!isActivated)
                return;
            
            if (Time.time - startTime >= difficulty.battleLength)
            {
                OnBattleClear();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }
    }
}
