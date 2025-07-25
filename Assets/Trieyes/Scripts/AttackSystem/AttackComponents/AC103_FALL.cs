using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using CharacterSystem.Enemies;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 위에서 아래로 떨어지는 범위 공격 컴포넌트
    /// 지정된 위치에 범위 공격을 수행하며, GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    public class AC103_FALL : AttackComponent
    {
        [Header("낙하 공격 설정")]

        private Vector2 targetPosition;
        public Vector2 fallXYOffset;

        public int fallXRandomOffsetMin;
        public int fallXRandomOffsetMax;
        public int fallYRandomOffsetMin;
        public int fallYRandomOffsetMax;
        public int fallDamage = 100;
        public float fallRadius = 2f; // 공격 범위
        public float fallDuration = 0.5f; // 공격 지속 시간
        public float fallDelay = 0.1f; // 낙하 시작까지의 지연 시간
        
        [Header("VFX 설정")]
        public GameObject fallVFXPrefab;
        public float vfxDuration = 0.3f;
        
        // VFX 시스템 설정
        [Header("VFX System Settings")]
        [SerializeField] private readonly int FALLING_VFX_ID = 3; // 떨어지는 파이어볼 VFX ID
        [SerializeField] private readonly int EXPLOSION_VFX_ID = 4; // 폭발 VFX ID
        private GameObject fallingVFX;
        private GameObject explosionVFX;

        // 낙하 공격 상태 관리
        private FallAttackState fallState = FallAttackState.None;
        private float fallTimer = 0f;
        private bool explosionVFXCreated = false; // 폭발 VFX 생성 여부
        private List<Enemy> hitTargets = new List<Enemy>(); // 재사용 가능한 리스트
        
        // VFX 이동 관련
        private Vector2 fallingVFXStartPosition; // 떨어지는 VFX 시작 위치
        private Vector2 fallingVFXTargetPosition; // 떨어지는 VFX 목표 위치
        private float fallingVFXMoveSpeed = 8f; // 떨어지는 VFX 이동 속도

        // 낙하 공격 상태 열거형
        private enum FallAttackState
        {
            None,
            Preparing,
            Falling,
            Impact,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fallState = FallAttackState.Preparing;
            fallTimer = 0f;
            explosionVFXCreated = false;
            hitTargets.Clear();
            
            // 타겟 위치 설정 (공격자의 앞쪽)
            // fallXYOffset가 0이면 랜덤으로 아니면 그대로 사용
            if (fallXYOffset == Vector2.zero)
            {
                targetPosition = (Vector2)attacker.transform.position + 
                new Vector2(Random.Range(fallXRandomOffsetMin, fallXRandomOffsetMax), 
                Random.Range(fallYRandomOffsetMin, fallYRandomOffsetMax));
            }
            else
            {
                targetPosition = (Vector2)attacker.transform.position + fallXYOffset;
            }

            // 낙하 공격 시작
            StartFallAttack();
        }

        private void StartFallAttack()
        {
            fallState = FallAttackState.Preparing;
            fallTimer = 0f;
            
            // VFX 생성 (낙하 예고)
            CreateFallWarningVFX();
        }

        protected override void Update()
        {
            base.Update();
            
            // 낙하 공격 처리
            ProcessFallAttack();
        }

        private void ProcessFallAttack()
        {
            switch (fallState)
            {
                case FallAttackState.None:
                    // 아무것도 하지 않음
                    break;

                case FallAttackState.Preparing:
                    // 낙하 준비 단계
                    fallTimer += Time.deltaTime;
                    
                    if (fallTimer >= fallDelay)
                    {
                        fallState = FallAttackState.Falling;
                        fallTimer = 0f;
                        
                        // 실제 낙하 시작
                        StartFalling();
                    }
                    break;

                case FallAttackState.Falling:
                    // 낙하 중
                    fallTimer += Time.deltaTime;
                    
                    // 떨어지는 VFX 이동
                    if (fallingVFX != null)
                    {
                        Vector2 currentPosition = fallingVFX.transform.position;
                        Vector2 newPosition = Vector2.MoveTowards(currentPosition, fallingVFXTargetPosition, fallingVFXMoveSpeed * Time.deltaTime);
                        fallingVFX.transform.position = newPosition;
                        
                        Debug.Log($"<color=blue>[FALL] VFX 이동: {currentPosition} -> {newPosition}, 목표: {fallingVFXTargetPosition}</color>");
                        
                        // 목표 지점에 도달했는지 확인
                        if (Vector2.Distance(newPosition, fallingVFXTargetPosition) < 0.1f)
                        {
                            Debug.Log("<color=green>[FALL] VFX가 목표 지점에 도달!</color>");
                            fallState = FallAttackState.Impact;
                            fallTimer = 0f;
                            
                            // 충격 효과 및 데미지 적용
                            ApplyImpactDamage();
                        }
                    }
                    else
                    {
                        // VFX가 없으면 타이머로 처리
                        if (fallTimer >= fallDuration)
                        {
                            fallState = FallAttackState.Impact;
                            fallTimer = 0f;
                            
                            // 충격 효과 및 데미지 적용
                            ApplyImpactDamage();
                        }
                    }
                    break;

                case FallAttackState.Impact:
                    // 충격 효과 처리
                    fallTimer += Time.deltaTime;
                    
                    // 폭발 VFX 즉시 생성 (착탄 지점의 남쪽 모서리에 생성)
                    if (!explosionVFXCreated)
                    {
                        Vector2 explosionPosition = targetPosition;
                        
                        // 착탄 지점의 남쪽 모서리 위치 계산
                        // fallRadius는 공격 범위이므로, 그 반지름만큼 아래로 이동
                        explosionPosition = targetPosition + Vector2.down * (fallRadius - 1.3f);
                        
                        explosionVFX = CreateAndSetupExplosionVFX(explosionPosition, Vector2.zero);
                        PlayVFX(explosionVFX);
                        explosionVFXCreated = true;
                        Debug.Log($"<color=green>[FALL] 폭발 VFX 생성! 위치: {explosionPosition} (착탄 지점의 남쪽 모서리)</color>");
                    }
                    
                    if (fallTimer >= vfxDuration)
                    {
                        fallState = FallAttackState.Finished;
                    }
                    break;

                case FallAttackState.Finished:
                    // 낙하 공격 완료
                    fallState = FallAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void StartFalling()
        {
            // 떨어지는 파이어볼 VFX 생성 (VFX ID 3)
            fallingVFXStartPosition = targetPosition + Vector2.up * 2f;
            fallingVFXTargetPosition = targetPosition;
            fallingVFX = CreateAndSetupFallingVFX(fallingVFXStartPosition, Vector2.down);
            PlayVFX(fallingVFX);
        
            // 착탄 지점에 Radius 반경의 원형 Draw 만들기
            // 원형 필드 디버그 (원의 둘레를 그리기)
            int segments = 24;
            Vector2 prevPoint = targetPosition + Vector2.right * (fallRadius-1f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (360f / segments) * i * Mathf.Deg2Rad;
                Vector2 currentPoint = targetPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (fallRadius-1f);
                Debug.DrawLine(prevPoint, currentPoint, Color.red, 1f);
                prevPoint = currentPoint;
            }
        }

        private void ApplyImpactDamage()
        {
            // Impact 순간에 범위 내 적들을 다시 탐지
            DetectTargetsInRange();
            
            // 탐지된 모든 적에게 데미지 적용
            for (int i = hitTargets.Count - 1; i >= 0; i--)
            {
                Enemy target = hitTargets[i];
                if (target != null && target.transform != null)
                {
                    // 데미지 적용
                    ApplyDamageToTarget(target);
                }
                else
                {
                    // 파괴된 객체 제거
                    hitTargets.RemoveAt(i);
                }
            }
        }
        
        private void DetectTargetsInRange()
        {
            hitTargets.Clear();
            
            // Impact 순간의 범위 내 적들을 가져오기
            var enemiesInRange = BattleStage.now.GetEnemiesInCircleRangeOrderByDistance(targetPosition, fallRadius, 10);
            
            // 유효한 적들만 필터링하여 수집
            foreach (var enemy in enemiesInRange)
            {
                if (enemy != null && enemy.transform != null)
                {
                    hitTargets.Add(enemy);
                }
            }
            
            Debug.Log($"<color=blue>[FALL_ATTACK] Impact 순간 범위 내 적 탐지: {hitTargets.Count}명</color>");
        }

        private void ApplyDamageToTarget(Enemy target)
        {
            // AttackResult 생성 및 데미지 처리
            var attackResult = AttackResult.Create(attack, target);
            
            // 기본 데미지 설정
            attackResult.attacker = attack.attacker;
            attackResult.target = target;
            attackResult.isCritical = false;
            attackResult.isEvaded = false;
            attackResult.totalDamage = fallDamage;

            target.ApplyDamage(attackResult);

            Debug.Log($"<color=red>[FALL_ATTACK] {target.pawnName}에게 {attackResult.totalDamage} 데미지 적용</color>");
        }

        private void CreateFallWarningVFX()
        {
            if (fallVFXPrefab != null)
            {
                GameObject warningVFX = Instantiate(fallVFXPrefab);
                warningVFX.transform.position = targetPosition;
                
                // 경고 VFX 설정 (예: 빨간 원형 마커)
                FallWarningVFX warningComponent = warningVFX.GetComponent<FallWarningVFX>();
                if (warningComponent != null)
                {
                    warningComponent.Initialize(fallDelay, fallRadius);
                }
            }
        }

        private void CreateFallVFX()
        {
            if (fallVFXPrefab != null)
            {
                GameObject fallVFX = Instantiate(fallVFXPrefab);
                fallVFX.transform.position = targetPosition;
                
                // 낙하 VFX 설정
                FallVFX fallComponent = fallVFX.GetComponent<FallVFX>();
                if (fallComponent != null)
                {
                    fallComponent.Initialize(fallDuration, fallRadius);
                }
            }
        }

        private void CreateImpactVFX()
        {
            if (fallVFXPrefab != null)
            {
                GameObject impactVFX = Instantiate(fallVFXPrefab);
                impactVFX.transform.position = targetPosition;
                
                // 충격 VFX 설정
                ImpactVFX impactComponent = impactVFX.GetComponent<ImpactVFX>();
                if (impactComponent != null)
                {
                    impactComponent.Initialize(vfxDuration, fallRadius);
                }
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // VFX 정리
            if (fallingVFX != null)
            {
                StopAndReturnVFX(fallingVFX, FALLING_VFX_ID);
                fallingVFX = null;
            }
            
            if (explosionVFX != null)
            {
                StopAndReturnVFX(explosionVFX, EXPLOSION_VFX_ID);
                explosionVFX = null;
            }
            
            // 상태 초기화
            fallState = FallAttackState.None;
            fallTimer = 0f;
            explosionVFXCreated = false;
            hitTargets.Clear();
        }

        /// <summary>
        /// 떨어지는 파이어볼 VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        private GameObject CreateAndSetupFallingVFX(Vector2 position, Vector2 direction)
        {
            // VFXFactory를 통해 떨어지는 파이어볼 VFX 생성
            GameObject vfx = VFXFactory.Instance.SpawnVFX(FALLING_VFX_ID, position, direction);
            
            // 여기서 VFX의 세부 조정을 할 수 있습니다
            ParticleSystem childVFX = vfx.transform.GetChild(0).GetComponent<ParticleSystem>();

            // Render Alignment 설정
            ParticleSystemRenderer renderer = childVFX.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.alignment = ParticleSystemRenderSpace.Local;
            }

            vfx.transform.position = position;
            
            // radius에 연동된 scaling 계산 (1.5배가 기본 크기)
            float baseScale = 1.5f;
            float finalScale = baseScale * fallRadius * 1.5f;
            vfx.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
            
            // 파이어볼 색상 설정 (빨간색 계열)
            var colorOverLifetime = childVFX.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), new GradientColorKey(Color.yellow, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.65f, 0.5f), new GradientAlphaKey(0.3f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            return vfx;
        }

        /// <summary>
        /// 폭발 VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        private GameObject CreateAndSetupExplosionVFX(Vector2 position, Vector2 direction)
        {
            // VFXFactory를 통해 폭발 VFX 생성
            GameObject vfx = VFXFactory.Instance.SpawnVFX(EXPLOSION_VFX_ID, position, direction);

            // 여기서 VFX의 세부 조정을 할 수 있습니다
            ParticleSystem childVFX = vfx.transform.GetChild(0).GetComponent<ParticleSystem>();
            
            // Render Alignment 설정
            ParticleSystemRenderer renderer = childVFX.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.alignment = ParticleSystemRenderSpace.Local;
                renderer.sortingOrder = 100;
            }

            vfx.transform.position = position;
            
            // radius에 연동된 scaling 계산 (1.5배가 기본 크기)
            float baseScale = 1.5f;
            float finalScale = baseScale * fallRadius * 1.25f;
            vfx.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
            
            // 폭발 색상 설정 (오렌지-노란색 계열)
            var colorOverLifetime = childVFX.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.3f), new GradientColorKey(Color.red, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.65f, 0.5f), new GradientAlphaKey(0.3f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            return vfx;
        }
    }

    /// <summary>
    /// 낙하 경고 VFX 컴포넌트
    /// </summary>
    public class FallWarningVFX : MonoBehaviour
    {
        public float warningDuration = 0.5f;
        public float warningRadius = 2f;
        public Color warningColor = Color.red;
        
        private float currentDuration = 0f;
        private SpriteRenderer spriteRenderer;
        
        public void Initialize(float duration, float radius)
        {
            warningDuration = duration;
            warningRadius = radius;
            currentDuration = 0f;
            
            // 스프라이트 렌더러 설정
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // 경고 표시 설정
            transform.localScale = Vector3.one * warningRadius * 2f;
            spriteRenderer.color = warningColor;
            spriteRenderer.sortingOrder = 10;
        }
        
        private void Update()
        {
            currentDuration += Time.deltaTime;
            
            // 깜빡이는 효과
            float alpha = Mathf.PingPong(currentDuration * 4f, 1f);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            
            if (currentDuration >= warningDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 낙하 VFX 컴포넌트
    /// </summary>
    public class FallVFX : MonoBehaviour
    {
        public float fallDuration = 0.5f;
        public float fallRadius = 2f;
        public Color fallColor = Color.yellow;
        
        private float currentDuration = 0f;
        private SpriteRenderer spriteRenderer;
        
        public void Initialize(float duration, float radius)
        {
            fallDuration = duration;
            fallRadius = radius;
            currentDuration = 0f;
            
            // 스프라이트 렌더러 설정
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // 낙하 효과 설정
            transform.localScale = Vector3.one * fallRadius * 2f;
            spriteRenderer.color = fallColor;
            spriteRenderer.sortingOrder = 10;
        }
        
        private void Update()
        {
            currentDuration += Time.deltaTime;
            
            // 낙하 애니메이션
            float progress = currentDuration / fallDuration;
            transform.localScale = Vector3.Lerp(Vector3.one * fallRadius * 2f, Vector3.one * fallRadius * 0.5f, progress);
            
            if (currentDuration >= fallDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 충격 VFX 컴포넌트
    /// </summary>
    public class ImpactVFX : MonoBehaviour
    {
        public float impactDuration = 0.3f;
        public float impactRadius = 2f;
        public Color impactColor = Color.red;
        
        private float currentDuration = 0f;
        private SpriteRenderer spriteRenderer;
        
        public void Initialize(float duration, float radius)
        {
            impactDuration = duration;
            impactRadius = radius;
            currentDuration = 0f;
            
            // 스프라이트 렌더러 설정
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // 충격 효과 설정
            transform.localScale = Vector3.one * impactRadius * 2f;
            spriteRenderer.color = impactColor;
            spriteRenderer.sortingOrder = 10;
        }
        
        private void Update()
        {
            currentDuration += Time.deltaTime;
            
            // 충격 파동 애니메이션
            float progress = currentDuration / impactDuration;
            transform.localScale = Vector3.Lerp(Vector3.one * impactRadius * 0.5f, Vector3.one * impactRadius * 3f, progress);
            
            // 투명도 감소
            Color color = spriteRenderer.color;
            color.a = 1f - progress;
            spriteRenderer.color = color;
            
            if (currentDuration >= impactDuration)
            {
                Destroy(gameObject);
            }
        }
    }
} 