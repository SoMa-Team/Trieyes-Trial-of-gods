using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;
using System;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// - 얼음 : 공격에 맞은 적들을 둔화 시킵니다.
    /// </summary>
    public class AC003_HeroSwordEnchantmentIce : AttackComponent
    {
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름
        
        public float debuffDuration = 3f; // 둔화 지속 시간

        public float debuffMultiplier = 10f;

        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        // FSM 상태 관리
        private IceAttackState attackState = IceAttackState.None;
        private float attackTimer = 0f;
        private Vector2 attackDirection;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹

        [SerializeField] private GameObject DebuffVFXPrefab; // DOT VFX 프리팹 (AC 전달용)
        private GameObject spawnedVFX;

        // 얼음 공격 상태 열거형
        private enum IceAttackState
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
            attackState = IceAttackState.Preparing;
            attackTimer = 0f;
            attackDirection = direction.normalized;

            // Radius를 공격자의 스탯 값으로 할당, Range / 10 = Radius
            attackRadius = attack.attacker.statSheet[StatType.AttackRange] / 10f;
            attackDuration = Mathf.Max(0.1f, 1f / (attack.attacker.statSheet[StatType.AttackSpeed] / 10f));
            
            // 얼음 공격 시작
            StartIceAttack();
        }

        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            if (eventType == Utils.EventType.OnKilled || eventType == Utils.EventType.OnKilledByCritical)
            {
                var _attacker = attack.attacker as Character001_Hero;
                if (_attacker != null)
                {
                    _attacker.killedDuringSkill001++;
                }
            }
        }

        private void StartIceAttack()
        {
            attackState = IceAttackState.Preparing;
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
            spawnedVFX = CreateAndSetupVFX(vfxPrefab, vfxPosition, attackDirection);
            
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
            
            //debug.log("<color=cyan>[AC004] 얼음 강화 공격 시작!</color>");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            var hero = attack.attacker as Character001_Hero;
            if (hero != null && hero.RAC012Trigger && targetPawn.bIsStatusValid(PawnStatusType.Freeze))
            {
                // 둔화 중첩 효과 처리
                ProcessSlowStackEffect(targetPawn);
                return;
            }

            // 기본 둔화 효과 적용
            ApplyBasicSlowEffect(targetPawn);
        }

        /// <summary>
        /// 둔화 중첩 효과를 처리합니다.
        /// </summary>
        /// <param name="targetPawn">대상</param>
        private void ProcessSlowStackEffect(Pawn targetPawn)
        {
            //둔화가 걸린 적이 다시 둔화에 걸리는 경우 해당 적의 방어력이 대폭 감소합니다.
            var _target = targetPawn as Enemy;
            if (_target != null)
            {
                var debuffInfo = new DebuffInfo
                {
                    debuffType = DEBUFFType.DecreaseDefense,
                    attack = attack,
                    target = targetPawn,
                    debuffMultiplier = debuffMultiplier,
                    debuffDuration = debuffDuration,
                };

                var debuff = new DEBUFF();
                debuff.Activate(debuffInfo);
            }

            targetPawn.AddStatus(PawnStatusType.Freeze, new PawnStatus 
            { duration = debuffDuration, lastTime = Time.time });
        }

        /// <summary>
        /// 기본 둔화 효과를 적용합니다.
        /// </summary>
        /// <param name="targetPawn">대상</param>
        private void ApplyBasicSlowEffect(Pawn targetPawn)
        {
            // 새로운 DEBUFF 클래스 사용
            var debuffInfo = new DebuffInfo
            {
                debuffType = DEBUFFType.Slow,
                attack = attack,
                target = targetPawn,
                debuffMultiplier = debuffMultiplier,
                debuffDuration = debuffDuration,
                debuffVFXPrefab = DebuffVFXPrefab,
            };

            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            targetPawn.AddStatus(PawnStatusType.Freeze, new PawnStatus
            {
                duration = debuffDuration,
                lastTime = Time.time,
            });
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
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 얼음 공격 상태 처리
            ProcessIceAttackState();

            if (attackState == IceAttackState.Active && attack.attackCollider != null)
            {
                ////DrawFanShapeDebug();
            }
        }

        private void ProcessIceAttackState()
        {
            switch (attackState)
            {
                case IceAttackState.None:
                    break;

                case IceAttackState.Preparing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 준비 시간
                    {
                        attackState = IceAttackState.Active;
                        attackTimer = 0f;
                        ActivateIceAttack();
                    }
                    break;

                case IceAttackState.Active:
                    attackTimer += Time.deltaTime;
                    
                    // 위치 업데이트
                    attack.transform.position = attack.attacker.transform.position;
                    attack.transform.rotation = Quaternion.Euler(0, 0, 0);
                    
                    if (attackTimer >= attackDuration)
                    {
                        attackState = IceAttackState.Finished;
                        attackTimer = 0f;
                        FinishIceAttack();
                    }
                    break;

                case IceAttackState.Finished:
                    attackState = IceAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateIceAttack()
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
            
            //debug.log("<color=green>[AC004] 얼음 강화 공격 활성화!</color>");
        }

        private void FinishIceAttack()
        {
            // 콜라이더 비활성화 (삭제하지 않고)
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = false;
            }
            
            // VFX 정리
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
            }
            
            //debug.log("<color=cyan>[AC004] 얼음 강화 공격 종료!</color>");
        }

        /// <summary>
        /// VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 기본 VFX 생성 (base 호출)
            if (spawnedVFX is null)
            {
                spawnedVFX = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            }
            
            spawnedVFX.transform.position = position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            spawnedVFX.transform.rotation = Quaternion.Euler(0, 0, angle);
            spawnedVFX.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            
            spawnedVFX.SetActive(true);
            return spawnedVFX;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            StopAndDestroyVFX(spawnedVFX);
        }
    }
}