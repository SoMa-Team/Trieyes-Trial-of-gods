using AttackSystem;
using UnityEngine;
using System;
using CharacterSystem;
using Stats;
using BattleSystem;
using System.Collections.Generic;
using VFXSystem;

namespace AttackComponents
{
    public enum ProjectileColliderType
    {
        Box,     // 네모 콜라이더
        Capsule  // 캡슐 콜라이더
    }

    public enum ProjectileDestroyType
    {
        OnHit,      // 충돌 시 파괴
        OnDistance, // 거리 제한
        OnTime      // 시간 제한
    }

    /// <summary>
    /// 특정 방향으로 이동하는 발사체 컴포넌트
    /// 네모/캡슐 콜라이더와 VFX 시스템을 포함합니다.
    /// </summary>
    public class AC106_Projectile : AttackComponent
    {   
        // 발사체 콜라이더 설정
        [Header("발사체 콜라이더 설정")]
        public ProjectileColliderType colliderType = ProjectileColliderType.Box;
        public float colliderWidth = 0.5f;   // 네모일 때 가로
        public float colliderHeight = 0.5f;  // 네모일 때 세로
        public float colliderRadius = 0.25f; // 캡슐일 때 반지름

        // 관통 설정
        [Header("관통 설정")]
        public int pierceCount = 0;          // 관통 횟수 (0이면 무한 관통, 화면 벗어날 때까지)
        private int currentPierceCount = 0;   // 현재 관통 횟수

        // 발사체 이동 설정
        [Header("발사체 이동 설정")]
        public float projectileSpeed = 5f;    // 이동 속도 (외부에서 설정 가능)
        public Vector2 projectileDirection;   // 이동 방향 (외부에서 설정)

        // 발사체 생명주기 설정
        [Header("발사체 생명주기 설정")]
        public ProjectileDestroyType destroyType = ProjectileDestroyType.OnHit;
        public float maxDistance = 10f;       // 최대 이동 거리
        public float maxLifetime = 5f;        // 최대 생존 시간

        // 발사체 상태 관리
        private Vector2 startPosition;
        private float currentDistance = 0f;
        private float currentLifetime = 0f;
        private bool isDestroyed = false;
        private List<Enemy> hitTargets = new List<Enemy>(10); // 재사용 가능한 리스트 (AC100~105 패턴)

        // VFX 설정
        [Header("VFX 설정")]
        [SerializeField] public GameObject projectileVFXPrefab; // 발사체 VFX 프리팹
        private GameObject spawnedVFX;

        // FSM 상태 관리
        private ProjectileState projectileState = ProjectileState.None;

        // 발사체 상태 열거형
        private enum ProjectileState
        {
            None,
            Preparing,
            Active,
            Destroying,
            Destroyed
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            projectileState = ProjectileState.Preparing;
            projectileDirection = direction.normalized;
            startPosition = attack.transform.position;
            currentDistance = 0f;
            currentLifetime = 0f;
            currentPierceCount = 0;
            isDestroyed = false;
            hitTargets.Clear(); // AC100~105 패턴: 리스트 초기화
            
            // 발사체 시작
            StartProjectile();
        }

        /// <summary>
        /// 발사체 속도를 설정합니다.
        /// </summary>
        /// <param name="speed">발사체 속도</param>
        public void SetProjectileSpeed(float speed)
        {
            projectileSpeed = speed;
        }

        /// <summary>
        /// 발사체 방향을 설정합니다.
        /// </summary>
        /// <param name="direction">발사체 방향</param>
        public void SetProjectileDirection(Vector2 direction)
        {
            projectileDirection = direction.normalized;
        }

        /// <summary>
        /// 발사체 콜라이더 크기를 설정합니다.
        /// </summary>
        /// <param name="width">가로 크기</param>
        /// <param name="height">세로 크기</param>
        /// <param name="radius">반지름 (캡슐일 때)</param>
        public void SetProjectileColliderSize(float width, float height, float radius)
        {
            colliderWidth = width;
            colliderHeight = height;
            colliderRadius = radius;
        }

        /// <summary>
        /// 관통 횟수를 설정합니다.
        /// </summary>
        /// <param name="pierce">관통 횟수</param>
        public void SetPierceCount(int pierce)
        {
            pierceCount = pierce;
        }

        private void StartProjectile()
        {
            projectileState = ProjectileState.Preparing;
            
            // 콜라이더 설정
            SetupProjectileCollider();
            
            // VFX 생성 및 설정 (AC100~105 패턴)
            spawnedVFX = CreateAndSetupVFX(projectileVFXPrefab, attack.transform.position, projectileDirection);
            PlayVFX(spawnedVFX);
            
            Debug.Log("<color=orange>[AC106] 발사체 시작!</color>");
        }

