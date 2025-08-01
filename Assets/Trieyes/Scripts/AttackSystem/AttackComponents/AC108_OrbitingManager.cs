using AttackSystem;
using UnityEngine;
using System.Collections.Generic;
using CharacterSystem;
using PrimeTween;

namespace AttackComponents
{
    /// <summary>
    /// 공전 객체들을 관리하는 매니저 컴포넌트
    /// 한 캐릭터에 하나만 존재하며, PrimeTween을 사용하여 부드러운 궤도 보정을 처리합니다.
    /// </summary>
    public class AC108_OrbitingManager : AttackComponent
    {
        [Header("공전 설정")]
        public float orbitRadius = 2f;
        public float orbitSpeed = 90f;
        public OrbitDirection orbitDirection = OrbitDirection.Clockwise;
        
        [Header("DOTween 설정")]
        public float angleCorrectionDuration = 0.5f;
        public float positionMoveDuration = 0.3f;
        public float spawnScaleDuration = 0.2f;
        public float despawnScaleDuration = 0.2f;
        
        [Header("풀링 설정")]
        public int initialPoolSize = 10;
        public AttackData ac107AttackData; // AC107용 AttackData
        
        // Pawn 타입 변수 (RAC009, AC000 계열에서 전달받음)
        public Pawn orbitOwner;
        
        // 공전 객체 관리 (Attack 객체로 관리)
        private List<Attack> orbitingObjects = new List<Attack>();
        private Queue<Attack> objectPool = new Queue<Attack>();
        
        // 공전 상태
        private float currentBaseAngle = 0f;
        private Vector2 orbitCenter;
        private Transform orbitTarget;
        
        // PrimeTween 애니메이션 관리
        private Dictionary<Attack, Tween> activeTweens = new Dictionary<Attack, Tween>();
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            orbitOwner = attack.attacker;
            InitializeObjectPool();
        }
        
        protected override void Update()
        {
            UpdateOrbitCenter();
            UpdateBaseAngle();
            
            // 실시간 회전 위치 업데이트
            UpdateOrbitingObjectsPositions();
        }
        
        /// <summary>
        /// 모든 공전 객체의 위치를 실시간으로 업데이트합니다.
        /// </summary>
        private void UpdateOrbitingObjectsPositions()
        {
            if (orbitingObjects.Count == 0) return;
            
            for (int i = 0; i < orbitingObjects.Count; i++)
            {
                Attack obj = orbitingObjects[i];
                if (obj != null && obj.gameObject.activeInHierarchy)
                {
                    // PrimeTween으로 부드러운 실시간 회전
                    float baseAngle = (360f / orbitingObjects.Count) * i;
                    float realAngle = baseAngle + currentBaseAngle;
                    Vector2 newPosition = CalculateOrbitPosition(realAngle);
                    
                    // 실시간 위치 업데이트 (매우 짧은 지속시간으로 부드럽게)
                    Tween.Position(obj.transform, newPosition, 0.1f, Ease.InOutSine);
                }
            }
        }
        
        /// <summary>
        /// 공전 대상을 설정합니다.
        /// </summary>
        /// <param name="target">공전할 대상</param>
        public void SetOrbitTarget(Transform target)
        {
            orbitTarget = target;
            orbitCenter = target.position;
        }
        
        /// <summary>
        /// 공전 설정을 업데이트합니다.
        /// </summary>
        /// <param name="radius">공전 반지름</param>
        /// <param name="speed">공전 속도</param>
        /// <param name="direction">공전 방향</param>
        public void SetOrbitParameters(float radius, float speed, OrbitDirection direction)
        {
            orbitRadius = radius;
            orbitSpeed = speed;
            orbitDirection = direction;
        }
        
        /// <summary>
        /// 새로운 공전 객체를 추가합니다.
        /// </summary>
        /// <param name="attackData">AC107용 AttackData</param>
        /// <param name="attacker">공격자 (Pawn 타입)</param>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <returns>생성된 AC107 Attack 객체</returns>
        public Attack AddOrbitingObject(AttackData attackData, Pawn attacker, GameObject vfxPrefab)
        {
            // 풀에서 객체 가져오기
            Attack orbitingAttack = GetOrbitingObjectFromPool();
            
            if (orbitingAttack != null)
            {
                // Pawn 타입 변수 설정
                orbitOwner = attacker;
                
                // AC107 컴포넌트 가져오기
                AC107_OrbitingElement orbitingComponent = orbitingAttack.components[0] as AC107_OrbitingElement;
                if (orbitingComponent != null)
                {
                    // 객체 설정
                    orbitingComponent.SetupOrbitingObject(attackData, attacker, vfxPrefab);
                }
                
                // 리스트에 추가
                orbitingObjects.Add(orbitingAttack);
                
                // 객체를 즉시 올바른 궤도 위치에 배치
                PlaceObjectAtCorrectOrbitPosition(orbitingAttack, orbitingObjects.Count - 1);
                
                // 등장 애니메이션
                PlaySpawnAnimation(orbitingAttack);
                
                Debug.Log($"[AC108] 공전 객체 추가 완료! (현재 {orbitingObjects.Count}개, 소유자: {attacker.name})");
            }
            
            return orbitingAttack;
        }
        
