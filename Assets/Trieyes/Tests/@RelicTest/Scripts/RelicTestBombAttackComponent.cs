using AttackComponents;
using AttackSystem;
using CharacterSystem;
using RelicSystem;
using UnityEngine;

namespace Trieyes.Tests.RelicTest.Scripts
{
    public class RelicTestBombAttackComponent : AttackComponent
    {
        public int projectileCount = 8;
        public AttackData newAttackData;
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            projectileCount += attack.getRelicStat(RelicStatType.ProjectileCount);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            base.ProcessComponentCollision(targetPawn);
                
            for (int i = 0; i < projectileCount; i++)
            {
                var itemAngle = Mathf.Lerp(0, 360, (float)i / (projectileCount));
                var newDirection = new Vector2(Mathf.Cos(itemAngle * Mathf.Deg2Rad), Mathf.Sin(itemAngle * Mathf.Deg2Rad));
                var newAttack = AttackFactory.Instance.Create(newAttackData, attack.attacker, attack, newDirection);
                attack.AddAttack(newAttack);
            }
                
            AttackFactory.Instance.Deactivate(attack);
        }
    }
}