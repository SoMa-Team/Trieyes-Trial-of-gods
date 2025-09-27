using UnityEngine;
using BattleSystem;
using CharacterSystem;
using Stats;

namespace Enemies
{

    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class EnemyController : Controller
    {
        public EnemyType enemyType;

        protected Transform playerTarget;

        protected Enemy enemy;

        protected float minFollowDistance = 0.1f; // 너무 가까우면 멈춤

        protected float minFollowRandomDistance = 2f;

        protected float minRandomRadius = 0.3f;
        protected float maxRandomRadius = 1.5f;

        protected float minRunDistance = 1f;

        protected float recalculateRandomRadiusInterval = 0.5f;

        protected Vector3 targetCollisionOffset;
        
        // 랜덤 추적 관련 변수들
        private Vector3 randomTargetPosition;
        private float lastRandomTargetUpdateTime;
        private bool hasRandomTarget = false;

        public override void Update()
        {
            if (owner == null || playerTarget == null || enemy.isDead)
            {
                return;
            }

            base.Update();
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
            
            // 랜덤 타겟 초기화
            lastRandomTargetUpdateTime = Time.time;
            GenerateRandomTarget();
        }

        /// <summary>
        /// 플레이어 주변의 랜덤 타겟 위치를 생성합니다.
        /// </summary>
        private void GenerateRandomTarget()
        {
            if (playerTarget == null) return;
            
            // 플레이어 위치를 중심으로 maxRandomRadius 반경 내의 랜덤 위치 생성
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minRandomRadius, maxRandomRadius);
            
            randomTargetPosition = playerTarget.position + (Vector3)(randomDirection * randomDistance);
            hasRandomTarget = true;
            lastRandomTargetUpdateTime = Time.time;
        }
        
        /// <summary>
        /// 랜덤 타겟이 업데이트되어야 하는지 확인합니다.
        /// </summary>
        private bool ShouldUpdateRandomTarget()
        {
            return Time.time - lastRandomTargetUpdateTime >= recalculateRandomRadiusInterval;
        }
        
        /// <summary>
        /// 랜덤 타겟을 사용하여 플레이어를 추적합니다.
        /// </summary>
        private void FollowPlayerWithRandomTarget()
        {
            Vector2 toPlayer = (playerTarget.position - transform.position + (Vector3)targetCollisionOffset);
            float distToPlayer = toPlayer.magnitude;
            
            // 플레이어와의 거리가 minFollowRandomDistance 이상인 경우
            if (distToPlayer >= minFollowRandomDistance)
            {
                // 랜덤 타겟으로 이동
                if (hasRandomTarget)
                {
                    Vector2 toRandomTarget = (randomTargetPosition - transform.position);
                    float distToRandomTarget = toRandomTarget.magnitude;
                    
                    if (distToRandomTarget > 0.1f) // 랜덤 타겟에 도달하지 않았으면
                    {
                        enemy.Move(toRandomTarget.normalized);
                    }
                    else
                    {
                        // 랜덤 타겟에 도달했으면 새로운 타겟 생성
                        GenerateRandomTarget();
                    }
                }
                else
                {
                    // 랜덤 타겟이 없으면 생성
                    GenerateRandomTarget();
                }
            }
            // 플레이어와의 거리가 minFollowRandomDistance 이하인 경우
            else
            {
                // 기본 추적 로직 (플레이어 중심으로 이동)
                if (distToPlayer > minFollowDistance)
                {
                    enemy.Move(toPlayer.normalized);
                }
                else
                {
                    enemy.ExecuteAttack();
                    enemy.Move(Vector2.zero); // 너무 가까우면 멈춤
                }
            }
        }

        protected virtual void Behaviour()
        {
            var attackRange = enemy.statSheet[StatType.AttackRange];
            var playerPos = playerTarget.position;
            var enemyPos = transform.position;
            
            // 랜덤 타겟 업데이트 확인
            if (ShouldUpdateRandomTarget())
            {
                GenerateRandomTarget();
            }
            
            if (!lockMovement)
            {
                switch (enemyType)
                {
                    default:
                    case EnemyType.Follow:
                        FollowPlayerWithRandomTarget();
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