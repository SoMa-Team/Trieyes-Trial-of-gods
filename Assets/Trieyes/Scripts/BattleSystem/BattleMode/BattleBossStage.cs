using UnityEngine;
using GameFramework;
using System.Collections;

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
        public bool isBossDead = false;
        public override void Update()
        {
            if (!isActivated)
                return;
            
            // BOSS.cs가 따로 생긴다면 BOSS에서 죽을 때 이 Stage의 flag 값을 활성화 시켜줘야 함
            if (isBossDead)
            {
                OnBattleClear();
            }
        }
        
        protected override void OnActivated()
        {
            difficulty.spawnMode = SpawnMode.Once;
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }
    }
}
