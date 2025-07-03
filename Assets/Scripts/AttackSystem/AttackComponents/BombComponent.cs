using AttackSystem;
using CharacterSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace AttackComponents
{
    public class BombComponent : AttackComponent
    {
        public AttackData newAttackData;
        public int itemNumber;

        public override void ProcessComponentCollision(Pawn targetPawn, GameObject hitObject)
        {
            base.ProcessComponentCollision(targetPawn, hitObject);
            
            for (int i = 0; i < itemNumber; i++)
            {
                var itemAngle = Mathf.Lerp(0, 360, (float)i / (itemNumber - 1));
                var newDirection = new Vector2(Mathf.Cos(itemAngle * Mathf.Deg2Rad), Mathf.Sin(itemAngle * Mathf.Deg2Rad));
                var newAttack = AttackFactory.Instance.Create(newAttackData, attack.attacker, attack, newDirection);
                attack.AddAttack(newAttack);
            }
            
            attack.Deactivate();
        }
    }
}