using UnityEngine;
using BattleSystem;
using Stats;

namespace CharacterSystem.Enemies
{
    public enum EnemyType
    {
        Follow,
        RangeAttackRun,
        RangeAttackOnly,
        Block,
        Boss
    }

    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class EnemyController : Controller
    {
        public float minFollowDistance = 0.1f; // 너무 가까우면 멈춤

        public float minRunDistance = 1f;

        public EnemyType enemyType;
        private Transform playerTarget;

        private Enemy enemy;

        private Vector3 targetCollisionOffset;

        private void Update()
        {
            if (owner == null || playerTarget == null || enemy.isDead)
            {
                return;
            }

            Behaviour();
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);

            var playerObj = BattleStage.now.mainCharacter.gameObject;
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                targetCollisionOffset = BattleStage.now.mainCharacter.CenterOffset;
            }
            
            enemy = pawn as Enemy;
        }

        private void Behaviour()
        {
            var attackRange = enemy.statSheet[StatType.AttackRange];
            var playerPos = playerTarget.position;
            var enemyPos = transform.position;
            if (!lockMovement)
            {
                switch (enemyType)
                {
                    default:
                    case EnemyType.Follow:
                        Vector2 toPlayer = (playerTarget.position - transform.position + (Vector3)targetCollisionOffset);
                        float dist = toPlayer.magnitude;
                        if (dist > minFollowDistance)
                        {
                            enemy.Move(toPlayer.normalized);
                        }
                        else
                        {
                            enemy.Move(Vector2.zero); // 너무 가까우면 멈춤
                        }
                        break;
                    case EnemyType.RangeAttackRun:
                        float distanceToPlayer = Vector2.Distance(playerPos, enemyPos);
                        
                        // 1. 플레이어와의 거리가 minRunDistance 이내로 들어오면 도망침
                        if (distanceToPlayer <= minRunDistance)
                        {
                            // 플레이어 방향 벡터의 반대 방향으로 도망침
                            Vector2 runDirection = (enemyPos - playerPos).normalized;
                            enemy.Move(runDirection);
                        }
                        // 2. 공격 사거리 내에 있는 경우 공격
                        else if (distanceToPlayer <= attackRange)
                        {
                            enemy.Move(Vector2.zero);
                            enemy.ExecuteAttack();
                        }
                        // 3. 기본적으로 공격 사거리 내로 이동
                        else
                        {
                            Vector2 moveDirection = (playerPos - enemyPos).normalized;
                            enemy.Move(moveDirection);
                        }
                        break;
                    case EnemyType.RangeAttackOnly:
                        // owner 사정거리 정보 가져옴
                        if (Vector2.Distance(playerPos, enemyPos) <= attackRange)
                        {
                            enemy.Move(Vector2.zero);
                            enemy.ExecuteAttack();
                        }
                        else
                        {
                            enemy.Move(playerPos - enemyPos);
                        }
                        break;
                    case EnemyType.Boss:
                        break;
                }
            }
        }
    }
}