        private void SetupProjectileCollider()
        {
            // 기존 콜라이더 제거
            if (attack.attackCollider != null)
            {
                DestroyImmediate(attack.attackCollider);
            }

            switch (colliderType)
            {
                case ProjectileColliderType.Box:
                    // BoxCollider2D 생성
                    var boxCollider = attack.gameObject.AddComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(colliderWidth, colliderHeight);
                    boxCollider.isTrigger = true;
                    attack.attackCollider = boxCollider;
                    break;

                case ProjectileColliderType.Capsule:
                    // CapsuleCollider2D 생성
                    var capsuleCollider = attack.gameObject.AddComponent<CapsuleCollider2D>();
                    capsuleCollider.size = new Vector2(colliderWidth, colliderHeight);
                    capsuleCollider.isTrigger = true;
                    attack.attackCollider = capsuleCollider;
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // 발사체 상태 처리
            ProcessProjectileState();
        }

        private void ProcessProjectileState()
        {
            switch (projectileState)
            {
                case ProjectileState.None:
                    break;

                case ProjectileState.Preparing:
                    projectileState = ProjectileState.Active;
                    break;

                case ProjectileState.Active:
                    // 발사체 이동
                    MoveProjectile();
                    
                    // 생명주기 체크
                    if (ShouldDestroyProjectile())
                    {
                        projectileState = ProjectileState.Destroying;
                        DestroyProjectile();
                    }
                    break;

                case ProjectileState.Destroying:
                    projectileState = ProjectileState.Destroyed;
                    break;

                case ProjectileState.Destroyed:
                    projectileState = ProjectileState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void MoveProjectile()
        {
            // 발사체 이동
            Vector2 movement = projectileDirection * projectileSpeed * Time.deltaTime;
            attack.transform.position += new Vector3(movement.x, movement.y, 0);
            
            // VFX도 함께 이동
            if (spawnedVFX != null)
            {
                spawnedVFX.transform.position = attack.transform.position;
            }
            
            // 발사체 회전 (이동 방향에 따라)
            float angle = Mathf.Atan2(projectileDirection.y, projectileDirection.x) * Mathf.Rad2Deg;
            attack.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 거리 및 시간 업데이트
            currentDistance = Vector2.Distance(startPosition, attack.transform.position);
            currentLifetime += Time.deltaTime;
        }

        private bool ShouldDestroyProjectile()
        {
            if (isDestroyed) return true;
            
            // 관통 횟수 체크
            if (pierceCount > 0 && currentPierceCount >= pierceCount)
            {
                return true;
            }
            
            switch (destroyType)
            {
                case ProjectileDestroyType.OnDistance:
                    return currentDistance >= maxDistance;
                    
                case ProjectileDestroyType.OnTime:
                    return currentLifetime >= maxLifetime;
                    
                case ProjectileDestroyType.OnHit:
                default:
                    return false; // 충돌 시에만 파괴
            }
        }

        private void DestroyProjectile()
        {
            isDestroyed = true;
            
            // VFX 정리 (AC100~105 패턴)
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }
            
            // 콜라이더 비활성화
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = false;
            }
            
            Debug.Log("<color=orange>[AC106] 발사체 파괴!</color>");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // 충돌 시 데미지 처리 (AC100~105 패턴)
            if (targetPawn != null && targetPawn != attacker)
            {
                // 기본 데미지 설정 (필요시 수정)
                attack.statSheet[StatType.AttackPower] = new IntegerStatValue(20);
                DamageProcessor.ProcessHit(attack, targetPawn);
                
                // AC100~105 패턴: 타겟 리스트에 추가
                if (targetPawn is Enemy enemy && !hitTargets.Contains(enemy))
                {
                    hitTargets.Add(enemy);
                }
                
                Debug.Log($"<color=yellow>[AC106] 발사체가 {targetPawn.name}에게 충돌! (관통: {currentPierceCount}/{pierceCount})</color>");
                
                // 관통 로직 처리
                if (pierceCount == 0)
                {
                    // 무한 관통: 데미지만 주고 계속 진행
                    Debug.Log("<color=green>[AC106] 무한 관통으로 계속 진행!</color>");
                }
                else
                {
                    // 제한된 관통: 횟수 증가
                    currentPierceCount++;
                    Debug.Log($"<color=orange>[AC106] 관통 횟수 증가: {currentPierceCount}/{pierceCount}</color>");
                    
                    // 관통 횟수 초과 시 파괴
                    if (currentPierceCount >= pierceCount)
                    {
                        projectileState = ProjectileState.Destroying;
                        DestroyProjectile();
                    }
                }
                
                // OnHit 타입이면 즉시 파괴 (관통과 무관하게)
                if (destroyType == ProjectileDestroyType.OnHit)
                {
                    projectileState = ProjectileState.Destroying;
                    DestroyProjectile();
                }
            }
        }

        /// <summary>
        /// 발사체 VFX를 생성하고 설정합니다. (AC100~105 패턴)
        /// </summary>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 프리팹이 없으면 VFX 없이 진행
            if (vfxPrefab == null)
            {
                return null;
            }

            // 기본 VFX 생성 (base 호출)
            if (spawnedVFX is null)
            {
                spawnedVFX = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            }
            
            spawnedVFX.transform.position = position;
            
            // 발사 방향에 따라 VFX 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            spawnedVFX.transform.rotation = Quaternion.Euler(0, 0, angle);
            
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