using AttackSystem;
using UnityEngine;
using CharacterSystem;
using Stats;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    public enum BUFFType
    {
        None,
        IncreaseSpeed,
        IncreaseAttackSpeed,
        IncreaseAttackPower,
        IncreaseDefense,
        IncreaseCriticalChance,
        IncreaseCriticalDamage,
        IncreaseMoveSpeed,
        IncreaseAttackRangeAdd,
        IncreaseAttackRangeMulti,
        
        // 상태 버프들
        Haste,
        Berserk,
        Shield,
        Regeneration,
        Invincibility,
        Stealth,
        Rage,
        Protection
    }

    public class BuffInfo
    {
        public BUFFType buffType;

        public Attack attack;
        public Pawn target; // 이 친구가 버프의 대상이
        public int buffValue = 10;
        public float buffMultiplier = 1f;
        public float buffDuration = 10f;

        public float buffInterval = 1f;
    }

    /// <summary>
    /// 버프 효과 적용
    /// </summary>
    public class BUFF
    {   
        // 버프 타입 ENUM
        public BUFFType buffType;
        public int buffValue = 10;
        public float buffMultiplier = 1f;

        public float currentBuffDuration = 0f;
        public float buffDuration = 10f;

        public float buffInterval = 1f;

        public Attack attack;
        public Pawn target;

        public void Activate(BuffInfo buffInfo)
        {
            buffType = buffInfo.buffType;
            buffValue = buffInfo.buffValue;
            buffMultiplier = buffInfo.buffMultiplier;
            buffDuration = buffInfo.buffDuration;
            buffInterval = buffInfo.buffInterval;
            target = buffInfo.target;
            attack = buffInfo.attack;

            ApplyBuffEffect();
        }

        private void ApplyBuffEffect()
        {
            switch (buffType)
            {
                case BUFFType.IncreaseSpeed:
                    ApplyStatBuff(target, StatType.MoveSpeed, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseAttackSpeed:
                    ApplyStatBuff(target, StatType.AttackSpeed, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseAttackPower:
                    ApplyStatBuff(target, StatType.AttackPower, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseDefense:
                    ApplyStatBuff(target, StatType.Defense, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseCriticalChance:
                    ApplyStatBuff(target, StatType.CriticalRate, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseCriticalDamage:
                    ApplyStatBuff(target, StatType.CriticalDamage, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseMoveSpeed:
                    ApplyStatBuff(target, StatType.MoveSpeed, buffValue, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseAttackRangeAdd:
                    ApplyStatBuff(target, StatType.AttackRange, buffValue, 1f, buffDuration, buffInterval);
                    break;
                case BUFFType.IncreaseAttackRangeMulti:
                    ApplyStatBuff(target, StatType.AttackRange, 0, buffMultiplier, buffDuration, buffInterval);
                    break;
                case BUFFType.Haste:
                    ApplyHasteEffect(target);
                    break;
                case BUFFType.Berserk:
                    ApplyBerserkEffect(target);
                    break;
                case BUFFType.Shield:
                    ApplyShieldEffect(target);
                    break;
                case BUFFType.Regeneration:
                    ApplyStatBuff(target, StatType.Health, buffValue, 1f, buffDuration, buffInterval);
                    break;
                case BUFFType.Invincibility:
                    ApplyInvincibilityEffect(target);
                    break;
                case BUFFType.Stealth:
                    ApplyStealthEffect(target);
                    break;
                case BUFFType.Rage:
                    ApplyRageEffect(target);
                    break;
                case BUFFType.Protection:
                    ApplyProtectionEffect(target);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 통합 스탯 버프 적용 함수
        /// </summary>
        /// <param name="target">버프 대상</param>
        /// <param name="statType">적용할 스탯 타입</param>
        /// <param name="buffValue">가산 버프 값 (0이면 무시)</param>
        /// <param name="buffMultiplier">승산 버프 배율 (1f면 무시)</param>
        /// <param name="duration">버프 지속시간</param>
        /// <param name="interval">버프 간격 (주기적 버프용)</param>
        private void ApplyStatBuff(Pawn target, StatType statType, int buffValue, float buffMultiplier, float duration, float interval)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                return;
            }

            // 가산 버프 적용
            if (buffValue != 0)
            {
                var additiveBuff = new StatModifier(
                    buffValue,
                    BuffOperationType.Additive,
                    false,
                    duration
                );
                target.statSheet[statType].AddBuff(additiveBuff);
            }

            // 승산 버프 적용
            if (buffMultiplier != 1f)
            {
                var multiplicativeBuff = new StatModifier(
                    (int)(buffMultiplier), // 승산 배율 그대로 사용
                    BuffOperationType.Multiplicative,
                    false,
                    duration
                );
                target.statSheet[statType].AddBuff(multiplicativeBuff);
            }
        }

        private void ApplyHasteEffect(Pawn target)
        {
            // 이동속도와 공격속도 모두 증가
            ApplyStatBuff(target, StatType.MoveSpeed, 0, buffMultiplier, buffDuration, buffInterval);
            ApplyStatBuff(target, StatType.AttackSpeed, 0, buffMultiplier, buffDuration, buffInterval);
        }

        private void ApplyBerserkEffect(Pawn target)
        {
            // 공격력 증가, 방어력 감소
            ApplyStatBuff(target, StatType.AttackPower, 0, buffMultiplier * 1.5f, buffDuration, buffInterval);
            ApplyStatBuff(target, StatType.Defense, 0, 0.5f, buffDuration, buffInterval); // 50% 감소
        }

        private void ApplyShieldEffect(Pawn target)
        {
            // 방어력 대폭 증가
            ApplyStatBuff(target, StatType.Defense, 0, buffMultiplier * 2f, buffDuration, buffInterval);
        }

        private void ApplyInvincibilityEffect(Pawn target)
        {
            // 무적 효과 = 방어력을 매우 높게 설정
            ApplyStatBuff(target, StatType.Defense, 999999, 1f, buffDuration, buffInterval);
        }

        private void ApplyStealthEffect(Pawn target)
        {
            // 은신 효과 = 크리티컬 확률 대폭 증가
            ApplyStatBuff(target, StatType.CriticalRate, 0, buffMultiplier * 3f, buffDuration, buffInterval);
        }

        private void ApplyRageEffect(Pawn target)
        {
            // 분노 효과 = 공격력과 크리티컬 데미지 증가
            ApplyStatBuff(target, StatType.AttackPower, 0, buffMultiplier * 2f, buffDuration, buffInterval);
            ApplyStatBuff(target, StatType.CriticalDamage, 0, buffMultiplier * 1.5f, buffDuration, buffInterval);
        }

        private void ApplyProtectionEffect(Pawn target)
        {
            // 보호 효과 = 모든 방어 스탯 증가
            ApplyStatBuff(target, StatType.Defense, 0, buffMultiplier * 1.5f, buffDuration, buffInterval);
        }
    }
}