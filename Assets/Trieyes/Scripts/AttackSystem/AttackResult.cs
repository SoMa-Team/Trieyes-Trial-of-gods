using CharacterSystem;
using JetBrains.Annotations;
using RelicSystem;
using Stats;
using UnityEngine;

namespace AttackSystem
{
    public enum DamageType
    {
        DamagedByProjectile, // 투사체에 부딪혀서 충돌
        DamagedByEnemyCollision, // 몸이 부딪혀서 충돌
    }
    
    public class AttackResult
    {
        public DamageType damageType;
        [CanBeNull] public Attack attack;
        public Pawn attacker;
        public Pawn target;

        public bool isEvaded;
        public bool isCritical;
        public int totalDamage;
        public int attackerHealed;
        public int attackerReflectDamage;
        
        public static AttackResult Create(Attack attack, Pawn target)
        {
            var initAttackResult = new AttackResult
            {
                damageType = DamageType.DamagedByProjectile,
                attack = attack,
                attacker = attack.attacker,
                target = target
            };

            var attackStat = attack.statSheet;
            var targetStat = target.statSheet;

            var attackResult = calcAttackResultByStatSheet(initAttackResult, attackStat, targetStat);
            return attackResult;
        }
    
        public static AttackResult Create(Pawn attacker, Pawn target)
        {
            var initAttackResult = new AttackResult
            {
                damageType = DamageType.DamagedByEnemyCollision,
                attack = null,
                attacker = attacker,
                target = target
            };

            var attackStat = attacker.statSheet;
            var targetStat = target.statSheet;

            var attackResult = calcAttackResultByStatSheet(initAttackResult, attackStat, targetStat);

            return attackResult;
        }
        
        private static AttackResult calcAttackResultByStatSheet(AttackResult attackResult, StatSheet attackStat,
            StatSheet targetStat)
        {
            attackResult.isEvaded = Random.Range(0f, 100f) < targetStat.Get(StatType.Evasion);
            
            //Debug.Log(targetStat.Get(StatType.Evasion));

            if (attackResult.isEvaded)
                return attackResult;
            
            attackResult.isCritical = Random.Range(0f, 100f) < attackStat.Get(StatType.CriticalRate);
            
            var attackDamageIncreasement = attackResult.attack?.getRelicStat(RelicStatType.DamageIncreasement) ?? 0;
            var pureDamage = attackStat.Get(StatType.AttackPower) * (attackResult.attack?.attackData?.damageMultiplier ?? 1) *
                (100 + attackDamageIncreasement) / 100;
            var baseDamage = (int)(pureDamage / (1 + 8.9e-6 * targetStat.Get(StatType.Defense)));
            
            attackResult.totalDamage = attackResult.isCritical ? baseDamage * (100 + (int)attackStat.Get(StatType.CriticalRate)) / 100 : baseDamage;
            attackResult.attackerHealed = (int)(attackResult.totalDamage * attackStat.Get(StatType.LifeSteal) / 100);
            attackResult.attackerReflectDamage = (int)(attackResult.totalDamage * targetStat.Get(StatType.Reflect) / 100);
            return attackResult;
        }
    }
}