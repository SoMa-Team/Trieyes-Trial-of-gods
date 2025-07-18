using AttackSystem;
using UnityEngine;
using System;
using CharacterSystem;
using Stats;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    public enum AOETargetType
    {
        SingleTarget,      // 단일 대상에게만 데미지
        AreaAroundTarget,  // 대상 주변에 AOE
        AreaAtPosition     // 좌표 기반 AOE
    }

    public enum AOEShapeType
    {
        None,    // 형태 없음, 단일 타격
        Rect,    // 직사각형
        Circle   // 원형
    }

    public enum AOEMode
    {
        SingleHit, // 1회 발동
        MultiHit   // N회 발동
    }

    public enum AdditionalBuffType
    {
        None,
        SpeedBoost,
        MoveSpeedBoost,
        AttackSpeedBoost,
        AttackPowerBoost,
        DefenseBoost,
        CriticalChanceBoost,
        CriticalDamageBoost
    }

    /// <summary>
    /// AOE 공격 컴포넌트
    /// 다양한 타겟팅, 형태, 발동 방식을 지원하는 AOE 공격을 구현합니다.
    /// </summary>
    public class AC100_AOE : AttackComponent
    {   
        // AOE 타겟팅 설정
        [Header("AOE 타겟팅 설정")]
        public AOETargetType aoeTargetType = AOETargetType.SingleTarget;

        // AOE 형태 설정
        [Header("AOE 형태 설정")]
        public AOEShapeType aoeShapeType = AOEShapeType.None;
        public float aoeWidth = 1f;    // Rect 형태일 때 가로
        public float aoeHeight = 1f;   // Rect 형태일 때 세로
        public float aoeRadius = 1f;   // Circle 형태일 때 반지름

        // AOE 발동 설정
        [Header("AOE 발동 설정")]
        public AOEMode aoeMode = AOEMode.SingleHit;
        public int aoeDamage = 20;
        public float aoeDuration = 5f;     // MultiHit 모드에서 사용
        public float aoeInterval = 1f;     // MultiHit 모드에서 사용

        // AOE 상태 관리
        private float aoeTimer = 0f;
        private float aoeDurationTimer = 0f;
        private List<Enemy> aoeTargets = new List<Enemy>(10);
        private bool aoeActivated = false;

        // 추가 버프 설정
        [Header("추가 버프 설정")]
        public AdditionalBuffType additionalBuffType = AdditionalBuffType.None;
        public float additionalBuffDuration = 5f;
        public float additionalBuffMultiplier = 1.5f;
        public int additionalBuffValue = 10;

        // AOE VFX 설정
        [Header("AOE VFX 설정")]
        public GameObject aoeVFXPrefab;
        public float aoeVFXDuration = 0.3f;

        // FSM 상태 관리
        private AOEAttackState attackState = AOEAttackState.None;

        // AOE 공격 상태 열거형
        private enum AOEAttackState
        {
            None,
            Preparing,
            Active,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = AOEAttackState.Preparing;
            aoeTimer = 0f;
            aoeDurationTimer = 0f;
            aoeTargets.Clear();
            aoeActivated = false;
            
            // AOE 공격 시작
            StartAC100Attack();
        }

        private void StartAC100Attack()
        {
            attackState = AOEAttackState.Preparing;
            aoeTimer = 0f;
            aoeDurationTimer = 0f;
            
            Debug.Log("<color=orange>[AC100] AOE 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // AOE 공격 상태 처리
            ProcessAC100AttackState();
        }

        private void ProcessAC100AttackState()
        {
            switch (attackState)
            {
                case AOEAttackState.None:
                    break;

                case AOEAttackState.Preparing:
                    aoeTimer += Time.deltaTime;
                    
                    if (aoeTimer >= 0.1f) // 준비 시간
                    {
                        attackState = AOEAttackState.Active;
                        aoeTimer = 0f;
                        ActivateAC100Attack();
                    }
                    break;

                case AOEAttackState.Active:
                    aoeTimer += Time.deltaTime;
                    aoeDurationTimer += Time.deltaTime;
                    
                    // AOE 공격 처리
                    ProcessAC100Attack();
                    
                    // 종료 조건 체크
                    if (ShouldFinishAC100Attack())
                    {
                        attackState = AOEAttackState.Finishing;
                        aoeTimer = 0f;
                        FinishAC100Attack();
                    }
                    break;

                case AOEAttackState.Finishing:
                    aoeTimer += Time.deltaTime;
                    
                    if (aoeTimer >= 0.1f) // 종료 시간
                    {
                        attackState = AOEAttackState.Finished;
                    }
                    break;

                case AOEAttackState.Finished:
                    attackState = AOEAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessAC100Attack()
        {
            switch (aoeMode)
            {
                case AOEMode.SingleHit:
                    // 1회 발동
                    if (!aoeActivated)
                    {
                        ExecuteAC100Attack();
                        aoeActivated = true;
                    }
                    break;

                case AOEMode.MultiHit:
                    // N회 발동
                    if (aoeTimer >= aoeInterval)
                    {
                        ExecuteAC100Attack();
                        aoeTimer = 0f;
                    }
                    break;
            }
        }

        private void ExecuteAC100Attack()
        {
            switch (aoeTargetType)
            {
                case AOETargetType.SingleTarget:
                    ExecuteSingleTargetAttack();
                    break;
                case AOETargetType.AreaAroundTarget:
                    ExecuteAreaAroundTargetAttack();
                    break;
                case AOETargetType.AreaAtPosition:
                    ExecuteAreaAtPositionAttack();
                    break;
            }
        }

        private void ExecuteSingleTargetAttack()
        {
            if (attack.target == null) return;

            // 단일 대상에게 데미지 적용
            var attackResult = AttackResult.Create(attack, attack.target);
            attackResult.totalDamage = aoeDamage;
            attack.target.ApplyDamage(attackResult);

            // 버프 적용
            ApplyAdditionalBuffEffect(attack.target);

            Debug.Log($"<color=yellow>[AC100] 단일 대상 {attack.target.pawnName}에게 {aoeDamage} 데미지 적용</color>");
        }

        private void ExecuteAreaAroundTargetAttack()
        {
            Vector2 aoePosition = attack.target != null ? 
                (Vector2)attack.target.transform.position : 
                (Vector2)attack.attacker.transform.position;

            ExecuteAreaAttack(aoePosition);
        }

        private void ExecuteAreaAtPositionAttack()
        {
            Vector2 aoePosition = (Vector2)attack.attacker.transform.position;
            ExecuteAreaAttack(aoePosition);
        }

        private void ExecuteAreaAttack(Vector2 aoePosition)
        {
            // AOE 범위 내 적 탐지
            DetectEnemiesInAOE(aoePosition);
            
            // 탐지된 적들에게 데미지 적용
            for (int i = 0; i < aoeTargets.Count; i++)
            {
                Pawn enemy = aoeTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    var attackResult = AttackResult.Create(attack, enemy);
                    attackResult.totalDamage = aoeDamage;
                    enemy.ApplyDamage(attackResult);
                }
            }
            
            // AOE VFX 생성
            CreateAC100VFX(aoePosition);
            
            // 공격자에게 버프 적용 (한 번만 적용)
            if (additionalBuffType != AdditionalBuffType.None && !aoeActivated)
            {
                ApplyAdditionalBuffEffect(attack.attacker);
            }

            Debug.Log($"<color=yellow>[AC100] AOE 공격으로 {aoeTargets.Count}명에게 {aoeDamage} 데미지 적용</color>");
        }

        private void DetectEnemiesInAOE(Vector2 aoePosition)
        {
            aoeTargets.Clear();
            
            switch (aoeShapeType)
            {
                case AOEShapeType.None:
                    // 형태가 없으면 단일 대상만 처리
                    if (attack.target != null)
                    {
                        aoeTargets.Add(attack.target as Enemy);
                    }
                    break;

                case AOEShapeType.Rect:
                    // 직사각형 범위 탐지
                    Vector2 rectStart = aoePosition - new Vector2(aoeWidth * 0.5f, aoeHeight * 0.5f);
                    Vector2 rectEnd = aoePosition + new Vector2(aoeWidth * 0.5f, aoeHeight * 0.5f);
                    aoeTargets = BattleStage.now.GetEnemiesInRectRange(rectStart, rectEnd);
                    break;

                case AOEShapeType.Circle:
                    // 원형 범위 탐지
                    aoeTargets = BattleStage.now.GetEnemiesInCircleRange(aoePosition, aoeRadius);
                    break;
            }
        }

        private bool ShouldFinishAC100Attack()
        {
            switch (aoeMode)
            {
                case AOEMode.SingleHit:
                    // 1회 발동이면 바로 종료
                    return aoeActivated;
                    
                case AOEMode.MultiHit:
                    // N회 발동이면 지속시간 체크
                    return aoeDurationTimer >= aoeDuration;
                    
                default:
                    return true;
            }
        }

        private void ActivateAC100Attack()
        {
            Debug.Log("<color=green>[AC100] AOE 공격 활성화!</color>");
        }

        private void FinishAC100Attack()
        {
            Debug.Log("<color=orange>[AC100] AOE 공격 종료!</color>");
        }

        private void CreateAC100VFX(Vector2 position)
        {
            if (aoeVFXPrefab != null)
            {
                GameObject aoeVFX = Instantiate(aoeVFXPrefab);
                aoeVFX.transform.position = position;
                
                // AOE VFX 지속시간 후 제거
                Destroy(aoeVFX, aoeVFXDuration);
                
                Debug.Log("<color=blue>[AC100] AOE VFX 생성</color>");
            }
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
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.Haste,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 속도 증가 버프 적용</color>");
        }

        private void ApplyMoveSpeedBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseMoveSpeed,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 이동속도 증가 버프 적용</color>");
        }

        private void ApplyAttackSpeedBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackSpeed,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 공격속도 증가 버프 적용</color>");
        }

        private void ApplyAttackPowerBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackPower,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 공격력 증가 버프 적용</color>");
        }

        private void ApplyDefenseBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseDefense,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 방어력 증가 버프 적용</color>");
        }

        private void ApplyCriticalChanceBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseCriticalChance,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 치명타 확률 증가 버프 적용</color>");
        }

        private void ApplyCriticalDamageBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseCriticalDamage,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC100] {target.pawnName}에게 치명타 데미지 증가 버프 적용</color>");
        }
    }
}