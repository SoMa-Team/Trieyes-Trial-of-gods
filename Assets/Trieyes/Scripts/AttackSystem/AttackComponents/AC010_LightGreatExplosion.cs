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
    /// 빛의 대폭발 효과
    /// 지정된 범위에 강력한 빛의 폭발을 생성하여 적들에게 대량의 데미지를 입힙니다.
    /// GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    public class AC010_LightGreatExplosion : AttackComponent
    {
        [Header("빛의 대폭발 설정")]
        public float explosionDamage = 80f;
        public float explosionRadius = 4f;
        public float explosionDuration = 1.5f;
        public float explosionDelay = 0.3f;
        public float chargeTime = 0.5f; // 충전 시간

        [Header("VFX 설정")]
        public GameObject explosionVFXPrefab;
        public float vfxDuration = 0.6f;

        // 폭발 상태 관리
        private ExplosionState explosionState = ExplosionState.None;
        private float explosionTimer = 0f;
        private float chargeTimer = 0f;
        private Vector2 targetPosition;
        private List<Pawn> hitTargets = new List<Pawn>(15);

        // 폭발 상태 열거형
        private enum ExplosionState
        {
            None,
            Charging,
            Preparing,
            Exploding,
            Impact,
            Finished
        }

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(30);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            explosionState = ExplosionState.None;
            explosionTimer = 0f;
            chargeTimer = 0f;
            hitTargets.Clear();
            
            // 타겟 위치 설정
            targetPosition = (Vector2)attacker.transform.position + direction * 5f;
            
            // 폭발 시작
            StartExplosion();
        }

        private void StartExplosion()
        {
            explosionState = ExplosionState.Charging;
            chargeTimer = 0f;
            
            // 충전 VFX 생성
            CreateChargeVFX();
        }

        protected override void Update()
        {
            base.Update();
            
            // 폭발 처리
            ProcessExplosion();
        }

        private void ProcessExplosion()
        {
            switch (explosionState)
            {
                case ExplosionState.None:
                    break;

                case ExplosionState.Charging:
                    chargeTimer += Time.deltaTime;
                    
                    if (chargeTimer >= chargeTime)
                    {
                        explosionState = ExplosionState.Preparing;
                        explosionTimer = 0f;
                        StartPreparing();
                    }
                    break;

                case ExplosionState.Preparing:
                    explosionTimer += Time.deltaTime;
                    
                    if (explosionTimer >= explosionDelay)
                    {
                        explosionState = ExplosionState.Exploding;
                        explosionTimer = 0f;
                        StartExploding();
                    }
                    break;

                case ExplosionState.Exploding:
                    explosionTimer += Time.deltaTime;
                    
                    if (explosionTimer >= explosionDuration)
                    {
                        explosionState = ExplosionState.Impact;
                        explosionTimer = 0f;
                        ApplyExplosionDamage();
                    }
                    break;

                case ExplosionState.Impact:
                    explosionTimer += Time.deltaTime;
                    
                    if (explosionTimer >= vfxDuration)
                    {
                        explosionState = ExplosionState.Finished;
                    }
                    break;

                case ExplosionState.Finished:
                    explosionState = ExplosionState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void StartPreparing()
        {
            // 준비 VFX 생성
            CreatePreparationVFX();
        }

        private void StartExploding()
        {
            // 폭발 VFX 생성
            CreateExplosionVFX();
            
            // 범위 내 적 탐지
            DetectTargetsInRange();
        }

        private void DetectTargetsInRange()
        {
            reusableColliders.Clear();
            hitTargets.Clear();
            
            Physics2D.OverlapCircle(targetPosition, explosionRadius, new ContactFilter2D().NoFilter(), reusableColliders);
            
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

        private void ApplyExplosionDamage()
        {
            for (int i = 0; i < hitTargets.Count; i++)
            {
                Pawn target = hitTargets[i];
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    ApplyDamageToTarget(target);
                }
            }
            
            CreateImpactVFX();
        }

        private void ApplyDamageToTarget(Pawn target)
        {
            var attackResult = AttackResult.Create(attack, target);
            attackResult.totalDamage = (int)explosionDamage;
            DamageProcessor.ProcessHit(attack, target);
        }

        private void CreateChargeVFX()
        {
            // 충전 VFX 생성 로직
        }

        private void CreatePreparationVFX()
        {
            // 준비 VFX 생성 로직
        }

        private void CreateExplosionVFX()
        {
            // 폭발 VFX 생성 로직
        }

        private void CreateImpactVFX()
        {
            // 충격 VFX 생성 로직
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            explosionState = ExplosionState.None;
            explosionTimer = 0f;
            chargeTimer = 0f;
            hitTargets.Clear();
        }
    }
} 