using AttackSystem;
using UnityEngine;

namespace AttackComponents
{
    public class StraightMove : AttackComponent
    {
        public float angularSpeed = 10.0f;
        public float speed = 1.0f;
        public Vector3 direction;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            this.direction = new Vector3(direction.x, direction.y, 0);
        }

        protected override void Update()
        {
            base.Update();

            attack.transform.localPosition += (speed * Time.deltaTime) * direction;
            attack.transform.Rotate(0, 0, angularSpeed * Time.deltaTime);
        }
    }
}