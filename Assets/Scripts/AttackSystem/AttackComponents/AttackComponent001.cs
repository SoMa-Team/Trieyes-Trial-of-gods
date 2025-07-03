using AttackSystem;
using UnityEngine;

namespace AttackComponents
{
    public class AttackComponent001 : AttackComponent
    {
        public float angularSpeed = 10.0f;
        public float speed = 1.0f;
        public Vector3 direction;

        public override void Activate(Attack attack)
        {
            base.Activate(attack);
            direction = attack.transform.right.normalized;
        }

        protected override void Update()
        {
            base.Update();

            attack.transform.localPosition += (speed * Time.deltaTime) * direction;
            attack.transform.Rotate(0, 0, angularSpeed * Time.deltaTime);
        }
    }
}