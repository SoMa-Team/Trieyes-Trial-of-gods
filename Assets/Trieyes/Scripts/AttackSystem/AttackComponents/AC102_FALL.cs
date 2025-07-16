using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using CharacterSystem.Enemies;

namespace AttackComponents
{
    /// <summary>
    /// 위에서 아래로 떨어지는 범위 공격 컴포넌트
    /// 지정된 위치에 범위 공격을 수행하며, GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    public class AC102_FALL : AttackComponent
    {
        [Header("낙하 공격 설정")]
        public float fallDamage = 50f;
        public float fallRadius = 2f; // 공격 범위
        public float fallDuration = 0.5f; // 공격 지속 시간
        public float fallDelay = 0.1f; // 낙하 시작까지의 지연 시간
        
        [Header("VFX 설정")]
        public GameObject fallVFXPrefab;
        public float vfxDuration = 0.3f;

        // 낙하 공격 상태 관리
        private FallAttackState fallState = FallAttackState.None;
        private float fallTimer = 0f;
        private Vector2 targetPosition;
        private List<Pawn> hitTargets; // 재사용 가능한 리스트

        // 낙하 공격 상태 열거형
        private enum FallAttackState
        {
            None,
            Preparing,
            Falling,
            Impact,
            Finished
        }

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(20);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fallState = FallAttackState.None;
            fallTimer = 0f;
            hitTargets.Clear();
            
            // 타겟 위치 설정 (공격자의 앞쪽)
            targetPosition = (Vector2)attacker.transform.position + direction * 3f;
            
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
                    
                    if (fallTimer >= fallDuration)
                    {
                        fallState = FallAttackState.Impact;
                        fallTimer = 0f;
                        
                        // 충격 효과 및 데미지 적용
                        ApplyImpactDamage();
                    }
                    break;

                case FallAttackState.Impact:
                    // 충격 효과 처리
                    fallTimer += Time.deltaTime;
                    
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
            // 낙하 VFX 생성
            CreateFallVFX();
            
            // 범위 내 적 탐지 및 데미지 적용 준비
            DetectTargetsInRange();
        }

        private void DetectTargetsInRange()
        {
            // 재사용 가능한 리스트 초기화
            reusableColliders.Clear();
            hitTargets.Clear();
            
            // 범위 내 모든 콜라이더 탐지
            Physics2D.OverlapCircle(targetPosition, fallRadius, new ContactFilter2D().NoFilter(), reusableColliders);
            
            // 적만 필터링하여 수집
            for (int i = 0; i < reusableColliders.Count; i++)
            {
                Collider2D collider = reusableColliders[i];
                if (collider == null) continue;

                Pawn enemy = collider.GetComponent<Pawn>();
                if (enemy != null && enemy.GetComponent<Controller>() is EnemyController)
                {
                    hitTargets.Add(enemy);
                }
            }
        }

        private void ApplyImpactDamage()
        {
            // 충격 VFX 생성
            CreateImpactVFX();

            // 탐지된 모든 적에게 데미지 적용
            for (int i = 0; i < hitTargets.Count; i++)
            {
                Pawn target = hitTargets[i];
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    // 데미지 적용
                    ApplyDamageToTarget(target);
                }
            }
        }

        private void ApplyDamageToTarget(Pawn target)
        {
            // AttackResult 생성 및 데미지 처리
            var attackResult = AttackResult.Create(attack, target);
            
            // 기본 데미지 설정
            attackResult.totalDamage = (int)fallDamage;
            
            // DamageProcessor를 통한 데미지 처리
            DamageProcessor.ProcessHit(attack, target);
            
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
            
            // 상태 초기화
            fallState = FallAttackState.None;
            fallTimer = 0f;
            hitTargets.Clear();
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