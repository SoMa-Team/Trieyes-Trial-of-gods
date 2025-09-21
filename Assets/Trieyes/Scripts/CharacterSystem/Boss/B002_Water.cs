using System;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using PrimeTween;
using Stats;
using UnityEngine;
using EventType = Utils.EventType;
using Random = UnityEngine.Random;

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
            
            AttackFactory.Instance.Create(attackData, this, null, LastMoveDirection, null, true);
            availableAttackTime = Time.time + attackData.cooldown;
            return true;
        }

        protected override void OnSelfDeath(AttackResult result)
        {
            var sequence = Sequence.Create();
            Controller.SetLockMovement(true);
            
            for (int i = 0; i < dropGold; i++)
            {
                var realDropGold = 1;
                if (Random.Range(0, 100f) < result.attacker.statSheet.Get(StatType.GoldDropRate))
                    realDropGold = 2;
                Gold gold = DropFactory.Instance.CreateGold(transform.position, realDropGold, false);
                sequence.Insert(0.01f * i, DropFactory.Instance.AnimationDrop(gold));
                BattleStage.now.AttachGold(gold);
            }
            
            sequence.OnComplete(() =>
            {
                EnemyFactory.Instance.Deactivate(this);
                if (BattleStage.now is BattleBossStage)
                {
                    Tween.Delay(10f).OnComplete(() => BattleStage.now.OnBattleClear());
                }
            });
        }
    }
}
