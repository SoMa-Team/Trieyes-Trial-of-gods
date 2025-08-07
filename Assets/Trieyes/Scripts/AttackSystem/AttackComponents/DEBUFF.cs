using AttackSystem;
using UnityEngine;
using System;
using CharacterSystem;
using Stats;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    public enum DEBUFFType
    {
        None,
        DecreaseSpeed,
        DecreaseAttackSpeed,
        DecreaseAttackPower,
        DecreaseDefense,
        DecreaseCriticalChance,
        DecreaseCriticalDamage,
        DecreaseMoveSpeed,

        Slow,
        Frozen,
        Stun,
        Burn,
        Poison,
        Bleed,
        Shock,
        Freeze
    }

    public class DebuffInfo
    {
        public DEBUFFType debuffType;

        public Attack attack;
        public Pawn target;
        public int debuffValue = 10;

        public float debuffInterval = 1f;
        public float debuffMultiplier = 1f;
        public float debuffDuration = 10f;

        public GameObject debuffVFXPrefab;
    }

    /// <summary>
    /// 디버프 효과 적용
    /// </summary>
    public class DEBUFF : AttackComponent
    {   
        // 디버프 타입 ENUM
        public DEBUFFType debuffType;
        public int debuffValue = 10;
        public float debuffMultiplier = 1f;

        public float currentDebuffDuration = 0f;
        public float debuffDuration = 10f;

        public float debuffInterval = 1f;

        public Attack attack;
        public Pawn target;

        public GameObject spawnedVFX;

        private const int AC101_SINGLE_DOT = 1;

        public List<AttackData> attackDatas = new List<AttackData>();

        public void Activate(DebuffInfo debuffInfo)
        {
            debuffType = debuffInfo.debuffType;
            debuffValue = debuffInfo.debuffValue;
            debuffMultiplier = debuffInfo.debuffMultiplier;
            debuffDuration = debuffInfo.debuffDuration;
            debuffInterval = debuffInfo.debuffInterval;
            target = debuffInfo.target;
            attack = debuffInfo.attack;
            spawnedVFX = debuffInfo.debuffVFXPrefab;

            ApplyDebuffEffect();
        }

        private void ApplyDebuffEffect()
        {
            switch (debuffType)
            {
                case DEBUFFType.DecreaseSpeed:
                    ApplyStatDebuff(target, StatType.MoveSpeed, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseAttackSpeed:
                    ApplyStatDebuff(target, StatType.AttackSpeed, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseAttackPower:
                    ApplyStatDebuff(target, StatType.AttackPower, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseDefense:
                    ApplyStatDebuff(target, StatType.Defense, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseCriticalChance:
                    ApplyStatDebuff(target, StatType.CriticalRate, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseCriticalDamage:
                    ApplyStatDebuff(target, StatType.CriticalDamage, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.DecreaseMoveSpeed:
                    ApplyStatDebuff(target, StatType.MoveSpeed, debuffValue, debuffMultiplier, debuffDuration, debuffInterval);
                    break;
                case DEBUFFType.Slow:
                    ApplySlowEffect(target);
                    break;
                case DEBUFFType.Frozen:
                    ApplyFrozenEffect(target);
                    break;
                case DEBUFFType.Stun:
                    ApplyStunEffect(target);
                    break;
                case DEBUFFType.Burn:
                    ApplyBurnEffect(target);
                    break;
                case DEBUFFType.Poison:
                    ApplyPoisonEffect(target);
                    break;
                case DEBUFFType.Bleed:
                    ApplyBleedEffect(target);
                    break;
                case DEBUFFType.Shock:
                    ApplyShockEffect(target);
                    break;
                case DEBUFFType.Freeze:
                    ApplyFrozenEffect(target);
                    break;
                default:
                    break;
            }

            if (spawnedVFX != null && target as Enemy != null)
            {
                var enemy = target as Enemy;
                if (enemy.IsVFXCached(spawnedVFX.name))
                {
                    var vfx = enemy.GetVFX(spawnedVFX.name);
                    vfx.SetActive(true);
                }
                else
                {
                    CreateDOTVFXForTarget(target as Enemy);
                }
            }
        }

        private void CreateDOTVFXForTarget(Enemy target)
        {
            if (target == null || spawnedVFX == null) return;

            // 대상이 유효한지 추가 체크
            if (!target.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[AC101] 대상 {target.pawnName}이 비활성화되어 VFX 생성 취소");
                return;
            }

            // VFX 생성
            spawnedVFX = CreateAndSetupVFX(spawnedVFX, Vector2.zero, Vector2.zero);
            target.AddVFX(spawnedVFX.name, spawnedVFX);
            
            if (spawnedVFX != null)
            {
                // VFX를 대상의 자식으로 설정하여 자동으로 따라가도록 함
                spawnedVFX.transform.SetParent(target.transform, false);
                spawnedVFX.transform.localPosition = Vector3.zero; // 대상 중심에 위치
                spawnedVFX.transform.localRotation = Quaternion.identity;

                var main = spawnedVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main;
                main.duration = debuffDuration;
                main.startLifetime = debuffDuration; // 예: 기존 1초였다면
                main.simulationSpeed = 1f; // 느려지면 안되므로
                
                // VFX 재생
                PlayVFX(spawnedVFX);
                
                Debug.Log($"<color=green>[AC101] {target.pawnName}에게 DOT VFX 생성 및 부착</color>");
            }
        }

        /// <summary>
        /// 통합 스탯 디버프 적용 함수
        /// </summary>
        /// <param name="target">디버프 대상</param>
        /// <param name="statType">적용할 스탯 타입</param>
        /// <param name="debuffValue">가산 디버프 값 (0이면 무시)</param>
        /// <param name="debuffMultiplier">승산 디버프 배율 (1f면 무시)</param>
        /// <param name="duration">디버프 지속시간</param>
        /// <param name="interval">디버프 간격 (주기적 디버프용)</param>
        private void ApplyStatDebuff(Pawn target, StatType statType, int debuffValue, float debuffMultiplier, float duration, float interval)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                return;
            }

            // 가산 디버프 적용 (음수 값)
            if (debuffValue != 0)
            {
                var additiveDebuff = new StatModifier(
                    -debuffValue, // 음수로 적용
                    BuffOperationType.Additive,
                    false,
                    duration
                );
                target.statSheet[statType].AddBuff(additiveDebuff);
            }

            // 승산 디버프 적용 (1f 미만의 배율)
            if (debuffMultiplier != 1f)
            {
                var multiplicativeDebuff = new StatModifier(
                    -(int)(debuffMultiplier), // 승산 배율 그대로 사용
                    BuffOperationType.Multiplicative,
                    false,
                    duration
                );
                target.statSheet[statType].AddBuff(multiplicativeDebuff);
            }
        }

        private void ApplySlowEffect(Pawn target)
        {
            // 이동속도 감소 효과 적용
            ApplyStatDebuff(target, StatType.MoveSpeed, 0, debuffMultiplier, debuffDuration, debuffInterval);
        }

        private void ApplyFrozenEffect(Pawn target)
        {
            // 얼려지는 효과 = N 초동안 이동속도가 0이면 됨
            ApplyStatDebuff(target, StatType.MoveSpeed, 0, 0f, debuffDuration, debuffInterval);
        }

        private void ApplyStunEffect(Pawn target)
        {
            // 스턴 효과 = N 초동안 이동속도가 0이면 됨
            ApplyStatDebuff(target, StatType.MoveSpeed, 0, 0f, debuffDuration, debuffInterval);
        }

        private void ApplyBurnEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.Create(attackDatas[AC101_SINGLE_DOT], attack.attacker, null, Vector2.zero);

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = debuffValue; // 디버프 값 사용
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval;
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;
            dotComponent.dotTarget = target as Enemy;
        }

        private void ApplyPoisonEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.Create(attackDatas[AC101_SINGLE_DOT], attack.attacker, null, Vector2.zero);

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = debuffValue; // 디버프 값 사용
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval; 
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;
            dotComponent.dotTarget = target as Enemy;
        }

        private void ApplyBleedEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.Create(attackDatas[AC101_SINGLE_DOT], attack.attacker, null, Vector2.zero);

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = debuffValue; // 디버프 값 사용
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval;
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;
            dotComponent.dotTarget = target as Enemy;

        }

        private void ApplyShockEffect(Pawn target)
        {
            // 이동속도 0 만들고 + DOT 데미지 주면 됨
            ApplyFrozenEffect(target);
            ApplyBleedEffect(target);
        }
    }
}