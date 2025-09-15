using UnityEngine;
using CharacterSystem;
using BattleSystem;
using Stats;

namespace Enemies
{
    public enum BossState
    {
        Idle,
        Move,

        InitDash, // VFX 같은 거 만들고
        Dash,
    }

    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class B001_Controller : EnemyController
    {
        B001_Roki bossOwner;
        private Character Target;
        private BossState currentState;

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            bossOwner = pawn as B001_Roki;
            Target = BattleStage.now.mainCharacter;
            currentState = BossState.Idle;
        }

        private void Update()
        {
            if (bossOwner == null || Target == null || bossOwner.isDead)
            {
                return;
            }

            Behaviour();
        }

        private void Behaviour()
        {
            var attackRange = bossOwner.statSheet[StatType.AttackRange];
            var playerPos = Target.transform.position;
            var enemyPos = transform.position;
            Vector2 toPlayer = (playerPos - enemyPos);

            if(!lockMovement)
            {
                if (toPlayer.magnitude <= bossOwner.dashRange)
                {
                    bossOwner.Move(toPlayer.normalized);
                }
                else if (bossOwner.CheckCooldown(PawnAttackType.Skill1))
                {
                    bossOwner.ExecuteAttack(PawnAttackType.Skill1);
                }
                else
                {
                    bossOwner.Move(toPlayer.normalized);
                }
            }
        }
    }
}