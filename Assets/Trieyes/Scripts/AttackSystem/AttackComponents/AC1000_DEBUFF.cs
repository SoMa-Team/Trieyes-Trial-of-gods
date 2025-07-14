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

        // DOT 클래스의 Update 함수는 dotInterval 마다 호출되며, dotDuration 만큼 지속됩니다.
        protected override void Update()
        {
            if (target == null || target.isDead || debuffDuration <= 0f || currentDebuffDuration >= debuffDuration)
            {
                // 디버프 효과는 StatModifier 시스템에서 자동으로 만료되므로 별도 처리 불필요
                AttackFactory.Instance.Deactivate(attack);
                return;
            }

            currentDebuffDuration += Time.deltaTime;
        }
    }
}