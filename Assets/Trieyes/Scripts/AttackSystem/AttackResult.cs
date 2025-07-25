using CharacterSystem;
using RelicSystem;
using Stats;
using UnityEngine;

namespace AttackSystem
{
    public class AttackResult
    {
        public Attack attack;
        public Pawn attacker;
        public Pawn target;

        public bool isEvaded;
        public bool isCritical;
        public int totalDamage;
        public int attackerHealed;
        public int attackerDamage;
        
        public static AttackResult Create(Attack attack, Pawn target)
        {
            var attackResult = new AttackResult();
            
            attackResult.attack = attack;
            attackResult.attacker = attack.attacker;
            attackResult.target = target;

            var attackStat = attack.statSheet;
            var targetStat = target.statSheet;
            
            attackResult.isEvaded = Random.Range(0f, 100f) < targetStat[StatType.Evasion];

            if (attackResult.isEvaded)
                return attackResult;
            
            // TODO : 수식 논의 필요
            attackResult.isCritical = Random.Range(0f, 100f) < attackStat[StatType.CriticalRate];
            
            var attackDamageIncreasement = attack.getRelicStat(RelicStatType.DamageIncreasement);
            var pureDamage = attackStat[StatType.AttackPower] * attack.attackData.damageMultiplier *
                (100 + attackDamageIncreasement) / 100;
            var baseDamage = (int)(pureDamage) * 100 / (100 + targetStat[StatType.Defense]);
            
            attackResult.totalDamage = attackResult.isCritical ? baseDamage * (100 + attackStat[StatType.CriticalDamage]) / 100 : baseDamage;
            attackResult.attackerHealed = attackResult.totalDamage * attackStat[StatType.LifeSteal] / 100;
            attackResult.attackerDamage = attackResult.totalDamage * targetStat[StatType.Reflect] / 100;

            return attackResult;
        }
    
        // TO-DO : 폰과 폰 사이의 공격의 경우에 대한 Result Create 함수 추가
        public static AttackResult Create(Enemy enemy, Character character)
        {
            var attackResult = new AttackResult();

            attackResult.attack = null;
            
            attackResult.attacker = enemy;
            attackResult.target = character;

            var attackStat = enemy.statSheet;
            var targetStat = character.statSheet;
            
            attackResult.isEvaded = Random.Range(0f, 100f) < targetStat[StatType.Evasion];

            if (attackResult.isEvaded)
                return attackResult;
            
            // TODO : 수식 논의 필요
            attackResult.isCritical = Random.Range(0f, 100f) < attackStat[StatType.CriticalRate];
            
            // Attack 인스턴스가 있는 경우 해당 인스턴스의 데이터 사용, 없으면 기본값 사용
            var attackDamageIncreasement = attackResult.attack?.getRelicStat(RelicStatType.DamageIncreasement) ?? 0;
            var damageMultiplier = attackResult.attack?.attackData?.damageMultiplier ?? 1.0f;
            var pureDamage = attackStat[StatType.AttackPower] * damageMultiplier *
                (100 + attackDamageIncreasement) / 100;
            var baseDamage = (int)(pureDamage) * 100 / (100 + targetStat[StatType.Defense]);
            
            attackResult.totalDamage = attackResult.isCritical ? baseDamage * (100 + attackStat[StatType.CriticalDamage]) / 100 : baseDamage;
            attackResult.attackerHealed = attackResult.totalDamage * attackStat[StatType.LifeSteal] / 100;
            attackResult.attackerDamage = attackResult.totalDamage * targetStat[StatType.Reflect] / 100;

            return attackResult;
        }
    }
}