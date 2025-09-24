using UnityEngine;
using GameFramework;

namespace BattleSystem
{
    /// <summary>
    /// 타이머 기반 전투 스테이지
    /// 현재 BattleStage의 모든 기능을 그대로 유지합니다.
    /// </summary>
    public class BattleTimerStage : BattleStage<BattleTimerStage>
    {
        // 현재 BattleStage의 모든 기능을 상속받아 사용
        // 추가적인 구현이 필요한 경우 OnActivated() 또는 OnDeactivated()를 오버라이드

        // 페이즈 증가 마다 SpawnIntervalMultiplier 증가
        private float Phase1SpawnIntervalMultiplier = 1.5f;
        private float Phase2SpawnIntervalMultiplier = 2.5f;

        private float Phase1Time = 15f;
        private bool isPhase1 = false;
        private float Phase2Time = 30f;
        private bool isPhase2 = false;

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

            if (Time.time - startTime >= Phase1Time && !isPhase1)
            {
                Debug.LogWarning("Phase1");
                spawnManager.SpawnIntervalMultiplier = Phase1SpawnIntervalMultiplier;
                isPhase1 = true;
            }
            if (Time.time - startTime >= Phase2Time && !isPhase2)
            {
                Debug.LogWarning("Phase2");
                spawnManager.SpawnIntervalMultiplier = Phase2SpawnIntervalMultiplier;
                isPhase2 = true;
            }
        }

        protected override void OnActivated()
        {
            difficulty.spawnMode = SpawnMode.Frequency;
            difficulty.battleLength = 50f;
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }
    }
}
