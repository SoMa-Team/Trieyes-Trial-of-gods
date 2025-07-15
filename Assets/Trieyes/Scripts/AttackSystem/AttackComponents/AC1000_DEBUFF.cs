using AttackSystem;
using UnityEngine;
using System;
using System.Collections;
using CharacterSystem;
using Stats;

namespace AttackComponents
{
    public enum DEBUFFType
    {
        DecreaseSpeed,
        DecreaseAttackSpeed,
        DecreaseAttackPower,
        DecreaseDefense,
        DecreaseCriticalChance,
        DecreaseCriticalDamage,
        DecreaseMoveSpeed,
    }

    /// <summary>
    /// 디버프 효과 적용
    /// </summary>
    public class AC1000_DEBUFF  : AttackComponent
    {   
        // 디버프 타입 ENUM
        public DEBUFFType debuffType;
        public int debuffValue = 10;
        public float debuffMultiplier = 1f;

        public float currentDebuffDuration = 0f;
        public float debuffDuration = 10f;

        public Pawn target;
        private StatModifier appliedDebuff;

        public override void Activate(Attack attack, Vector2 direction)
        {
            if (attack.target != null)
            {
                target = attack.target;
            }
            DEBUFFHandlerByIndividual();

            // 디버프를 주고 바로 소멸
            AttackFactory.Instance.Deactivate(attack);
        }

        private void DEBUFFHandlerByIndividual()
        {
            switch (debuffType)
            {
                case DEBUFFType.DecreaseSpeed:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.MoveSpeed].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseAttackSpeed:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.AttackSpeed].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseAttackPower:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.AttackPower].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseDefense:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.Defense].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseCriticalChance:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.CriticalRate].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseCriticalDamage:
                    appliedDebuff = new StatModifier(-debuffValue, BuffOperationType.Additive, false, debuffDuration);
                    target.statSheet[StatType.CriticalDamage].AddBuff(appliedDebuff);
                    break;
                case DEBUFFType.DecreaseMoveSpeed:
                    appliedDebuff = new StatModifier(-(int)(debuffMultiplier * 100), BuffOperationType.Multiplicative, false, debuffDuration);
                    target.statSheet[StatType.MoveSpeed].AddBuff(appliedDebuff);
                    break;
                default:
                    break;
            }
        }
    }
}