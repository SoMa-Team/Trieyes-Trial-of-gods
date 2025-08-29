using System;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace CharacterSystem
{
    public enum B002AttackType
    {
        Default,
        StoneSummon,
        Rush,
        StoneExplode,
        FireDischarge,
        SpawnSlowField,
        CircularSector
    }
    
    public class B002_Water : Enemy
    {
        public bool isAnimated { get; private set; }

        public override void Activate()
        {
            base.Activate();
            isAnimated = false;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            isAnimated = false;
        }

        [Header("====== Boss 공격 종류 ======")]
        [SerializeField] private AttackData attackDefault;
        [SerializeField] private AttackData attackStoneSummon;
        [SerializeField] private AttackData attackRush;
        [SerializeField] private AttackData attackStoneExplode;
        [SerializeField] private AttackData attackFireDischarge;
        [SerializeField] private AttackData attackSpawnSlowField;
        [SerializeField] private AttackData attackMoveCenter;
        [SerializeField] private AttackData attackCircularSector;
        
        public bool ExecuteBossAttack(B002AttackType attackType)
        {
            // if (isAnimated)
            //     return false;
            // isAnimated = true;
            
            AttackFactory.Instance.Create(attackType switch
            {
                B002AttackType.Default => attackDefault,
                B002AttackType.StoneSummon => attackStoneSummon,
                B002AttackType.Rush => attackRush,
                B002AttackType.StoneExplode => attackStoneExplode,
                B002AttackType.FireDischarge => attackFireDischarge,
                B002AttackType.SpawnSlowField => attackSpawnSlowField,
                B002AttackType.CircularSector => attackCircularSector,
                _ => throw new Exception($"B002.ExecuteBossAttack: Attack {attackType} is not exist."),
            }, this, null, LastMoveDirection);

            return true;
        }
    }
}