        /// <summary>
        /// 객체를 올바른 궤도 위치에 배치합니다.
        /// </summary>
        /// <param name="orbitingAttack">배치할 Attack 객체</param>
        /// <param name="index">궤도에서의 인덱스</param>
        private void PlaceObjectAtCorrectOrbitPosition(Attack orbitingAttack, int index)
        {
            if (orbitingObjects.Count == 0) return;
            
            float angleStep = 360f / orbitingObjects.Count;
            float targetAngle = angleStep * index;
            Vector2 targetPosition = CalculateOrbitPosition(targetAngle);
            
            // owner(플레이어) 위치에서 시작
            orbitingAttack.transform.position = orbitOwner.transform.position;
            
            // AC107의 궤도 인덱스 설정
            AC107_OrbitingElement orbitingComponent = orbitingAttack.GetComponent<AC107_OrbitingElement>();
            if (orbitingComponent != null)
            {
                orbitingComponent.SetOrbitIndex(index, orbitingObjects.Count);
            }
            
            // owner 위치에서 플레이어 주변 궤도로 Tween 이동
            Tween moveTween = Tween.Position(orbitingAttack.transform, targetPosition, positionMoveDuration, Ease.OutQuad);
            activeTweens[orbitingAttack] = moveTween;
            
            Debug.Log($"[AC108] AC107 {index}번째 객체를 owner에서 각도 {targetAngle}°로 이동");
        }
        
        /// <summary>
        /// 공전 객체를 제거합니다.
        /// </summary>
        /// <param name="orbitingAttack">제거할 Attack 객체</param>
        public void RemoveOrbitingObject(Attack orbitingAttack)
        {
            if (orbitingAttack != null && orbitingObjects.Contains(orbitingAttack))
            {
                // 사라짐 애니메이션
                PlayDespawnAnimation(orbitingAttack, () => {
                    // 리스트에서 제거
                    orbitingObjects.Remove(orbitingAttack);
                    
                    // 풀로 반환
                    ReturnToPool(orbitingAttack);
                    
                    // 나머지 객체들을 올바른 궤도 위치로 재배치
                    RecalculateOrbitPositionsWithPrimeTween();
                    
                    Debug.Log($"[AC108] 공전 객체 제거 완료! (현재 {orbitingObjects.Count}개)");
                });
            }
        }
        
        /// <summary>
        /// 모든 공전 객체를 제거합니다.
        /// </summary>
        public void ClearAllOrbitingObjects()
        {
            foreach (var obj in orbitingObjects.ToArray())
            {
                RemoveOrbitingObject(obj);
            }
        }
        
        /// <summary>
        /// 공전 중심을 업데이트합니다.
        /// </summary>
        private void UpdateOrbitCenter()
        {
            if (orbitTarget != null)
            {
                orbitCenter = orbitTarget.position;
            }
        }
        
        /// <summary>
        /// 기본 공전 각도를 업데이트합니다.
        /// </summary>
        private void UpdateBaseAngle()
        {
            float directionMultiplier = (orbitDirection == OrbitDirection.Clockwise) ? 1f : -1f;
            currentBaseAngle += orbitSpeed * directionMultiplier * Time.deltaTime;
            currentBaseAngle %= 360f;
        }
        
        /// <summary>
        /// PrimeTween을 사용하여 궤도 위치를 재계산하고 애니메이션을 적용합니다.
        /// 모든 AC107이 동일한 각도 간격으로 배치됩니다.
        /// </summary>
        private void RecalculateOrbitPositionsWithPrimeTween()
        {
            if (orbitingObjects.Count == 0) return;
            
            float angleStep = 360f / orbitingObjects.Count;
            
            for (int i = 0; i < orbitingObjects.Count; i++)
            {
                Attack obj = orbitingObjects[i];
                if (obj != null && obj.gameObject.activeInHierarchy)
                {
                    // AC107 컴포넌트 가져오기
                    AC107_OrbitingElement orbitingComponent = obj.GetComponent<AC107_OrbitingElement>();
                    if (orbitingComponent != null)
                    {
                        // AC107의 궤도 인덱스 설정
                        orbitingComponent.SetOrbitIndex(i, orbitingObjects.Count);
                    }
                    
                    // PrimeTween으로 부드러운 위치 이동 (모든 각도가 동일하게)
                    float targetAngle = angleStep * i;
                    Vector2 targetPosition = CalculateOrbitPosition(targetAngle);
                    
                    // 기존 애니메이션 정리
                    if (activeTweens.ContainsKey(obj) && activeTweens[obj].isAlive)
                    {
                        activeTweens[obj].Stop();
                    }
                    
                    // 새로운 위치로 부드럽게 이동
                    Tween moveTween = Tween.Position(obj.transform, targetPosition, positionMoveDuration, Ease.OutQuad);
                    activeTweens[obj] = moveTween;
                    
                    Debug.Log($"[AC108] AC107 {i}번째 객체를 각도 {targetAngle}°로 재배치");
                }
            }
        }
        
