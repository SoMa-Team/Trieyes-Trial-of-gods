using System;
using CharacterSystem;
using System.Collections.Generic;
using AttackSystem;
using GameFramework;
using NodeStage;
using UnityEngine;
using System.Linq;

namespace BattleSystem
{
    /// <summary>
    /// 타이머 기반 전투 스테이지
    /// 현재 BattleStage의 모든 기능을 그대로 유지합니다.
    /// </summary>
    public class BattleTimer : BattleStage<BattleTimer>
    {
        // 현재 BattleStage의 모든 기능을 상속받아 사용
        // 추가적인 구현이 필요한 경우 OnActivated() 또는 OnDeactivated()를 오버라이드
        public override void Update()
        {
            if (!isActivated)
                return;
            
            if (Time.time - startTime >= difficulty.battleLength)
            {
                OnBattleClear();
            }

            if (Time.time - lastTick > ticDuration)
            {
                mainCharacter.OnEvent(Utils.EventType.OnTick, mainCharacter);
                lastTick = Time.time;
            }
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }
    }
}
