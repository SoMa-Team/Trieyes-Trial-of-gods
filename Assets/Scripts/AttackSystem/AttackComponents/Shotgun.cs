using AttackSystem;
using UnityEngine;

namespace AttackComponents
{
    public class Shotgun : AttackComponent
    {
        public AttackData newAttackData;
        public int itemNumber;
        public float angle;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);

            var baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i < itemNumber; i++)
            {
                var itemAngle = Mathf.Lerp(-angle / 2, angle / 2, (float)i / (itemNumber - 1));
                var newAngle = baseAngle + itemAngle;
                var newDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
                var newAttack = AttackFactory.Instance.Create(newAttackData, attack.attacker, attack, newDirection);
                attack.AddAttack(newAttack);
            }
        }
    }
}