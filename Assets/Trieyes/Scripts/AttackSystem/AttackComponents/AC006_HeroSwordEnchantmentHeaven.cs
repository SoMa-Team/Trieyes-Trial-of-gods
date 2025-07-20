using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// 천상 : 이동속도와 사거리가 증가합니다. 방어력이 감소합니다. AC1001_BUFF 버프를 줍니다.
    /// </summary>
    public class AC006_HeroSwordEnchantmentHeaven : AttackComponent
    {
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름

        // FSM 상태 관리
        private HeavenAttackState attackState = HeavenAttackState.None;
        private float attackTimer = 0f;
        private Vector2 attackDirection;

        public Vector2 direction;
        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        // Skill 002에 대하여 AOE 공격 발동 시 AOE의 기본 정보들
        public AOETargetType dotCollisionType = AOETargetType.AreaAtPosition;
        public AOEShapeType dotShapeType = AOEShapeType.Circle;
        public float dotRadius = 3f;
        public float dotWidth = 1f;
        public float dotHeight = 1f;
        public int dotDamage = 100;
        public float dotDuration = 1f;
        public float dotInterval = 1f;

        public AttackData aoeAttackData;

        // 천상 공격 상태 열거형
        private enum HeavenAttackState
        {
            None,
            Preparing,
            Active,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = HeavenAttackState.Preparing;
            attackTimer = 0f;
            attackDirection = direction.normalized;

            // Radius를 공격자의 스탯 값으로 할당, Range / 10 = Radius
            attackRadius = attack.attacker.statSheet[StatType.AttackRange] / 10f;
        }

        private void StartHeavenAttack()
        {
            // 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
            var pawnPrefab = attack.attacker.PawnPrefab;
            var weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;
            if (weaponGameObject == null)
            {
                //debug.logError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            // 부모 설정을 하지 않고 위치만 동기화 (성능 최적화)
            attack.transform.position = weaponGameObject.transform.position;
            attack.transform.rotation = weaponGameObject.transform.rotation;

            // 콜라이더가 이미 존재하면 재사용, 없으면 새로 생성
            if (attack.attackCollider == null)
            {
                attack.attackCollider = attack.gameObject.AddComponent<PolygonCollider2D>();
            }
            
            var collider = attack.attackCollider as PolygonCollider2D;

            // 부채꼴 모양의 콜라이더 포인트 생성
            Vector2[] points = CreateFanShapePoints(attackDirection, attackAngle, attackRadius);
            collider.points = points;

            attack.attackCollider.isTrigger = true;
            attack.attackCollider.enabled = true;
            
            //debug.log("<color=white>[AC006] 천상 강화 공격 시작!</color>");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // 빛 속성일 때 AOE 공격
            var hero = attack.attacker as Character001_Hero;
            if (hero != null && hero.weaponElementState == HeroWeaponElementState.Light && hero.activateLight)
            {
                SpawnAC100Attack(targetPawn);
            }
        }
        
        /// <summary>
        /// Light 속성일 때 AC100 AOE 공격을 소환합니다
        /// </summary>
        /// <param name="targetPawn">타겟 적</param>
        private void SpawnAC100Attack(Pawn targetPawn)
        {
            var aoeAttack = AttackFactory.Instance.Create(aoeAttackData, attack.attacker, null, Vector2.zero);
            if (aoeAttack != null)
            {
                var aoeComponent = aoeAttack.components[0] as AC100_AOE;
                aoeComponent.aoeTargetType = dotCollisionType;
                aoeComponent.aoeShapeType = dotShapeType;
                aoeComponent.aoeRadius = dotRadius;
                aoeComponent.aoeDamage = dotDamage;
                aoeComponent.aoeDuration = dotDuration;
                aoeComponent.aoeInterval = dotInterval;

                // AOE 위치 설정 (타겟 위치)
                aoeComponent.SetAOEPosition((Vector2)targetPawn.transform.position);
                
                Debug.Log("<color=cyan>[AC006] Light 속성으로 AC100 AOE 공격 소환!</color>");
            }
        }

        /// <summary>
        /// 방향 벡터를 기준으로 부채꼴 모양의 콜라이더 포인트를 생성합니다.
        /// </summary>
        /// <param name="direction">기준 방향 벡터</param>
        /// <param name="totalAngle">전체 각도 (이 값의 절반씩 양쪽으로 회전)</param>
        /// <param name="radius">부채꼴 반지름</param>
        /// <returns>PolygonCollider2D에 사용할 포인트 배열</returns>
        private Vector2[] CreateFanShapePoints(Vector2 direction, float totalAngle, float radius)
        {
            // 중심점 + 호를 따라 생성되는 점들
            Vector2[] points = new Vector2[segments + 2];
            
            // 첫 번째 점은 중심점 (0, 0)
            points[0] = Vector2.zero;
            
            // 절반 각도로 시계 방향과 시계 반대 방향 계산
            float halfAngle = totalAngle * 0.5f;
            
            // 시계 방향과 시계 반대 방향 벡터 계산
            Vector2 clockwiseDirection = RotateVector2D(direction, -halfAngle);
            Vector2 counterClockwiseDirection = RotateVector2D(direction, halfAngle);

            ////debug.log($"clockwiseDirection: {clockwiseDirection}, counterClockwiseDirection: {counterClockwiseDirection}");
            
            // 부채꼴 호를 따라 점들 생성
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments; // 0부터 1까지
                
                // 시계 방향에서 시계 반대 방향으로 보간
                Vector2 currentDirection = Vector2.Lerp(clockwiseDirection, counterClockwiseDirection, t).normalized;
                points[i + 1] = currentDirection * radius;
            }
            
            return points;
        }

        /// <summary>
        /// 2D 벡터를 주어진 각도만큼 회전시킵니다.
        /// </summary>
        /// <param name="vector">회전시킬 벡터</param>
        /// <param name="degrees">회전 각도 (도 단위)</param>
        /// <returns>회전된 벡터</returns>
        private Vector2 RotateVector2D(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        protected override void Update()
        {
            base.Update();
            
            // 천상 공격 상태 처리
            ProcessHeavenAttackState();

            // 디버그 그리기는 개발 모드에서만 (성능 최적화)
            #if UNITY_EDITOR
            if (attackState == HeavenAttackState.Active && attack.attackCollider != null)
            {
                DrawFanShapeDebug();
            }
            #endif
        }

        /// <summary>
        /// 부채꼴 모양을 Scene 뷰에 디버그 라인으로 그립니다.
        /// </summary>
        private void DrawFanShapeDebug()
        {
            if (attack.attackCollider is PolygonCollider2D collider)
            {
                Vector2[] points = collider.points;
                
                // 부채꼴 모양 그리기
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector3 startPos = attack.transform.position + new Vector3(points[i].x, points[i].y, 0);
                    Vector3 endPos = attack.transform.position + new Vector3(points[i + 1].x, points[i + 1].y, 0);
                    Debug.DrawLine(startPos, endPos, Color.yellow, 0.1f);
                }
                
                // 마지막 점과 첫 번째 점을 연결 (폐곡선 만들기)
                if (points.Length > 2)
                {
                    Vector3 lastPos = attack.transform.position + new Vector3(points[points.Length - 1].x, points[points.Length - 1].y, 0);
                    Vector3 firstPos = attack.transform.position + new Vector3(points[1].x, points[1].y, 0);
                    Debug.DrawLine(lastPos, firstPos, Color.yellow, 0.1f);
                }
            }
        }

        private void ActivateHeavenAttack()
        {
            // 콜라이더 활성화 및 방향 업데이트
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = true;
                
                // 플레이어의 현재 방향으로 콜라이더 포인트 재계산
                var collider = attack.attackCollider as PolygonCollider2D;
                if (collider != null)
                {
                    // 현재 플레이어의 이동 방향 가져오기
                    Vector2 currentDirection = attack.attacker.LastMoveDirection;
                    if (currentDirection.magnitude < 0.1f)
                    {
                        // 이동하지 않을 때는 이전 방향 유지
                        currentDirection = attackDirection;
                    }
                    else
                    {
                        attackDirection = currentDirection.normalized;
                    }
                    
                    // 새로운 방향으로 콜라이더 포인트 재계산
                    Vector2[] points = CreateFanShapePoints(attackDirection, attackAngle, attackRadius);
                    collider.points = points;
                }
            }
            
            //debug.log("<color=green>[AC006] 천상 강화 공격 활성화!</color>");
        }

        private void FinishHeavenAttack()
        {
            // 콜라이더 비활성화 (삭제하지 않고)
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = false;
            }
            
            //debug.log("<color=white>[AC006] 천상 강화 공격 종료!</color>");
        }

        private void ProcessHeavenAttackState()
        {
            switch (attackState)
            {
                case HeavenAttackState.None:
                    break;

                case HeavenAttackState.Preparing:
                    attackTimer += Time.deltaTime;
                    
                    StartHeavenAttack();
                    
                    if (attackTimer >= 0.1f) // 준비 시간
                    {
                        attackState = HeavenAttackState.Active;
                        attackTimer = 0f;
                        ActivateHeavenAttack();
                    }
                    break;

                case HeavenAttackState.Active:
                    attackTimer += Time.deltaTime;
                    
                    // 위치 업데이트 (성능 최적화: 필요할 때만)
                    if (attack.attacker != null)
                    {
                        attack.transform.position = attack.attacker.transform.position;
                    }
                    
                    if (attackTimer >= attackDuration)
                    {
                        attackState = HeavenAttackState.Finishing;
                        attackTimer = 0f;
                    }
                    break;

                case HeavenAttackState.Finishing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 종료 시간
                    {
                        attackState = HeavenAttackState.Finished;
                        FinishHeavenAttack();
                    }
                    break;

                case HeavenAttackState.Finished:
                    attackState = HeavenAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }
    }
}