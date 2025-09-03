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
    public class AC008_LightningField : AttackComponent
    {
        [Header("번개 장판 설정")]
        public float lightningFieldDamage = 30f;
        public float lightningFieldRadius = 2.5f;
        public float lightningFieldDuration = 3f;
        public float lightningFieldDelay = 0.1f;  

        // AC105 FollowingField VFX 설정
        [Header("AC105 FollowingField VFX Settings")]
        [SerializeField] private GameObject fieldVFXPrefab; // 필드 VFX 프리팹 (AC105에 전달용)
        
        // 버프 설정
        [Header("버프 설정")]
        public float moveSpeedBoostMultiplier; // 이동속도 증가 배율
        private float moveSpeedBoostDuration; // 이동속도 증가 지속시간

        // 번개 장판 상태 관리
        private LightningFieldState fieldState = LightningFieldState.None;
        private float fieldTimer = 0f;

        // AC105 인스턴스 관리
        private Attack summonedAC105;

        // 번개 장판 상태 열거형
        private enum LightningFieldState
        {
            None,
            Preparing,
            Active,
            Deactivating,
            Finished
        }

        public AttackData followingFieldData;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fieldState = LightningFieldState.None;
            moveSpeedBoostDuration = lightningFieldDuration;
            fieldTimer = 0f;
            
            // 번개 장판 시작
            StartLightningField();
        }

        private void StartLightningField()
        {
            fieldState = LightningFieldState.Preparing;
            fieldTimer = 0f;
            
            //Debug.Log("<color=cyan>[AC009] 번개 자기장 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
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
                    fieldState = LightningFieldState.Finished;
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
                target = attack.attacker,
                buffMultiplier = moveSpeedBoostMultiplier,
                buffDuration = moveSpeedBoostDuration,
            };

            var speedBuff = new BUFF();
            speedBuff.Activate(speedBuffInfo);
        }

        private void SummonAC104()
        {
            //Debug.Log("<color=yellow>[AC009] AC105 소환하여 따라다니는 자기장 생성!</color>");
            
            // 기존 AC105가 있다면 정리
            if (summonedAC105 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC105);
                summonedAC105 = null;
            }
            
            // AC105 생성
            summonedAC105 = AttackFactory.Instance.Create(followingFieldData, attack.attacker, null, Vector2.zero);
            
            // AC105 설정 (하드코딩)
            var ac105Component = summonedAC105.components[0] as AC105_FollowingField;
            if (ac105Component != null)
            {
                ac105Component.fieldRadius = lightningFieldRadius;
                ac105Component.fieldDamage = lightningFieldDamage;
                ac105Component.fieldDuration = lightningFieldDuration;
                ac105Component.fieldTickInterval = 0.5f;
                ac105Component.followPlayer = true;
                ac105Component.followDistance = 0f;
                
                // 필드 VFX 프리팹 전달
                ac105Component.fieldVFXPrefab = fieldVFXPrefab;
            }
            
            //Debug.Log("<color=green>[AC009] AC104 따라다니는 자기장 활성화 완료!</color>");
        }

        private void DeactivateField()
        {
            // AC104 비활성화
            if (summonedAC105 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC105);
                summonedAC105 = null;
            }
            
            //Debug.Log("<color=cyan>[AC009] AC104 따라다니는 자기장 종료!</color>");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // AC105 정리
            if (summonedAC105 != null)
            {
                AttackFactory.Instance.Deactivate(summonedAC105);
                summonedAC105 = null;
            }
            
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
        }
    }
} 