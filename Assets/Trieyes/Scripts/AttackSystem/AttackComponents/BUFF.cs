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
        public Pawn target;
        public int buffValue = 10;

        public float buffInterval = 1f;
        public int globalHeal = 10;
        public float buffMultiplier = 1f;
        public float buffDuration = 10f;
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
        public int globalHeal = 10;

        public Attack attack;
        public Pawn target;

        private const int AC100_SINGLE_AOE = 10;

        public void Activate(BuffInfo buffInfo)
        {
            buffType = buffInfo.buffType;
            buffValue = buffInfo.buffValue;
            buffMultiplier = buffInfo.buffMultiplier;
            buffDuration = buffInfo.buffDuration;
            buffInterval = buffInfo.buffInterval;
            globalHeal = buffInfo.globalHeal;
            target = buffInfo.target;
            attack = buffInfo.attack;

            ApplyBuffEffect(target);
        }

        private void ApplyBuffEffect(Pawn target)
        {
            switch (buffType)
            {
                case BUFFType.IncreaseSpeed:
                    ApplyIncreaseSpeedEffect(target);
                    break;
                case BUFFType.IncreaseAttackSpeed:
                    ApplyIncreaseAttackSpeedEffect(target);
                    break;
                case BUFFType.IncreaseAttackPower:
                    ApplyIncreaseAttackPowerEffect(target);
                    break;
                case BUFFType.IncreaseDefense:
                    ApplyIncreaseDefenseEffect(target);
                    break;
                case BUFFType.IncreaseCriticalChance:
                    ApplyIncreaseCriticalChanceEffect(target);
                    break;
                case BUFFType.IncreaseCriticalDamage:
                    ApplyIncreaseCriticalDamageEffect(target);
                    break;
                case BUFFType.IncreaseMoveSpeed:
                    ApplyIncreaseMoveSpeedEffect(target);
                    break;
                case BUFFType.IncreaseAttackRangeAdd:
                    ApplyIncreaseAttackRangeAddEffect(target);
                    break;
                case BUFFType.IncreaseAttackRangeMulti:
                    ApplyIncreaseAttackRangeMultiEffect(target);
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
                    ApplyRegenerationEffect(target);
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

        private void ApplyIncreaseSpeedEffect(Pawn target)
        {
            // null 체크 추가
            if (target == null || target.statSheet == null)
            {
                //Debug.LogWarning($"<color=yellow>[BUFF] ApplyIncreaseSpeedEffect: target 또는 statSheet가 null입니다.</color>");
                return;
            }

            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.MoveSpeed].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 이동속도 증가 효과 적용</color>");
        }

        private void ApplyIncreaseAttackSpeedEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackSpeed].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 공격속도 증가 효과 적용</color>");
        }

        private void ApplyIncreaseAttackPowerEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackPower].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 공격력 증가 효과 적용</color>");
        }

        private void ApplyIncreaseDefenseEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.Defense].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 방어력 증가 효과 적용</color>");
        }

        private void ApplyIncreaseCriticalChanceEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.CriticalRate].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 크리티컬 확률 증가 효과 적용</color>");
        }

        private void ApplyIncreaseCriticalDamageEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.CriticalDamage].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 크리티컬 데미지 증가 효과 적용</color>");
        }

        private void ApplyIncreaseMoveSpeedEffect(Pawn target)
        {
            //Debug.Log("이전 이동속도: " + target.statSheet[StatType.MoveSpeed].Value);

            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.MoveSpeed].AddBuff(buffModifier);
            
            //Debug.Log("이후 이동속도: " + target.statSheet[StatType.MoveSpeed].Value);
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 이동속도 증가 효과 적용</color>");
        }

        private void ApplyIncreaseAttackRangeAddEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                buffValue,
                BuffOperationType.Additive,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackRange].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 공격범위 증가 효과 적용</color>");
        }

        private void ApplyIncreaseAttackRangeMultiEffect(Pawn target)
        {
            var buffModifier = new StatModifier(
                (int)(buffMultiplier), // 30% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackRange].AddBuff(buffModifier);
            
            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 공격범위 증가 효과 적용</color>");
        }

        private void ApplyHasteEffect(Pawn target)
        {
            // 이동속도와 공격속도 모두 증가
            var moveSpeedBuff = new StatModifier(
                (int)(buffMultiplier),
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.MoveSpeed].AddBuff(moveSpeedBuff);

            var attackSpeedBuff = new StatModifier(
                (int)(buffMultiplier),
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackSpeed].AddBuff(attackSpeedBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 헤이스트 효과 적용</color>");
        }

        private void ApplyBerserkEffect(Pawn target)
        {
            // 공격력 증가, 방어력 감소
            var attackPowerBuff = new StatModifier(
                (int)(buffMultiplier * 150), // 50% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackPower].AddBuff(attackPowerBuff);

            var defenseDebuff = new StatModifier(
                -(int)(buffMultiplier * 50), // 50% 감소
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.Defense].AddBuff(defenseDebuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 버서크 효과 적용</color>");
        }

        private void ApplyShieldEffect(Pawn target)
        {
            // 방어력 대폭 증가
            var shieldBuff = new StatModifier(
                (int)(buffMultiplier * 200), // 100% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.Defense].AddBuff(shieldBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 실드 효과 적용</color>");
        }

        private void ApplyRegenerationEffect(Pawn target)
        {
            // AC100의 단일 AOE 효과를 힐링으로 적용
            var healAttack = AttackFactory.Instance.ClonePrefab(AC100_SINGLE_AOE);
            BattleStage.now.AttachAttack(healAttack);
            healAttack.target = target;

            var healComponent = healAttack.components[0] as AC100_AOE;
            healComponent.aoeDamage = -globalHeal; // 음수로 하면 힐링
            healComponent.aoeDuration = buffDuration;
            healComponent.aoeInterval = buffInterval;
            healComponent.aoeTargetType = AOETargetType.SingleTarget;
            healComponent.aoeShapeType = AOEShapeType.None;
            healComponent.aoeRadius = 0f;
            healComponent.aoeWidth = 0f;
            healComponent.aoeHeight = 0f;
            healComponent.aoeMode = AOEMode.SingleHit;

            healAttack.Activate(attack.attacker, Vector2.zero);
        }

        private void ApplyInvincibilityEffect(Pawn target)
        {
            // 무적 효과 = 방어력을 매우 높게 설정
            var invincibilityBuff = new StatModifier(
                999999, // 거의 무적
                BuffOperationType.Additive,
                false,
                buffDuration
            );
            target.statSheet[StatType.Defense].AddBuff(invincibilityBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 무적 효과 적용</color>");
        }

        private void ApplyStealthEffect(Pawn target)
        {
            // 은신 효과 = 크리티컬 확률 대폭 증가
            var stealthBuff = new StatModifier(
                (int)(buffMultiplier * 300), // 200% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.CriticalRate].AddBuff(stealthBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 은신 효과 적용</color>");
        }

        private void ApplyRageEffect(Pawn target)
        {
            // 분노 효과 = 공격력과 크리티컬 데미지 증가
            var attackPowerBuff = new StatModifier(
                (int)(buffMultiplier * 200), // 100% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.AttackPower].AddBuff(attackPowerBuff);

            var criticalDamageBuff = new StatModifier(
                (int)(buffMultiplier * 150), // 50% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.CriticalDamage].AddBuff(criticalDamageBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 분노 효과 적용</color>");
        }

        private void ApplyProtectionEffect(Pawn target)
        {
            // 보호 효과 = 모든 방어 스탯 증가
            var defenseBuff = new StatModifier(
                (int)(buffMultiplier * 150), // 50% 증가
                BuffOperationType.Multiplicative,
                false,
                buffDuration
            );
            target.statSheet[StatType.Defense].AddBuff(defenseBuff);

            //Debug.Log($"<color=green>[BUFF] {target.pawnName}에게 보호 효과 적용</color>");
        }
    }
}