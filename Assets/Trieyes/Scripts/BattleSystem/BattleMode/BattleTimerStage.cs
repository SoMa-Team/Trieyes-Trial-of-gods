using UnityEngine;
using GameFramework;
using System;

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
        private float Phase2SpawnIntervalMultiplier = 2.25f;

        private float Phase1Time = 15f;
        private bool bisPhase1 = false;
        private bool bisPhase1Ended = false;
        private float Phase2Time = 15f;
        private bool bisPhase2 = false;
        private bool bisPhase2Ended = false;

        private float Phase3Time = 10f;

        private float StempedeChance = 3f;

        private bool isStempede = false;

        private int StempedeCountMin = 10;
        private int StempedeCountMax = 20;

        protected override void OnActivated()
        {
            difficulty.spawnMode = SpawnMode.Frequency;
            difficulty.battleLength = 50f;
            // spawnManager.SpawnIntervalMultiplier = StartSpawnIntervalMultiplier;
        }

        public override void Update()
        {
            if (!isActivated)
                return;

            if (Time.time - lastTick > ticDuration)
            {
                if (UnityEngine.Random.Range(0, 100) < StempedeChance)
                {
                    isStempede = true;
                    spawnManager.SpawnEnemy(UnityEngine.Random.Range(StempedeCountMin, StempedeCountMax));
                }

                mainCharacter.OnEvent(Utils.EventType.OnTick, mainCharacter);
                lastTick = Time.time;
            }

            // Phase1 체크
            if (Time.time - startTime >= Phase1Time && !bisPhase1)
            {
                Debug.LogWarning("Phase1");
                UpdateSpawnIntervalMultiplier(Phase1SpawnIntervalMultiplier);
                (View as BattleTimerStageView)?.CreateBeacon();
                bisPhase1 = true;
            }

            // Phase2 체크 (Phase1이 완료된 후)
            if (bisPhase1 && !bisPhase1Ended)
            {
                return;
            }

            if (bisPhase1Ended && Time.time - startTime >= Phase2Time && !bisPhase2)
            {
                Debug.LogWarning("Phase2");
                UpdateSpawnIntervalMultiplier(Phase2SpawnIntervalMultiplier);
                (View as BattleTimerStageView)?.CreateBeacon();
                bisPhase2 = true;
            }

            // Phase2가 완료되면 Phase3 체크
            if (bisPhase2 && !bisPhase2Ended)
            {
                return;
            }

            if (bisPhase2Ended && Time.time - startTime >= Phase3Time)
            {
                Debug.Log("Phase3 completed! Battle will end.");
                OnBattleClear();
            }
        }

        public override void OnBattleClear()
        {
            base.OnBattleClear();
        }

        private void UpdateSpawnIntervalMultiplier(float phase1SpawnIntervalMultiplier)
        {
            spawnManager.SpawnIntervalMultiplier = phase1SpawnIntervalMultiplier;
        }
        
        /// <summary>
        /// 비콘이 활성화되었을 때 호출되는 콜백
        /// </summary>
        /// <param name="beacon">활성화된 비콘</param>
        public void OnBeaconActivated(Beacon beacon)
        {
            Debug.Log($"Beacon activated in BattleTimerStage! Duration: {beacon.Progress * 100:F1}%");
            HandleBeaconActivation(beacon);
        }
        
        /// <summary>
        /// 비콘 활성화 시 처리할 로직
        /// </summary>
        /// <param name="beacon">활성화된 비콘</param>
        private void HandleBeaconActivation(Beacon beacon)
        {
            Debug.Log("Handling beacon activation...");
            
            // Phase1이 완료되지 않았다면 Phase1 완료 처리
            if (bisPhase1 && !bisPhase1Ended)
            {
                bisPhase1Ended = true;
                startTime = Time.time; // Phase2 타이머 시작
                Debug.Log("Phase1 completed! Starting Phase2 timer.");
            }
            // Phase2가 완료되지 않았다면 Phase2 완료 처리
            else if (bisPhase2 && !bisPhase2Ended)
            {
                bisPhase2Ended = true;
                startTime = Time.time; // Phase3 타이머 시작
                Debug.Log("Phase2 completed! Starting Phase3 timer.");
            }
        }
    }
}