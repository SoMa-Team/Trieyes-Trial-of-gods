using UnityEngine;
using GameFramework;
using System.Collections;
using PrimeTween;

namespace BattleSystem
{
    /// <summary>
    /// 보스 전투 스테이지
    /// 기본 상속만 받는 구조로 생성
    /// </summary>
    public class BattleBossStage : BattleStage<BattleBossStage>
    {
        // 기본 상속만 받는 구조
        // 필요시 OnActivated() 또는 OnDeactivated()를 오버라이드하여 구현
        public override void Update()
        {
            if (!isActivated)
                return;
        }
        
        protected override void OnActivated()
        {
            difficulty.spawnMode = SpawnMode.Once;
        }
    }
}
