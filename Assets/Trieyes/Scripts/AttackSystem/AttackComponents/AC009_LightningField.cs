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
    /// 번개 장판 효과
    /// 지정된 범위에 번개 장판을 생성하여 적들에게 지속 데미지를 입힙니다.
    /// GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    public class AC009_LightningField : AttackComponent
    {
        [Header("번개 장판 설정")]
        public float lightningFieldDamage = 30f;
        public float lightningFieldRadius = 2.5f;
        public float lightningFieldDuration = 3f;
        public float lightningFieldDelay = 0.1f;
        public float lightningTickInterval = 0.5f; // 번개 틱 간격

        [Header("VFX 설정")]
        public GameObject lightningFieldVFXPrefab;
        public float vfxDuration = 0.4f;

        // 번개 장판 상태 관리
        private LightningFieldState fieldState = LightningFieldState.None;
        private float fieldTimer = 0f;
        private float tickTimer = 0f;
        private Vector2 targetPosition;
        private List<Pawn> hitTargets = new List<Pawn>(10);

        // 번개 장판 상태 열거형
        private enum LightningFieldState
        {
            None,
            Preparing,
            Active,
            Deactivating,
            Finished
        }

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(20);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
            tickTimer = 0f;
            hitTargets.Clear();
            
            // 타겟 위치 설정
            targetPosition = (Vector2)attacker.transform.position + direction * 3.5f;
            
            // 번개 장판 시작
            StartLightningField();
        }

        private void StartLightningField()
        {
            fieldState = LightningFieldState.Preparing;
            fieldTimer = 0f;
            
            // VFX 생성
            CreateLightningFieldVFX();
        }

        protected override void Update()
        {
            base.Update();
            
            // 번개 장판 처리
            ProcessLightningField();
        }

        private void ProcessLightningField()
        {
            switch (fieldState)
            {
                case LightningFieldState.None:
                    break;

                case LightningFieldState.Preparing:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= lightningFieldDelay)
                    {
                        fieldState = LightningFieldState.Active;
                        fieldTimer = 0f;
                        ActivateField();
                    }
                    break;

                case LightningFieldState.Active:
                    fieldTimer += Time.deltaTime;
                    tickTimer += Time.deltaTime;
                    
                    // 번개 틱 처리
                    if (tickTimer >= lightningTickInterval)
                    {
                        ApplyLightningTick();
                        tickTimer = 0f;
                    }
                    
                    if (fieldTimer >= lightningFieldDuration)
                    {
                        fieldState = LightningFieldState.Deactivating;
                        fieldTimer = 0f;
                        DeactivateField();
                    }
                    break;

                case LightningFieldState.Deactivating:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= vfxDuration)
                    {
                        fieldState = LightningFieldState.Finished;
                    }
                    break;

                case LightningFieldState.Finished:
                    fieldState = LightningFieldState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateField()
        {
            // 장판 VFX 생성
            CreateFieldVFX();
            
            // 범위 내 적 탐지
            DetectTargetsInRange();
        }

        private void DetectTargetsInRange()
        {
            reusableColliders.Clear();
            hitTargets.Clear();
            
            Physics2D.OverlapCircle(targetPosition, lightningFieldRadius, new ContactFilter2D().NoFilter(), reusableColliders);
            
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

        private void ApplyLightningTick()
        {
            // 실시간으로 범위 내 적 재탐지
            DetectTargetsInRange();
            
            for (int i = 0; i < hitTargets.Count; i++)
            {
                Pawn target = hitTargets[i];
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    ApplyDamageToTarget(target);
                }
            }
            
            CreateLightningTickVFX();
        }

        private void ApplyDamageToTarget(Pawn target)
        {
            var attackResult = AttackResult.Create(attack, target);
            attackResult.totalDamage = (int)lightningFieldDamage;
            DamageProcessor.ProcessHit(attack, target);
        }

        private void DeactivateField()
        {
            // 장판 비활성화 VFX 생성
            CreateDeactivationVFX();
        }

        private void CreateLightningFieldVFX()
        {
            // 번개 장판 VFX 생성 로직
        }

        private void CreateFieldVFX()
        {
            // 활성화된 장판 VFX 생성 로직
        }

        private void CreateLightningTickVFX()
        {
            // 번개 틱 VFX 생성 로직
        }

        private void CreateDeactivationVFX()
        {
            // 비활성화 VFX 생성 로직
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            fieldState = LightningFieldState.None;
            fieldTimer = 0f;
            tickTimer = 0f;
            hitTargets.Clear();
        }
    }
} 