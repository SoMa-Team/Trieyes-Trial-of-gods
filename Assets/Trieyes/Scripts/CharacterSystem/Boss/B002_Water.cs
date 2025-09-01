using System;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace CharacterSystem
{
    public enum B002AttackType
    {
        Default,
        CircularSector,
        StoneSummon
    }
    
    public class B002_Water : Enemy
    {
        private float availableAttackTime;
        public bool isDoAttack => Time.time < availableAttackTime;

        public override void Activate()
        {
            base.Activate();
            availableAttackTime = Time.time;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        [Header("====== Boss 공격 종류 ======")]
        [SerializeField] private AttackData attackDefault;
        [SerializeField] private AttackData attackCircularSector;
        [SerializeField] private AttackData attackStoneSummon;
        
        public bool ExecuteBossAttack(B002AttackType attackType)
        {
            if (Time.time < availableAttackTime)
                return false;
            
            var attackData = attackType switch
            {
                B002AttackType.Default => attackDefault,
                B002AttackType.CircularSector => attackCircularSector,
                B002AttackType.StoneSummon => attackStoneSummon,
                _ => throw new Exception($"B002.ExecuteBossAttack: Attack {attackType} is not exist."),
            };
            
            AttackFactory.Instance.Create(attackData, this, null, LastMoveDirection);
            availableAttackTime = Time.time + attackData.cooldown;
            return true;
        }
    }
}
