using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// - 불꽃 : 공격에 맞은 대상에게 지속적으로 화상데미지(도트)를 입힙니다.
    /// </summary>
    public class AC003_HeroSwordEnchantmentFire : AttackComponent
    {
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름

        public Vector2 direction;
        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        // FSM 상태 관리
        private FireAttackState attackState = FireAttackState.None;
        private float attackTimer = 0f;
        private Vector2 attackDirection;

        // 불꽃 도트 데미지 필드
        public DOTTargetType dotTargetType = DOTTargetType.SingleTarget;
        public int dotDamage = 20;
        public float dotDuration = 2f;
        public float dotInterval = 0.2f;

        public AttackData dotAttackData;

        // 생성된 VFX 인스턴스
        [Header("VFX Settings")]
        [SerializeField] private readonly int VFX_ID = 1; // BASIC_ATTACK VFX ID
        private GameObject spawnedVFX;

        // 불꽃 공격 상태 열거형
        private enum FireAttackState
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
            attackState = FireAttackState.Preparing;
            attackTimer = 0f;
            attackDirection = direction.normalized;

            // Radius를 공격자의 스탯 값으로 할당, Range / 10 = Radius
            attackRadius = attack.attacker.statSheet[StatType.AttackRange] / 10f;
            
            // 불꽃 공격 시작
            StartFireAttack();
        }

        private void StartFireAttack()
        {
            attackState = FireAttackState.Preparing;
            attackTimer = 0f;
            
            // 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
            var pawnPrefab = attack.attacker.PawnPrefab;
            var weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;
            if (weaponGameObject == null)
            {
                Debug.LogError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            attack.transform.SetParent(weaponGameObject.transform);
            attack.transform.localPosition = Vector3.zero;
            attack.transform.localRotation = Quaternion.Euler(0, 0, 0);

            // 부채꼴 중심점 계산 (공격 방향으로 반지름의 절반만큼 이동)
            Vector2 vfxPosition = (Vector2)weaponGameObject.transform.position + (attackDirection * (attackRadius * 0.5f));
            
            // VFX 생성 및 설정
            spawnedVFX = CreateAndSetupVFX(vfxPosition, attackDirection);
            
            // VFX 재생
            PlayVFX(spawnedVFX);

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
            
            //debug.log("<color=orange>[AC003] 불꽃 강화 공격 시작!</color>");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // 단일 대상에게 도트 데미지를 주는 DOT 소환
            var dotAttack = AttackFactory.Instance.Create(dotAttackData, attack.attacker, null, Vector2.zero);

            var dotComponent = dotAttack.components[0] as AC101_DOT;
            if (dotComponent != null)
            {
                dotComponent.dotTargetType = dotTargetType;
                dotComponent.dotDamage = dotDamage;
                dotComponent.dotDuration = dotDuration;
                dotComponent.dotInterval = dotInterval;
                dotComponent.dotTargets.Add(targetPawn as Enemy);
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
            
            // 불꽃 공격 상태 처리
            ProcessFireAttackState();

            if (attackState == FireAttackState.Active && attack.attackCollider != null)
            {
                DrawFanShapeDebug();
            }
        }

        private void ProcessFireAttackState()
        {
            switch (attackState)
            {
                case FireAttackState.None:
                    break;

                case FireAttackState.Preparing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 준비 시간
                    {
                        attackState = FireAttackState.Active;
                        attackTimer = 0f;
                        ActivateFireAttack();
                    }
                    break;

                case FireAttackState.Active:
                    attackTimer += Time.deltaTime;
                    
                    // 위치 업데이트
                    attack.transform.position = attack.attacker.transform.position;
                    attack.transform.rotation = Quaternion.Euler(0, 0, 0);
                    
                    if (attackTimer >= attackDuration)
                    {
                        attackState = FireAttackState.Finishing;
                        attackTimer = 0f;
                        FinishFireAttack();
                    }
                    break;

                case FireAttackState.Finishing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 종료 시간
                    {
                        attackState = FireAttackState.Finished;
                    }
                    break;

                case FireAttackState.Finished:
                    attackState = FireAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateFireAttack()
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
            
            //debug.log("<color=green>[AC003] 불꽃 강화 공격 활성화!</color>");
        }

        private void FinishFireAttack()
        {
            // 콜라이더 비활성화 (삭제하지 않고)
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = false;
            }
            
            // VFX 정리
            if (spawnedVFX != null)
            {
                StopAndReturnVFX(spawnedVFX, VFX_ID);
                spawnedVFX = null;
            }
            
            //debug.log("<color=orange>[AC003] 불꽃 강화 공격 종료!</color>");
        }

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

        /// <summary>
        /// VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(Vector2 position, Vector2 direction)
        {
            // VFXFactory를 통해 기본 VFX 생성
            GameObject vfx = VFXFactory.Instance.SpawnVFX(VFX_ID, position, direction);
            
            // 여기서 VFX의 세부 조정을 할 수 있습니다
            // 예: 특정 파티클 시스템의 속성 변경, 스케일 조정 등
            ParticleSystem childVFX = vfx.transform.GetChild(0).GetComponent<ParticleSystem>();
            
            // Render Alignment 설정
            ParticleSystemRenderer renderer = childVFX.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.alignment = ParticleSystemRenderSpace.Local; // 또는 View, Local
            }

            vfx.transform.position = position;

            // 방향에 맞게 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            vfx.transform.rotation = Quaternion.Euler(0, 0, angle);
            vfx.transform.localScale = new Vector3(1.5f, 1.5f);
            
            var colorOverLifetime = childVFX.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.red, 0.4f), new GradientColorKey(Color.black, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(1f, 0f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            return vfx;
        }
    }
}