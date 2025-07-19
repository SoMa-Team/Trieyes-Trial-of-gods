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
        public int globalDamage = 10;
        public float debuffMultiplier = 1f;
        public float debuffDuration = 10f;
    }

    /// <summary>
    /// 디버프 효과 적용
    /// </summary>
    public class DEBUFF
    {   
        // 디버프 타입 ENUM
        public DEBUFFType debuffType;
        public int debuffValue = 10;
        public float debuffMultiplier = 1f;

        public float currentDebuffDuration = 0f;
        public float debuffDuration = 10f;

        public float debuffInterval = 1f;
        public int globalDamage = 10;

        public Attack attack;
        public Pawn target;

        private const int AC101_SINGLE_DOT = 11;

        public void Activate(DebuffInfo debuffInfo)
        {
            debuffType = debuffInfo.debuffType;
            debuffValue = debuffInfo.debuffValue;
            debuffMultiplier = debuffInfo.debuffMultiplier;
            debuffDuration = debuffInfo.debuffDuration;
            debuffInterval = debuffInfo.debuffInterval;
            globalDamage = debuffInfo.globalDamage;
            target = debuffInfo.target;
            attack = debuffInfo.attack;

            ApplyDebuffEffect(target);
        }

        private void ApplyDebuffEffect(Pawn target)
        {
            switch (debuffType)
            {
                case DEBUFFType.DecreaseSpeed:
                    ApplyDecreaseSpeedEffect(target);
                    break;
                case DEBUFFType.DecreaseAttackSpeed:
                    ApplyDecreaseAttackSpeedEffect(target);
                    break;
                case DEBUFFType.DecreaseAttackPower:
                    ApplyDecreaseAttackPowerEffect(target);
                    break;
                case DEBUFFType.DecreaseDefense:
                    ApplyDecreaseDefenseEffect(target);
                    break;
                case DEBUFFType.DecreaseCriticalChance:
                    ApplyDecreaseCriticalChanceEffect(target);
                    break;
                case DEBUFFType.DecreaseCriticalDamage:
                    ApplyDecreaseCriticalDamageEffect(target);
                    break;
                case DEBUFFType.DecreaseMoveSpeed:
                    ApplyDecreaseMoveSpeedEffect(target);
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

                default:
                    break;
            }
        }

        private void ApplyDecreaseSpeedEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseAttackSpeedEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseAttackPowerEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseDefenseEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseCriticalChanceEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseCriticalDamageEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplyDecreaseMoveSpeedEffect(Pawn target)
        {
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
        }

        private void ApplySlowEffect(Pawn target)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                Debug.LogWarning($"<color=yellow>[DEBUFF] ApplySlowEffect: target 또는 statSheet가 null입니다.</color>");
                return;
            }

            // 이동속도 감소 효과 적용
            var debuffModifier = new StatModifier(
                -(int)(debuffMultiplier), // 30% 감소
                BuffOperationType.Multiplicative,
                false,
                debuffDuration
            );
            
            target.statSheet[StatType.MoveSpeed].AddBuff(debuffModifier);
            
            Debug.Log($"<color=blue>[GLOBAL_BLIZZARD] {target.pawnName}에게 슬로우 효과 적용</color>");
        }

        private void ApplyFrozenEffect(Pawn target)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                Debug.LogWarning($"<color=yellow>[DEBUFF] ApplyFrozenEffect: target 또는 statSheet가 null입니다.</color>");
                return;
            }

            // 얼려지는 효과 = N 초동안 이동속도가 0이면 됨
            var debuffModifier = new StatModifier(0, BuffOperationType.Multiplicative, false, debuffDuration);
            target.statSheet[StatType.MoveSpeed].AddBuff(debuffModifier);

            Debug.Log($"<color=blue>[GLOBAL_BLIZZARD] {target.pawnName}에게 얼려지는 효과 적용</color>");
        }

        private void ApplyStunEffect(Pawn target)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                Debug.LogWarning($"<color=yellow>[DEBUFF] ApplyStunEffect: target 또는 statSheet가 null입니다.</color>");
                return;
            }

            // 스턴 효과 = N 초동안 이동속도가 0이면 됨
            var debuffModifier = new StatModifier(0, BuffOperationType.Multiplicative, false, debuffDuration);
            target.statSheet[StatType.MoveSpeed].AddBuff(debuffModifier);

            Debug.Log($"<color=blue>[GLOBAL_BLIZZARD] {target.pawnName}에게 스턴 효과 적용</color>");
        }

        private void ApplyBurnEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.ClonePrefab(AC101_SINGLE_DOT);
            BattleStage.now.AttachAttack(burnAttack);
            burnAttack.target = target;

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = globalDamage;
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval;
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;

            burnAttack.Activate(attack.attacker, Vector2.zero);
        }

        private void ApplyPoisonEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.ClonePrefab(AC101_SINGLE_DOT);
            BattleStage.now.AttachAttack(burnAttack);
            burnAttack.target = target;

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = globalDamage;
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval; 
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;

            burnAttack.Activate(attack.attacker, Vector2.zero);
        }

        private void ApplyBleedEffect(Pawn target)
        {
            // AC101의 단일 DOT 효과 적용하면 됨
            var burnAttack = AttackFactory.Instance.ClonePrefab(AC101_SINGLE_DOT);
            BattleStage.now.AttachAttack(burnAttack);
            burnAttack.target = target;

            var dotComponent = burnAttack.components[0] as AC101_DOT;
            dotComponent.dotDamage = globalDamage;
            dotComponent.dotDuration = debuffDuration;
            dotComponent.dotInterval = debuffInterval;
            dotComponent.dotTargetType = DOTTargetType.SingleTarget;

            burnAttack.Activate(attack.attacker, Vector2.zero);
        }

        private void ApplyShockEffect(Pawn target)
        {
            // 이동속도 0 만들고 + DOT 데미지 주면 됨
            ApplyFrozenEffect(target);
            ApplyBleedEffect(target);
        }
    }
}