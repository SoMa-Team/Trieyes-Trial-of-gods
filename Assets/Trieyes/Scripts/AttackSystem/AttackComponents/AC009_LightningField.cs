using AttackSystem;
using CharacterSystem;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 번개 장판 효과
    /// 플레이어 주변에 자기장을 형성하여 닿는 적에게 피해를 주고, 이동속도가 증가합니다.
    /// AC100을 소환하여 자기장과 버프 기능을 구현합니다.
    /// </summary>
    public class AC009_LightningField : AttackComponent
    {
        [Header("번개 장판 설정")]
        public float lightningFieldDamage = 30f;
        public float lightningFieldRadius = 2.5f;
        public float lightningFieldDuration = 3f;
        public float lightningFieldDelay = 0.1f;

        [Header("VFX 설정")]
        public float vfxDuration = 0.4f;

        // AC100 소환 설정
        [Header("AC100 소환 설정")]
        private const int AC100_ID = 10; // AC100의 ID
        public float moveSpeedBoostMultiplier = 1.5f; // 이동속도 증가 배율
        public float moveSpeedBoostDuration = 5f; // 이동속도 증가 지속시간

        // 번개 장판 상태 관리
        private LightningFieldState fieldState = LightningFieldState.None;
        private float fieldTimer = 0f;

        // AC100 인스턴스 관리
        private Attack summonedAC100;

        // 번개 장판 상태 열거형
        private enum LightningFieldState
        {
            None,
            Preparing,
            Active,
            Deactivating,
            Finished
        }



        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
            
            // 번개 장판 시작
            StartLightningField();
        }

        private void StartLightningField()
        {
            fieldState = LightningFieldState.Preparing;
            fieldTimer = 0f;
            
            Debug.Log("<color=cyan>[AC009] 번개 자기장 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // 번개 장판 처리
            ProcessLightningField();
        }

        private void ProcessLightningField()
        {
            switch (fieldState)
            {
                case LightningFieldState.None:
                    break;

                case LightningFieldState.Preparing:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= lightningFieldDelay)
                    {
                        fieldState = LightningFieldState.Active;
                        fieldTimer = 0f;
                        ActivateField();
                    }
                    break;

                case LightningFieldState.Active:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= lightningFieldDuration)
                    {
                        fieldState = LightningFieldState.Deactivating;
                        fieldTimer = 0f;
                        DeactivateField();
                    }
                    break;

                case LightningFieldState.Deactivating:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= vfxDuration)
                    {
                        fieldState = LightningFieldState.Finished;
                    }
                    break;

                case LightningFieldState.Finished:
                    fieldState = LightningFieldState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateField()
        {
            // AC100 소환하여 자기장 생성
            SummonAC100();
        }

        private void SummonAC100()
        {
            Debug.Log("<color=yellow>[AC009] AC100 소환하여 자기장 생성!</color>");
            
            // AC100 생성
            summonedAC100 = AttackFactory.Instance.ClonePrefab(AC100_ID);
            BattleStage.now.AttachAttack(summonedAC100);
            
            // AC100 설정
            var ac100Component = summonedAC100.components[0] as AC100_AOE;
            if (ac100Component != null)
            {
                // AOE 영역 공격 설정
                ac100Component.createAreaAttack = true;
                ac100Component.areaAttackRadius = lightningFieldRadius;
                ac100Component.areaAttackDamage = lightningFieldDamage;
                ac100Component.areaAttackTickInterval = 0.5f;
                
                // 이동속도 증가 버프 설정
                ac100Component.additionalBuffType = AdditionalBuffType.MoveSpeedBoost;
                ac100Component.additionalBuffDuration = moveSpeedBoostDuration;
                ac100Component.additionalBuffMultiplier = moveSpeedBoostMultiplier;
                
                // 도트 설정
                ac100Component.dotType = DOTType.Lightning;
                ac100Component.dotCollisionType = DOTCollisionType.AreaRect;
                ac100Component.dotDuration = lightningFieldDuration;
                ac100Component.dotInterval = 0.5f;

                ac100Component.dotWidth = lightningFieldRadius;
                ac100Component.dotHeight = lightningFieldRadius;
            }
            
            // AC100 활성화
            summonedAC100.Activate(attack.attacker, Vector2.zero);
            
            Debug.Log("<color=green>[AC009] AC100 자기장 활성화 완료!</color>");
        }

        private void DeactivateField()
        {
            // AC100 비활성화
            if (summonedAC100 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC100);
                summonedAC100 = null;
            }
            
            Debug.Log("<color=cyan>[AC009] 번개 자기장 종료!</color>");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // AC100 정리
            if (summonedAC100 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC100);
                summonedAC100 = null;
            }
            
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
        }
    }
} 