        /// <summary>
        /// 궤도 위치를 계산합니다.
        /// </summary>
        /// <param name="angle">각도 (도)</param>
        /// <returns>계산된 위치</returns>
        private Vector2 CalculateOrbitPosition(float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            float x = orbitCenter.x + (orbitRadius * Mathf.Cos(radian));
            float y = orbitCenter.y + (orbitRadius * Mathf.Sin(radian));
            
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// 등장 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="orbitingAttack">애니메이션을 적용할 Attack 객체</param>
        private void PlaySpawnAnimation(Attack orbitingAttack)
        {
            // PrimeTween으로 등장 애니메이션
            orbitingAttack.transform.localScale = Vector3.zero;
            
            Tween scaleTween = Tween.Scale(orbitingAttack.transform, Vector3.one, spawnScaleDuration, Ease.OutBack);
            activeTweens[orbitingAttack] = scaleTween;
        }
        
        /// <summary>
        /// 사라짐 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="orbitingAttack">애니메이션을 적용할 Attack 객체</param>
        /// <param name="onComplete">완료 콜백</param>
        private void PlayDespawnAnimation(Attack orbitingAttack, System.Action onComplete)
        {
            // PrimeTween으로 사라짐 애니메이션
            Tween scaleTween = Tween.Scale(orbitingAttack.transform, Vector3.zero, despawnScaleDuration, Ease.InBack);
            scaleTween.OnComplete(() => onComplete?.Invoke());
            activeTweens[orbitingAttack] = scaleTween;
        }
        
        /// <summary>
        /// 객체 풀을 초기화합니다.
        /// </summary>
        private void InitializeObjectPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateOrbitingObjectForPool();
            }
            
            Debug.Log($"[AC108] 객체 풀 초기화 완료! (크기: {initialPoolSize})");
        }
        
        /// <summary>
        /// 풀용 공전 객체를 생성합니다. (AttackFactory 사용)
        /// </summary>
        private void CreateOrbitingObjectForPool()
        {
            if (ac107AttackData == null)
            {
                Debug.LogError("[AC108] AC107 AttackData가 설정되지 않았습니다!");
                return;
            }
            
            // AttackFactory를 통해 AC107 Attack 객체 생성
            Attack orbitingAttack = AttackFactory.Instance.Create(
                ac107AttackData,
                orbitOwner, // AC108의 attacker로 설정
                null, // parent는 null
                Vector2.zero
            );
            
            if (orbitingAttack != null)
            {
                // 부모 설정
                orbitingAttack.transform.SetParent(transform);
                orbitingAttack.gameObject.SetActive(false);
                
                objectPool.Enqueue(orbitingAttack);
                Debug.Log($"[AC108] AC107 Attack 객체 풀 생성 완료!");
            }
            else
            {
                Debug.LogError($"[AC108] AC107 Attack 객체 생성 실패!");
            }
        }
        
        /// <summary>
        /// 풀에서 공전 객체를 가져옵니다.
        /// </summary>
        /// <returns>공전 Attack 객체</returns>
        private Attack GetOrbitingObjectFromPool()
        {
            if (objectPool.Count == 0)
            {
                CreateOrbitingObjectForPool();
            }
            
            Attack obj = objectPool.Dequeue();
            obj.gameObject.SetActive(true);
            
            return obj;
        }
        
        /// <summary>
        /// 객체를 풀로 반환합니다.
        /// </summary>
        /// <param name="orbitingAttack">반환할 Attack 객체</param>
        private void ReturnToPool(Attack orbitingAttack)
        {
            if (orbitingAttack != null)
            {
                orbitingAttack.gameObject.SetActive(false);
                orbitingAttack.transform.localScale = Vector3.one;
                orbitingAttack.transform.position = Vector3.zero;
                orbitingAttack.transform.rotation = Quaternion.identity;
                
                objectPool.Enqueue(orbitingAttack);
            }
        }
        
        private void OnDestroy()
        {
            // PrimeTween 정리 작업
            foreach (var tween in activeTweens.Values)
            {
                if (tween.isAlive)
                {
                    tween.Stop();
                }
            }
            activeTweens.Clear();
        }
    }
} 