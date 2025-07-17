using AttackSystem;
using UnityEngine;
using System;
using System.Collections;
using CharacterSystem;
using CharacterSystem.Enemies;
using Stats;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    public enum DOTType
    {
        Fire,
        Ice,
        Lightning,
        Poison,
        Bleed,
    }

    public enum DOTCollisionType
    {
        Individual, // 모기처럼 한명한테 붙어서 도트 주는 놈
        AreaRect, // 네모 장판 범위 내 모든 적에게 도트 주는 놈
        AreaCircle, // 원형 장판 범위 내 모든 적에게 도트 주는 놈
    }

    public enum AOEMode
    {
        SingleHit, // 1회성 AOE 공격
        MultiHit,  // 지속적 AOE 공격
    }

    public enum AdditionalBuffType
    {
        None,
        SpeedBoost,
        AttackSpeedBoost,
        AttackPowerBoost,
        DefenseBoost,
        CriticalChanceBoost,
        CriticalDamageBoost,
        MoveSpeedBoost,
    }

    /// <summary>
    /// 도트 효과 적용
    /// 공격에 맞은 적에게 지속적으로 화상데미지(**도트**)를 입힙니다
    /// </summary>
    public class AC100_AOE  : AttackComponent
    {   
        // 도트 타입 ENUM
        public DOTType dotType;

        // 도트 콜리전 타입 (개별 Pawn에 적용되는지, Box Collider로 장판처럼 적용되는지)
        public DOTCollisionType dotCollisionType;
        public int dotDamage = 10;
        public float dotDuration = 10f;

        public float currentDotDuration = 0f;
        public float dotInterval = 1f;

        // 도트 콜리전 타입이 Individual일 때 사용되는 값
        public Pawn target;

        // 도트 콜리전 타입이 AreaRect일 때 사용되는 값
        public float dotWidth = 1f;
        public float dotHeight = 1f;

        // 도트 콜리전 타입이 AreaCircle일 때 사용되는 값
        public float dotRadius = 1f;
        public float dotAngle = 180f;
        public int dotSegments = 8;

        // 추가 버프 설정
        [Header("추가 버프 설정")]
        public AdditionalBuffType additionalBuffType = AdditionalBuffType.None;
        public float additionalBuffDuration = 5f;
        public float additionalBuffMultiplier = 1.5f;
        public int additionalBuffValue = 10;

        // AOE 영역 공격 설정
        [Header("AOE 영역 공격 설정")]
        public AOEMode aoeMode;
        public bool createAreaAttack = false;
        public float areaAttackRadius = 1f;
        public float areaAttackDamage = 20f;
        public float areaAttackTickInterval = 0.5f;
        private float areaAttackTimer = 0f;
        private List<Enemy> areaAttackTargets = new List<Enemy>(10);
        private bool buffApplied = false; // 버프 적용 여부 플래그

        // AOE VFX 설정
        [Header("AOE VFX 설정")]
        public GameObject areaAttackVFXPrefab;
        public float areaAttackVFXDuration = 0.3f;

        public override void Activate(Attack attack, Vector2 direction)
        {
            if (attack.target != null)
            {
                dotCollisionType = DOTCollisionType.Individual;
                target = attack.target;
            }
            else
            {
                dotCollisionType = DOTCollisionType.AreaRect;
            }

            // AOE 영역 공격이 활성화된 경우 AOE 로직 시작
            if (createAreaAttack)
            {
                StartAreaAttack();
            }
        }

        private void StartAreaAttack()
        {
            areaAttackTimer = 0f;
            areaAttackTargets.Clear();
            buffApplied = false;
            Debug.Log("<color=yellow>[AC100] AOE 영역 공격 시작!</color>");
        }

        private void DOTHandlerByCollisionType(DOTCollisionType dotCollisionType)
        {
            switch (dotCollisionType)
            {
                case DOTCollisionType.Individual:
                    DOTHandlerByIndividual();
                    break;
                case DOTCollisionType.AreaRect:
                    DOTHandlerByAreaRect();
                    break;
                case DOTCollisionType.AreaCircle:
                    DOTHandlerByAreaCircle();
                    break;
                default:
                    break;
            }
        }

        private void DOTHandlerByIndividual()
        {
            AttackResult result = new AttackResult();
            result.attacker = attack.attacker;
            result.totalDamage = dotDamage;

            target.ApplyDamage(result);

            // 추가 버프 적용
            ApplyAdditionalBuffEffect(target);

            if (target.isDead)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
        }

        private void DOTHandlerByAreaRect()
        {
            // 플레이어 주변 AOE 영역 공격 형성
            if (createAreaAttack)
            {
                ProcessAreaAttack();
            }
            
            // AOE 영역 공격이 활성화된 경우 도트 데미지는 AOE에서 처리하므로 여기서는 추가 처리하지 않음
        }

        private void DOTHandlerByAreaCircle()
        {
            throw new NotImplementedException();
        }

        private void ProcessAreaAttack()
        {
            if (aoeMode == AOEMode.SingleHit)
            {
                ApplyAreaAttackDamage();
                Debug.Log("<color=yellow>[AC100] SingleHit AOE 공격 실행 완료!</color>");
                AttackFactory.Instance.Deactivate(attack);
                return;
            }
            else if (dotCollisionType == DOTCollisionType.AreaRect)
            {
                // MultiHit 모드: 기존 지속적 AOE 로직
                areaAttackTimer += Time.deltaTime;
                
                if (areaAttackTimer >= areaAttackTickInterval)
                {
                    DetectEnemiesInAreaAttackRect();
                    ApplyAreaAttackDamage();
                    areaAttackTimer = 0f;
                }
            }
            else if (dotCollisionType == DOTCollisionType.AreaCircle)
            {
                DetectEnemiesInAreaAttackCircle();
                ApplyAreaAttackDamage();
            }
        }

        private void DetectEnemiesInAreaAttackCircle()
        {
            throw new NotImplementedException();
        }


        private void ApplyAreaAttackDamage()
        {
            // 플레이어 주변 적 탐지
            DetectEnemiesInAreaAttackRect();
            
            // 탐지된 적들에게 데미지 적용
            for (int i = 0; i < areaAttackTargets.Count; i++)
            {
                Pawn enemy = areaAttackTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    ApplyDamageToEnemy(enemy);
                }
            }
            
            // AOE VFX 생성
            CreateAreaAttackVFX();
            
            // 플레이어에게 버프 적용 (한 번만 적용)
            if (additionalBuffType != AdditionalBuffType.None && !buffApplied)
            {
                ApplyAdditionalBuffEffect(attack.attacker);
                buffApplied = true;
            }
        }

        private void DetectEnemiesInAreaAttackRect()
        {
            areaAttackTargets.Clear();
            
            // target 위치 기준으로 AOE 범위 탐지 (SingleHit 모드)
            Vector2 aoePositionStart = attack.target != null ? (Vector2)attack.target.transform.position : (Vector2)attack.attacker.transform.position;
            Vector2 aoePositionEnd = aoePositionStart + new Vector2(dotWidth, dotHeight);

            areaAttackTargets = BattleStage.now.GetEnemiesInRectRange(aoePositionStart, aoePositionEnd);
        }

        private void ApplyDamageToEnemy(Pawn enemy)
        {
            var attackResult = AttackResult.Create(attack, enemy);
            attackResult.totalDamage = (int)areaAttackDamage;
            enemy.ApplyDamage(attackResult);
            
            Debug.Log($"<color=yellow>[AC100] AOE 영역 공격으로 {enemy.pawnName}에게 {attackResult.totalDamage} 데미지 적용</color>");
        }

        private void ApplyAdditionalBuffEffect(Pawn target)
        {
            switch (additionalBuffType)
            {
                case AdditionalBuffType.SpeedBoost:
                    ApplySpeedBoost(target);
                    break;
                case AdditionalBuffType.MoveSpeedBoost:
                    ApplyMoveSpeedBoost(target);
                    break;
                case AdditionalBuffType.AttackSpeedBoost:
                    ApplyAttackSpeedBoost(target);
                    break;
                case AdditionalBuffType.AttackPowerBoost:
                    ApplyAttackPowerBoost(target);
                    break;
                case AdditionalBuffType.DefenseBoost:
                    ApplyDefenseBoost(target);
                    break;
                case AdditionalBuffType.CriticalChanceBoost:
                    ApplyCriticalChanceBoost(target);
                    break;
                case AdditionalBuffType.CriticalDamageBoost:
                    ApplyCriticalDamageBoost(target);
                    break;
                default:
                    break;
            }
        }

        private void ApplySpeedBoost(Pawn target)
        {
            var speedBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.MoveSpeed].AddBuff(speedBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 속도 증가 버프 적용</color>");
        }

        private void ApplyMoveSpeedBoost(Pawn target)
        {
            var moveSpeedBoostModifier = new StatModifier(
                (int)(additionalBuffMultiplier * 100),
                BuffOperationType.Multiplicative,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.MoveSpeed].AddBuff(moveSpeedBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 이동속도 증가 버프 적용</color>");
        }

        private void ApplyAttackSpeedBoost(Pawn target)
        {
            var attackSpeedBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.AttackSpeed].AddBuff(attackSpeedBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 공격속도 증가 버프 적용</color>");
        }

        private void ApplyAttackPowerBoost(Pawn target)
        {
            var attackPowerBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.AttackPower].AddBuff(attackPowerBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 공격력 증가 버프 적용</color>");
        }

        private void ApplyDefenseBoost(Pawn target)
        {
            var defenseBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.Defense].AddBuff(defenseBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 방어력 증가 버프 적용</color>");
        }

        private void ApplyCriticalChanceBoost(Pawn target)
        {
            var criticalChanceBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.CriticalRate].AddBuff(criticalChanceBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 치명타 확률 증가 버프 적용</color>");
        }

        private void ApplyCriticalDamageBoost(Pawn target)
        {
            var criticalDamageBoostModifier = new StatModifier(
                additionalBuffValue,
                BuffOperationType.Additive,
                false,
                additionalBuffDuration
            );
            
            target.statSheet[StatType.CriticalDamage].AddBuff(criticalDamageBoostModifier);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 치명타 데미지 증가 버프 적용</color>");
        }

        private void CreateAreaAttackVFX()
        {
            if (areaAttackVFXPrefab != null)
            {
                GameObject areaAttackVFX = Instantiate(areaAttackVFXPrefab);
                // target 위치에서 VFX 생성 (SingleHit 모드)
                Vector3 vfxPosition = attack.target != null ? attack.target.transform.position : attack.attacker.transform.position;
                areaAttackVFX.transform.position = vfxPosition;
                
                // AOE VFX 지속시간 후 제거
                Destroy(areaAttackVFX, areaAttackVFXDuration);
                
                Debug.Log("<color=blue>[AC100] AOE VFX 생성</color>");
            }
        }

                // DOT 클래스의 Update 함수는 dotInterval 마다 호출되며, dotDuration 만큼 지속됩니다.
        protected override void Update()
        {
            base.Update();
            
            // AOE 영역 공격이 활성화된 경우
            if (createAreaAttack)
            {
                // AOE 영역 공격 처리
                ProcessAreaAttack();
                
                // SingleHit 모드가 아닌 경우에만 지속시간 체크
                if (aoeMode != AOEMode.SingleHit)
                {
                    // 자기장 지속시간 체크
                    if (dotDuration <= 0f)
                    {
                        AttackFactory.Instance.Deactivate(attack);
                        return;
                    }
                    
                    dotDuration -= Time.deltaTime;
                }
            }
            else
            {
                // 일반 도트 처리
                if (target == null || target.isDead || dotDuration <= 0f)
                {
                    AttackFactory.Instance.Deactivate(attack);
                    return;
                }
                
                if (currentDotDuration < dotDuration && currentDotDuration >= dotInterval)
                {
                    DOTHandlerByCollisionType(dotCollisionType);
                    currentDotDuration = 0f;
                    dotDuration -= dotInterval;
                }
                else
                {
                    currentDotDuration += Time.deltaTime;
                }
            }
        }
    }
}