using AttackSystem;
using UnityEngine;
using System;
using System.Collections;
using CharacterSystem;
using Stats;

namespace AttackComponents
{
    public enum BUFFType
    {
        IncreaseSpeed,
        IncreaseAttackSpeed,
        IncreaseAttackPower,
        IncreaseDefense,
        IncreaseCriticalChance,
        IncreaseCriticalDamage,
        IncreaseMoveSpeed,
        IncreaseAttackRangeAdd,
        IncreaseAttackRangeMulti,
    }

    /// <summary>
    /// 디버프 효과 적용
    /// </summary>
    public class AC1001_BUFF  : AttackComponent
    {   
        // 버프 타입 ENUM
        public BUFFType buffType;
        public int buffValue = 10;
        public float buffMultiplier = 1f;

        public float currentBuffDuration = 0f;
        public float buffDuration = 10f;

        public Pawn target;
        private StatModifier appliedBuff;

        public override void Activate(Attack attack, Vector2 direction)
        {
            target = attack.attacker;
            BUFFHandlerByIndividual();

            // 버프를 주고 바로 소멸
            AttackFactory.Instance.Deactivate(attack);
        }

        private void BUFFHandlerByIndividual()
        {
            switch (buffType)
            {
                case BUFFType.IncreaseSpeed:
                    Debug.Log("IncreaseSpeed Before: " + target.statSheet[StatType.MoveSpeed].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.MoveSpeed].AddBuff(appliedBuff);
                    Debug.Log("IncreaseSpeed After: " + target.statSheet[StatType.MoveSpeed].Value);
                    break;
                case BUFFType.IncreaseAttackSpeed:
                    Debug.Log("IncreaseAttackSpeed Before: " + target.statSheet[StatType.AttackSpeed].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.AttackSpeed].AddBuff(appliedBuff);
                    Debug.Log("IncreaseAttackSpeed After: " + target.statSheet[StatType.AttackSpeed].Value);
                    break;
                case BUFFType.IncreaseAttackPower:
                    Debug.Log("IncreaseAttackPower Before: " + target.statSheet[StatType.AttackPower].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.AttackPower].AddBuff(appliedBuff);
                    Debug.Log("IncreaseAttackPower After: " + target.statSheet[StatType.AttackPower].Value);
                    break;
                case BUFFType.IncreaseDefense:
                    Debug.Log("IncreaseDefense Before: " + target.statSheet[StatType.Defense].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.Defense].AddBuff(appliedBuff);
                    Debug.Log("IncreaseDefense After: " + target.statSheet[StatType.Defense].Value);
                    break;
                case BUFFType.IncreaseCriticalChance:
                    Debug.Log("IncreaseCriticalChance Before: " + target.statSheet[StatType.CriticalRate].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.CriticalRate].AddBuff(appliedBuff);
                    Debug.Log("IncreaseCriticalChance After: " + target.statSheet[StatType.CriticalRate].Value);
                    break;
                case BUFFType.IncreaseCriticalDamage:
                    Debug.Log("IncreaseCriticalDamage Before: " + target.statSheet[StatType.CriticalDamage].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.CriticalDamage].AddBuff(appliedBuff);
                    Debug.Log("IncreaseCriticalDamage After: " + target.statSheet[StatType.CriticalDamage].Value);
                    break;
                case BUFFType.IncreaseMoveSpeed:
                    Debug.Log("IncreaseMoveSpeed Before: " + target.statSheet[StatType.MoveSpeed].Value);
                    appliedBuff = new StatModifier((int)(buffMultiplier * 100), BuffOperationType.Multiplicative, false, buffDuration);
                    target.statSheet[StatType.MoveSpeed].AddBuff(appliedBuff);
                    Debug.Log("IncreaseMoveSpeed After: " + target.statSheet[StatType.MoveSpeed].Value);
                    break;
                case BUFFType.IncreaseAttackRangeAdd:
                    Debug.Log("IncreaseAttackRangeAdd Before: " + target.statSheet[StatType.AttackRange].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Additive, false, buffDuration);
                    target.statSheet[StatType.AttackRange].AddBuff(appliedBuff);
                    Debug.Log("IncreaseAttackRangeAdd After: " + target.statSheet[StatType.AttackRange].Value);
                    break;
                case BUFFType.IncreaseAttackRangeMulti:
                    Debug.Log("IncreaseAttackRangeMulti Before: " + target.statSheet[StatType.AttackRange].Value);
                    appliedBuff = new StatModifier(buffValue, BuffOperationType.Multiplicative, false, buffDuration);
                    target.statSheet[StatType.AttackRange].AddBuff(appliedBuff);
                    Debug.Log("IncreaseAttackRangeMulti After: " + target.statSheet[StatType.AttackRange].Value);
                    break;
                default:
                    break;
            }
        }
    }
}