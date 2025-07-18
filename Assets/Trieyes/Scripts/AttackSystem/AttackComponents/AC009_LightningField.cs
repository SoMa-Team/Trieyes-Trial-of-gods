using AttackSystem;
using CharacterSystem;
using UnityEngine;
using BattleSystem;
using System;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 번개 장판 효과
    /// 플레이어 주변에 자기장을 형성하여 닿는 적에게 피해를 주고, 이동속도가 증가합니다.
    /// AC104를 소환하여 따라다니는 자기장과 버프 기능을 구현합니다.
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

        // AC104 소환 설정
        [Header("AC104 소환 설정")]
        private const int AC104_ID = 16; // AC104의 ID
        private const int AC1001_ID = 13;
        public float moveSpeedBoostMultiplier = 1f; // 이동속도 증가 배율
        public float moveSpeedBoostDuration; // 이동속도 증가 지속시간

        // 번개 장판 상태 관리
        private LightningFieldState fieldState = LightningFieldState.None;
        private float fieldTimer = 0f;

        // AC104 인스턴스 관리
        private Attack summonedAC104;

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
            // AC104 소환하여 따라다니는 자기장 생성
            SummonAC104();

            // AC1000 버프로 이동속도 증가
            ApplyMoveSpeedBuff();
        }

        private void ApplyMoveSpeedBuff()
        {
            // 새로운 BUFF 클래스 사용 - 이동속도 증가
            var speedBuffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseMoveSpeed,
                attack = attack,
                targets = new List<Pawn> { attack.attacker },
                buffValue = 10,
                buffMultiplier = moveSpeedBoostMultiplier,
                buffDuration = moveSpeedBoostDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var speedBuff = new BUFF();
            speedBuff.Activate(speedBuffInfo);
        }

        private void SummonAC104()
        {
            Debug.Log("<color=yellow>[AC009] AC104 소환하여 따라다니는 자기장 생성!</color>");
            
            // AC104 생성
            summonedAC104 = AttackFactory.Instance.ClonePrefab(AC104_ID);
            BattleStage.now.AttachAttack(summonedAC104);
            
            // AC104 설정 (하드코딩)
            var ac104Component = summonedAC104.components[0] as AC104_FollowingField;
            if (ac104Component != null)
            {
                ac104Component.fieldRadius = lightningFieldRadius;
                ac104Component.fieldDamage = lightningFieldDamage;
                ac104Component.fieldTickInterval = 0.5f;
                ac104Component.fieldDuration = lightningFieldDuration;
                ac104Component.followPlayer = true;
                ac104Component.followDistance = 0f;
                ac104Component.followOffset = Vector2.zero;
            }
            
            // AC104 활성화
            summonedAC104.Activate(attack.attacker, Vector2.zero);
            
            Debug.Log("<color=green>[AC009] AC104 따라다니는 자기장 활성화 완료!</color>");
        }

        private void DeactivateField()
        {
            // AC104 비활성화
            if (summonedAC104 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC104);
                summonedAC104 = null;
            }
            
            Debug.Log("<color=cyan>[AC009] AC104 따라다니는 자기장 종료!</color>");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // AC104 정리
            if (summonedAC104 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC104);
                summonedAC104 = null;
            }
            
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
        }
    }
} 