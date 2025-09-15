using UnityEngine;
using CharacterSystem;

namespace Enemies
{
    public class E005_Controller : EnemyController
    {
        public override void Update()
        {
            if (owner == null || playerTarget == null || enemy.isDead)
            {
                return;
            }

            base.Update();
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            enemy = pawn as E005_BlueGolem;
        }
    }
}