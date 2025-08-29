using AttackSystem;
using UnityEngine;

namespace AttackComponents
{
    public class StraightMove : AttackComponent
    {
        public float angularSpeed = 10.0f; // 회전 속도
        public float speed = 1.0f; // 이동 속도
        public Vector3 direction; // 이동 방향

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction); // 기본 활성화
            this.direction = new Vector3(direction.x, direction.y, 0);
        }

        protected override void Update()
        {
            base.Update(); // 기본 업데이트

            attack.transform.localPosition += (speed * Time.deltaTime) * direction;
            attack.transform.Rotate(0, 0, angularSpeed * Time.deltaTime);
        }
    }
}
