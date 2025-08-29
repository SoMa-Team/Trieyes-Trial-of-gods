using AttackSystem;
using UnityEngine;
using System;
using CharacterSystem;
using Stats;
using BattleSystem;
using System.Collections.Generic;
using VFXSystem;
using PrimeTween;

namespace AttackComponents
{
    public enum DashTargetType
    {
        MoveOnly,           // 이동만 하는 경우
        MoveWithCollision   // 이동하면서 충돌하는 Pawn에게 데미지를 주는 경우
    }

    public enum DashCollisionType
    {
        Rect,    // 직사각형 콜라이더
        Circle   // 원형 콜라이더
    }

    /// <summary>
    /// Dash 공격 컴포넌트
    /// owner pawn을 지정된 방향과 거리로 이동시키며, 설정에 따라 충돌하는 Pawn에게 데미지를 줍니다.
    /// </summary>
    public class AC109_Dash : AttackComponent
    {   
        // Dash 타겟팅 설정
        [Header("Dash 타겟팅 설정")]
        public DashTargetType dashTargetType = DashTargetType.MoveWithCollision;

        // Dash 이동 설정
        [Header("Dash 이동 설정")]
        public Vector2 dashDirection = Vector2.right;   // 이동 방향
        public float dashRange = 3f;                    // 이동 거리
        public float dashDuration = 0.5f;               // 이동 시간

        // Dash 충돌 설정
        [Header("Dash 충돌 설정")]
        public DashCollisionType dashCollisionType = DashCollisionType.Rect;
        public float dashColliderWidth = 1f;    // 직사각형일 때 가로
        public float dashColliderHeight = 1f;   // 직사각형일 때 세로
        public float dashColliderRadius = 0.5f; // 원형일 때 반지름
        public int dashDamage = 30;              // 충돌 시 데미지

        // Dash 상태 관리
        private float dashTimer = 0f;
        private Vector2 dashStartPosition;
        private Vector2 dashTargetPosition;
        private Pawn dashOwner; // 이동할 Pawn 객체
        private bool isDashActive = false;

        // Dash VFX 설정
        [Header("Dash VFX 설정")]
        [SerializeField] public GameObject dashVFXPrefab; // Dash VFX 프리팹
        public float dashVFXDuration = 0.3f;
        

        // FSM 상태 관리
        private DashAttackState attackState = DashAttackState.None;

        // Dash 공격 상태 열거형
        private enum DashAttackState
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
            attackState = DashAttackState.Preparing;
            dashTimer = 0f;
            dashOwner = attack.attacker;
            dashDirection = direction.normalized;
            
            // Dash 공격 시작
            StartAC109Attack();
        }

        /// <summary>
        /// Dash 방향과 거리를 설정합니다.
        /// </summary>
        /// <param name="direction">이동 방향</param>
        /// <param name="range">이동 거리</param>
        public void SetDashParameters(Vector2 direction, float range)
        {
            dashDirection = direction.normalized;
            dashRange = range;
        }

        private void StartAC109Attack()
        {
            attackState = DashAttackState.Preparing;
            dashTimer = 0f;
            isDashActive = false;
            
            Debug.Log("<color=orange>[AC109] Dash 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // Dash 공격 상태 처리
            ProcessAC109AttackState();
        }

        public void SetDashActive()
        {
            attackState = DashAttackState.Active;
            dashTimer = 0f;
            ActivateAC109Attack();
        }

        private void ProcessAC109AttackState()
        {
            switch (attackState)
            {
                case DashAttackState.None:
                    break;

                case DashAttackState.Preparing:
                    break;

                case DashAttackState.Active:
                    dashTimer += Time.deltaTime;
                    
                    // Dash 이동 처리
                    ProcessAC109Dash();
                    
                    // 종료 조건 체크
                    if (ShouldFinishAC109Attack())
                    {
                        attackState = DashAttackState.Finishing;
                        dashTimer = 0f;
                        FinishAC109Attack();
                    }
                    break;

                case DashAttackState.Finishing:
                    dashTimer += Time.deltaTime;
                    
                    if (dashTimer >= 0.1f) // 종료 시간
                    {
                        attackState = DashAttackState.Finished;
                    }
                    break;

                case DashAttackState.Finished:
                    attackState = DashAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessAC109Dash()
        {
            if (!isDashActive) return;

            // 진행률 계산 (0~1)
            float progress = dashTimer / dashDuration;
            
            // 현재 위치 계산
            Vector2 currentPosition = Vector2.Lerp(dashStartPosition, dashTargetPosition, progress);
            
            // Pawn 이동
            if (dashOwner != null && dashOwner.gameObject.activeInHierarchy)
            {
                dashOwner.transform.position = currentPosition;
            }
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            base.ProcessComponentCollision(targetPawn);
            DamageProcessor.ProcessHit(attack, targetPawn);
        }

        private bool ShouldFinishAC109Attack()
        {
            // 이동 시간 체크
            return dashTimer >= dashDuration;
        }

        private void ActivateAC109Attack()
        {
            if (dashOwner == null)
            {
                Debug.LogError("<color=red>[AC109] Dash owner가 null입니다!</color>");
                return;
            }

            // Dash 시작 위치와 목표 위치 설정
            dashStartPosition = dashOwner.transform.position;
            dashTargetPosition = dashStartPosition + (dashDirection * dashRange);

            // Pawn의 자동 이동 비활성화
            dashOwner.SetLockMovement(true);

            // Dash 활성화
            isDashActive = true;

            // Dash VFX 생성 및 시작
            spawnedVFX = CreateAndSetupVFX(dashVFXPrefab, dashStartPosition, dashDirection);
            PlayVFX(spawnedVFX);
            
            Debug.Log("<color=green>[AC109] Dash 공격 활성화!</color>");
        }

        private void FinishAC109Attack()
        {
            // Pawn의 자동 이동 다시 활성화
            dashOwner.SetLockMovement(false);

            // Dash 비활성화
            isDashActive = false;

            // Dash VFX 정리
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }
            
            Debug.Log("<color=orange>[AC109] Dash 공격 종료!</color>");
        }

        /// <summary>
        /// Dash VFX를 생성하고 설정합니다.
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
            spawnedVFX.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            spawnedVFX.SetActive(true);
            
            return spawnedVFX;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            dashOwner.SetLockMovement(false);
            
            // Dash 비활성화
            isDashActive = false;
            
            StopAndDestroyVFX(spawnedVFX);
        }
    }
} 