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
    public enum OrbitDirection
    {
        Clockwise,      // 시계 방향
        CounterClockwise // 반시계 방향
    }

    public enum CollisionBehavior
    {
        Continue,       // 충돌 후 계속 공전
        Destroy         // 충돌 후 파괴
    }

            /// <summary>
        /// 지정된 대상 주위를 공전하는 요소 컴포넌트
        /// 원형/타원형 공전, VFX 시스템, 충돌 처리를 포함합니다.
        /// </summary>
    public class AC107_OrbitingElement : AttackComponent
    {   
        // 공전 대상 설정
        [Header("공전 대상 설정")]
        public Transform orbitTarget;      // 공전할 대상 (외부에서 설정)
        public OrbitDirection orbitDirection = OrbitDirection.Clockwise;

        // 공전 설정
        [Header("공전 설정")]
        public float orbitRadius = 2f;     // 공전 반지름 (AC000에서 설정)
        public float orbitSpeed = 90f;     // 공전 속도 (도/초, AC000에서 설정)
        public float orbitOffset = 0f;     // 공전 시작 각도
        public float ellipseRatio = 1.0f;  // 타원 비율 (1.0 = 원형, >1.0 = 가로 긴 타원, <1.0 = 세로 긴 타원)

        // 콜라이더 설정
        [Header("콜라이더 설정")]
        public int colliderCount = 4;      // 콜라이더 개수
        public float colliderWidth = 0.5f; // 콜라이더 가로 크기
        public float colliderHeight = 0.5f; // 콜라이더 세로 크기
        public float colliderRadius = 1f;  // 콜라이더 공전 반지름
        public CollisionBehavior collisionBehavior = CollisionBehavior.Continue;

        // 공전 상태 관리
        private float currentAngle = 0f;
        private Vector2 orbitCenter;
        private List<GameObject> orbitingColliders = new List<GameObject>();
        private List<bool> colliderActive = new List<bool>();
        private List<Enemy> hitTargets = new List<Enemy>(10); // 재사용 가능한 리스트

        // VFX 설정
        [Header("VFX 설정")]
        [SerializeField] public GameObject orbitVFXPrefab; // 공전 VFX 프리팹
        private List<GameObject> spawnedVFXs = new List<GameObject>(); // 각 콜라이더별 VFX

        // FSM 상태 관리
        private OrbitState orbitState = OrbitState.Preparing;

        // 공전 상태 열거형
        public enum OrbitState
        {
            Preparing,
            Active,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            orbitState = OrbitState.Preparing;
            currentAngle = orbitOffset;
            hitTargets.Clear();
            
            Debug.Log("<color=purple>[AC107] 공전 요소 초기화 완료! (Preparing 상태)</color>");
        }

        /// <summary>
        /// 공전 대상을 설정합니다.
        /// </summary>
        /// <param name="target">공전할 대상</param>
        public void SetOrbitTarget(Transform target)
        {
            orbitTarget = target;
        }

        /// <summary>
        /// 공전 설정을 합니다.
        /// </summary>
        /// <param name="radius">공전 반지름</param>
        /// <param name="speed">공전 속도</param>
        /// <param name="ellipseRatio">타원 비율 (1.0 = 원형)</param>
        public void SetOrbitParameters(float radius, float speed, float ellipseRatio = 1.0f)
        {
            orbitRadius = radius;
            orbitSpeed = speed;
            this.ellipseRatio = ellipseRatio;
        }

        /// <summary>
        /// 콜라이더 설정을 합니다.
        /// </summary>
        /// <param name="count">콜라이더 개수</param>
        /// <param name="width">콜라이더 가로 크기</param>
        /// <param name="height">콜라이더 세로 크기</param>
        /// <param name="radius">콜라이더 공전 반지름</param>
        public void SetColliderParameters(int count, float width, float height, float radius)
        {
            colliderCount = count;
            colliderWidth = width;
            colliderHeight = height;
            colliderRadius = radius;
        }

        /// <summary>
        /// 외부에서 공전 요소를 Active 상태로 전환합니다.
        /// </summary>
        public void ActivateOrbiting()
        {
            if (orbitState == OrbitState.Preparing)
            {
                StartOrbiting();
                CreateOrbitingColliders(); // 별(콜라이더와 VFX) 생성

                orbitState = OrbitState.Active;
                Debug.Log("<color=green>[AC107] 외부에서 공전 요소 Active 상태로 전환!</color>");
            }
            else
            {
                Debug.LogWarning($"[AC107] 현재 상태({orbitState})에서는 Active로 전환할 수 없습니다!");
            }
        }
        
        private void StartOrbiting()
        {
            // 공전 중심 설정
            if (orbitTarget != null)
            {
                orbitCenter = orbitTarget.position;
            }
            else
            {
                orbitCenter = attack.transform.position;
            }
            
            Debug.Log("<color=purple>[AC107] 공전 요소 시작!</color>");
        }

        private void CreateOrbitingColliders()
        {
            // 기존 콜라이더 및 VFX 정리
            foreach (var collider in orbitingColliders)
            {
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
            foreach (var vfx in spawnedVFXs)
            {
                if (vfx != null)
                {
                    StopAndDestroyVFX(vfx);
                }
            }
            orbitingColliders.Clear();
            colliderActive.Clear();
            spawnedVFXs.Clear();

            // 새로운 콜라이더 생성 (360/콜라이더 개수로 고정 위치에 먼저 생성)
            float angleStep = 360f / colliderCount;
            for (int i = 0; i < colliderCount; i++)
            {
                float angle = orbitOffset + (angleStep * i);
                Vector2 position = CalculateOrbitPosition(angle);
                
                GameObject colliderObj = CreateColliderObject(position, angle);
                orbitingColliders.Add(colliderObj);
                colliderActive.Add(true);
                
                // 각 콜라이더마다 개별 VFX 생성
                GameObject vfx = CreateAndSetupVFX(orbitVFXPrefab, position, Vector2.zero);
                if (vfx != null)
                {
                    PlayVFX(vfx);
                    spawnedVFXs.Add(vfx);
                }
            }
        }

        private Vector2 CalculateOrbitPosition(float angle)
        {
            // 타원형 공전 계산 (기본 원형, 타원형 지원)
            float radian = angle * Mathf.Deg2Rad;
            float x = orbitCenter.x + (orbitRadius * Mathf.Cos(radian) * ellipseRatio);
            float y = orbitCenter.y + (orbitRadius * Mathf.Sin(radian));
            
            return new Vector2(x, y);
        }

        private GameObject CreateColliderObject(Vector2 position, float angle)
        {
            GameObject colliderObj = new GameObject($"OrbitingCollider_{orbitingColliders.Count}");
            colliderObj.transform.position = position;
            colliderObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            colliderObj.transform.SetParent(attack.transform);
            
            // BoxCollider2D 추가
            BoxCollider2D boxCollider = colliderObj.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(colliderWidth, colliderHeight);
            boxCollider.isTrigger = true;
            
            // Rigidbody2D 추가 (충돌 감지용)
            Rigidbody2D rb = colliderObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            
            // 충돌 감지 컴포넌트 추가
            var collisionDetector = colliderObj.AddComponent<OrbitingCollisionDetector>();
            collisionDetector.Initialize(this, orbitingColliders.Count);
            
            return colliderObj;
        }

        protected override void Update()
        {
            // AC107은 거리 제한을 무시하고 공전 상태만 처리
            // base.Update() 호출하지 않음 (거리 체크 방지)
            
            // 공전 상태 처리
            ProcessOrbitState();
        }

        private void ProcessOrbitState()
        {
            switch (orbitState)
            {
                case OrbitState.Preparing:
                    break;

                case OrbitState.Active:
                    // 공전 업데이트
                    UpdateOrbit();
                    break;

                case OrbitState.Finishing:
                    orbitState = OrbitState.Finished;
                    break;

                case OrbitState.Finished:
                    orbitState = OrbitState.Preparing;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void UpdateOrbit()
        {
            // 공전 중심 업데이트
            if (orbitTarget != null)
            {
                orbitCenter = orbitTarget.position;
            }

            // 공전 각도 업데이트 (AC000에서 설정한 속도로)
            float directionMultiplier = (orbitDirection == OrbitDirection.Clockwise) ? 1f : -1f;
            currentAngle += orbitSpeed * directionMultiplier * Time.deltaTime;
            
            // 모든 콜라이더와 VFX를 함께 회전 (고정 위치에서 시작하여 계속 회전)
            float angleStep = 360f / colliderCount;
            for (int i = 0; i < orbitingColliders.Count; i++)
            {
                if (colliderActive[i] && orbitingColliders[i] != null)
                {
                    float angle = currentAngle + (angleStep * i);
                    Vector2 position = CalculateOrbitPosition(angle);
                    
                    orbitingColliders[i].transform.position = position;
                    orbitingColliders[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                    
                    // 해당 VFX도 함께 이동
                    if (i < spawnedVFXs.Count && spawnedVFXs[i] != null)
                    {
                        spawnedVFXs[i].transform.position = position;
                        spawnedVFXs[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 콜라이더의 충돌을 처리합니다.
        /// </summary>
        /// <param name="colliderIndex">콜라이더 인덱스</param>
        /// <param name="targetPawn">충돌한 대상</param>
        public void HandleColliderCollision(int colliderIndex, Pawn targetPawn)
        {
            if (colliderIndex < 0 || colliderIndex >= orbitingColliders.Count || !colliderActive[colliderIndex])
                return;

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
                
                Debug.Log($"<color=yellow>[AC107] 공전 콜라이더 {colliderIndex}가 {targetPawn.name}에게 충돌!</color>");
                
                // 충돌 후 처리 (해당 콜라이더와 VFX만 제거)
                if (collisionBehavior == CollisionBehavior.Destroy)
                {
                    colliderActive[colliderIndex] = false;
                    if (orbitingColliders[colliderIndex] != null)
                    {
                        DestroyImmediate(orbitingColliders[colliderIndex]);
                    }
                    
                    // 해당 VFX도 제거
                    if (colliderIndex < spawnedVFXs.Count && spawnedVFXs[colliderIndex] != null)
                    {
                        StopAndDestroyVFX(spawnedVFXs[colliderIndex]);
                        spawnedVFXs[colliderIndex] = null;
                    }
                    
                    Debug.Log($"<color=orange>[AC107] 콜라이더 {colliderIndex}와 VFX 제거!</color>");
                }
            }
        }

        /// <summary>
        /// 공전을 종료합니다. (매니저에서 자동으로 호출됨)
        /// </summary>
        public void EndOrbiting()
        {
            orbitState = OrbitState.Finishing;
            FinishOrbiting();
        }

        private void FinishOrbiting()
        {
            // VFX 정리 (AC100~105 패턴)
            foreach (var vfx in spawnedVFXs)
            {
                if (vfx != null)
                {
                    StopAndDestroyVFX(vfx);
                }
            }
            spawnedVFXs.Clear();
            
            // 콜라이더 정리
            foreach (var collider in orbitingColliders)
            {
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
            orbitingColliders.Clear();
            colliderActive.Clear();
            
            Debug.Log("<color=purple>[AC107] 공전 요소 종료!</color>");
        }

        /// <summary>
        /// 공전 VFX를 생성하고 설정합니다. (AC100~105 패턴)
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
            GameObject spawnedVFX = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            
            if (spawnedVFX != null)
            {
                spawnedVFX.transform.position = position;
                spawnedVFX.SetActive(true);
            }
            
            return spawnedVFX;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            foreach (var vfx in spawnedVFXs)
            {
                if (vfx != null)
                {
                    StopAndDestroyVFX(vfx);
                }
            }
        }
    }

    /// <summary>
    /// 공전 콜라이더의 충돌을 감지하는 컴포넌트
    /// </summary>
    public class OrbitingCollisionDetector : MonoBehaviour
    {
        private AC107_OrbitingElement parentComponent;
        private int colliderIndex;

        public void Initialize(AC107_OrbitingElement parent, int index)
        {
            parentComponent = parent;
            colliderIndex = index;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 충돌한 객체의 Pawn 컴포넌트 찾기
            Pawn hitPawn = other.GetComponent<Pawn>();
            if (hitPawn != null && parentComponent != null)
            {
                parentComponent.HandleColliderCollision(colliderIndex, hitPawn);
            }
        }
    }
} 