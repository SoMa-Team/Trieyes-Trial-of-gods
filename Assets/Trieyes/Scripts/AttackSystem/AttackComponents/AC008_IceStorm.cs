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
    /// 얼음 폭풍 효과
    /// 지정된 범위에 얼음 폭풍을 생성하여 적들에게 데미지를 입힙니다.
    /// GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    public class AC008_IceStorm : AttackComponent
    {
        [Header("얼음 폭풍 설정")]
        public float iceStormDamage = 40f;
        public float iceStormRadius = 3f;
        public float iceStormDuration = 2f;
        public float iceStormDelay = 0.2f;

        [Header("VFX 설정")]
        public GameObject iceStormVFXPrefab;
        public float vfxDuration = 0.5f;

        // 얼음 폭풍 상태 관리
        private IceStormState stormState = IceStormState.None;
        private float stormTimer = 0f;
        private Vector2 targetPosition;
        private List<Pawn> hitTargets = new List<Pawn>(10);

        // 얼음 폭풍 상태 열거형
        private enum IceStormState
        {
            None,
            Preparing,
            Storming,
            Impact,
            Finished
        }

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(20);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            stormState = IceStormState.None;
            stormTimer = 0f;
            hitTargets.Clear();
            
            // 타겟 위치 설정
            targetPosition = (Vector2)attacker.transform.position + direction * 4f;
            
            // 얼음 폭풍 시작
            StartIceStorm();
        }

        private void StartIceStorm()
        {
            stormState = IceStormState.Preparing;
            stormTimer = 0f;
            
            // VFX 생성
            CreateIceStormVFX();
        }

        protected override void Update()
        {
            base.Update();
            
            // 얼음 폭풍 처리
            ProcessIceStorm();
        }

        private void ProcessIceStorm()
        {
            switch (stormState)
            {
                case IceStormState.None:
                    break;

                case IceStormState.Preparing:
                    stormTimer += Time.deltaTime;
                    
                    if (stormTimer >= iceStormDelay)
                    {
                        stormState = IceStormState.Storming;
                        stormTimer = 0f;
                        StartStorming();
                    }
                    break;

                case IceStormState.Storming:
                    stormTimer += Time.deltaTime;
                    
                    if (stormTimer >= iceStormDuration)
                    {
                        stormState = IceStormState.Impact;
                        stormTimer = 0f;
                        ApplyStormDamage();
                    }
                    break;

                case IceStormState.Impact:
                    stormTimer += Time.deltaTime;
                    
                    if (stormTimer >= vfxDuration)
                    {
                        stormState = IceStormState.Finished;
                    }
                    break;

                case IceStormState.Finished:
                    stormState = IceStormState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void StartStorming()
        {
            // 폭풍 VFX 생성
            CreateStormVFX();
            
            // 범위 내 적 탐지
            DetectTargetsInRange();
        }

        private void DetectTargetsInRange()
        {
            reusableColliders.Clear();
            hitTargets.Clear();
            
            Physics2D.OverlapCircle(targetPosition, iceStormRadius, new ContactFilter2D().NoFilter(), reusableColliders);
            
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

        private void ApplyStormDamage()
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
            attackResult.totalDamage = (int)iceStormDamage;
            DamageProcessor.ProcessHit(attack, target);
        }

        private void CreateIceStormVFX()
        {
            // 얼음 폭풍 VFX 생성 로직
        }

        private void CreateStormVFX()
        {
            // 폭풍 VFX 생성 로직
        }

        private void CreateImpactVFX()
        {
            // 충격 VFX 생성 로직
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            stormState = IceStormState.None;
            stormTimer = 0f;
            hitTargets.Clear();
        }
    }
} 