using AttackSystem;
using UnityEngine;

namespace AttackComponents
{
    public class Shotgun : AttackComponent
    {
        public AttackData newAttackData; // 생성할 공격 데이터
        public int itemNumber; // 생성할 투사체 개수
        public float angle; // 퍼지는 각도

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction); // 기본 활성화

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
