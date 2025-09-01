using AttackSystem;
using UnityEngine;
using CharacterSystem;
using Stats;
using BattleSystem;
using PrimeTween;

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
    /// 단일 공전 객체 컴포넌트
    /// AC108 매니저에 의해 관리되며, 자신의 VFX와 충돌만 담당합니다.
    /// </summary>
    public class AC107_OrbitingElement : AttackComponent
    {   
        // 공전 대상 설정
        [Header("공전 대상 설정")]
        public Pawn orbitOwner;

        public int orbitDamage = 10;

        // 콜라이더 설정
        [Header("콜라이더 설정")]
        public float colliderWidth = 0.5f; // 콜라이더 가로 크기
        public float colliderHeight = 0.5f; // 콜라이더 세로 크기
        public CollisionBehavior collisionBehavior = CollisionBehavior.Continue;

        // VFX 설정
        [Header("VFX 설정")]
        [SerializeField] public GameObject orbitVFXPrefab; // 공전 VFX 프리팹
         // 단일 VFX

        // 공격 관련
        private AttackData attackData;

        // 충돌 감지
        private BoxCollider2D boxCollider;
        private Rigidbody2D rb;
        private OrbitingCollisionDetector collisionDetector;
        
        // 궤도 각도 상태
        private float baseAngle = 0f;      // 고정 각도 (360/N * index)

        /// <summary>
        /// 공전 객체를 설정합니다. (AC108에서 호출)
        /// </summary>
        /// <param name="attackData">공격 데이터</param>
        /// <param name="orbitOwner">공격자 (Pawn 타입)</param>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        public void SetupOrbitingObject(AttackData attackData, Pawn orbitOwner, GameObject vfxPrefab)
        {
            this.attackData = attackData;
            this.orbitVFXPrefab = vfxPrefab;
            this.orbitOwner = orbitOwner;
            
            // 콜라이더 설정
            SetupCollider();
            
            // VFX 생성
            CreateVFX();
        }

        /// <summary>
        /// 콜라이더를 설정합니다.
        /// </summary>
        private void SetupCollider()
        {
            // BoxCollider2D 추가
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(colliderWidth, colliderHeight);
            boxCollider.isTrigger = true;
            
            // Rigidbody2D 추가 (충돌 감지용)
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            
            // 충돌 감지 컴포넌트 추가
            collisionDetector = gameObject.AddComponent<OrbitingCollisionDetector>();
            collisionDetector.Initialize(this);
        }

        /// <summary>
        /// VFX를 생성합니다.
        /// </summary>
        private void CreateVFX()
        {
            if (orbitVFXPrefab != null)
            {
                spawnedVFX = Instantiate(orbitVFXPrefab, transform.position, transform.rotation, transform);
                spawnedVFX.transform.localPosition = Vector3.zero;
                
                // VFX 재생
                var particleSystem = spawnedVFX.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
            }
        }

        /// <summary>
        /// 충돌을 처리합니다.
        /// </summary>
        /// <param name="targetPawn">충돌한 대상</param>
        public void HandleCollision(Pawn targetPawn)
        {
            if (targetPawn != null && targetPawn != orbitOwner)
            {
                // Attack 객체 생성 및 데미지 처리
                Attack attack = AttackFactory.Instance.Create(attackData, orbitOwner, null, Vector2.zero);
                attack.statSheet[StatType.AttackPower] = new IntegerStatValue(orbitDamage);
                DamageProcessor.ProcessHit(attack, targetPawn);

                // 충돌 후 처리
                if (collisionBehavior == CollisionBehavior.Destroy)
                {
                    // AC108 매니저에게 제거 요청
                    var manager = GetComponentInParent<AC108_OrbitingManager>();
                    if (manager != null)
                    {
                        manager.RemoveOrbitingObject(this.attack);
                    }
                    
                }
                
                // Attack 객체 정리
                AttackFactory.Instance.Deactivate(attack);
            }
        }

        /// <summary>
        /// 궤도 인덱스를 설정합니다.
        /// </summary>
        /// <param name="index">궤도에서의 인덱스</param>
        /// <param name="totalCount">전체 객체 수</param>
        public void SetOrbitIndex(int index, int totalCount)
        {
            // orbitIndex = index; // 사용되지 않는 변수
            baseAngle = (360f / totalCount) * index;
            // currentAngle = baseAngle; // 사용되지 않는 변수
        }
    }

    /// <summary>
    /// 공전 객체의 충돌을 감지하는 컴포넌트
    /// </summary>
    public class OrbitingCollisionDetector : MonoBehaviour
    {
        private AC107_OrbitingElement parentComponent;

        public void Initialize(AC107_OrbitingElement parent)
        {
            parentComponent = parent;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 충돌한 객체의 Pawn 컴포넌트 찾기
            Pawn hitPawn = other.GetComponent<Pawn>();
            if (hitPawn != null && parentComponent != null)
            {
                parentComponent.HandleCollision(hitPawn);
            }
        }
    }
} 