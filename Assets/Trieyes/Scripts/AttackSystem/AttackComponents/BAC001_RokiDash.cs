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
    /// <summary>
    /// Boss Roki의 Dash 공격 컴포넌트
    /// AC109 Dash를 소환하여 Boss가 지정된 방향과 거리로 이동하며, 충돌하는 Pawn에게 데미지를 줍니다.
    /// </summary>
    public class BAC001_RokiDash : AttackComponent
    {   
        // Dash 소환 설정
        [Header("Dash 소환 설정")]
        public AttackData ac109AttackData; // AC109 Dash용 AttackData
        public Vector2 dashDirection = Vector2.right;   // 이동 방향
        public float dashRange = 5f;                    // 이동 거리
        public float dashDuration = 0.8f;               // 이동 시간

        // Dash 충돌 설정
        [Header("Dash 충돌 설정")]
        public float dashColliderWidth = 1.5f;    // 직사각형일 때 가로
        public float dashColliderHeight = 1.5f;   // 직사각형일 때 세로
        public int dashDamage = 50;                // 충돌 시 데미지

        // BAC 상태 관리
        private float bacTimer = 0f;
        private BACAttackState attackState = BACAttackState.None;
        private Attack spawnedAC109; // 소환된 AC109 Dash

        // BAC VFX 설정
        [Header("BAC VFX 설정")]
        [SerializeField] public GameObject bacVFXPrefab; // BAC VFX 프리팹
        public float bacVFXDuration = 0.3f;
        private GameObject spawnedVFX;

        // BAC 공격 상태 열거형
        private enum BACAttackState
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
            attackState = BACAttackState.Preparing;
            bacTimer = 0f;
            dashDirection = direction.normalized;
            
            // BAC 공격 시작
            StartBAC001Attack();
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

        private void StartBAC001Attack()
        {
            attackState = BACAttackState.Preparing;
            bacTimer = 0f;
            
            Debug.Log("<color=orange>[BAC001] Roki Dash 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // BAC 공격 상태 처리
            ProcessBAC001AttackState();
        }

        private void ProcessBAC001AttackState()
        {
            switch (attackState)
            {
                case BACAttackState.None:
                    break;

                case BACAttackState.Preparing:
                    bacTimer += Time.deltaTime;
                    
                    if (bacTimer >= 0.2f) // 준비 시간
                    {
                        attackState = BACAttackState.Active;
                        bacTimer = 0f;
                        ActivateBAC001Attack();
                    }
                    break;

                case BACAttackState.Active:
                    bacTimer += Time.deltaTime;
                    
                    // AC109 Dash 소환 처리
                    ProcessBAC001Dash();
                    
                    // 종료 조건 체크
                    if (ShouldFinishBAC001Attack())
                    {
                        attackState = BACAttackState.Finishing;
                        bacTimer = 0f;
                        FinishBAC001Attack();
                    }
                    break;

                case BACAttackState.Finishing:
                    bacTimer += Time.deltaTime;
                    
                    if (bacTimer >= 0.1f) // 종료 시간
                    {
                        attackState = BACAttackState.Finished;
                    }
                    break;

                case BACAttackState.Finished:
                    attackState = BACAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessBAC001Dash()
        {
            // AC109 Dash가 아직 소환되지 않았다면 소환
            if (spawnedAC109 == null)
            {
                SpawnAC109Dash();
            }
        }

        private void SpawnAC109Dash()
        {
            if (ac109AttackData == null)
            {
                Debug.LogError("<color=red>[BAC001] AC109 AttackData가 null입니다!</color>");
                return;
            }

            // AC109 Dash 공격 생성
            spawnedAC109 = AttackFactory.Instance.Create(ac109AttackData, attack.attacker, attack, dashDirection);
            
            if (spawnedAC109 != null)
            {
                // AC109 Dash 컴포넌트 찾기 및 설정
                var ac109Component = spawnedAC109.components[0] as AC109_Dash;
                if (ac109Component != null)
                {
                    // AC109 Dash 파라미터 설정
                    ac109Component.SetDashParameters(dashDirection, dashRange);
                    ac109Component.dashDuration = dashDuration;
                    ac109Component.dashColliderWidth = dashColliderWidth;
                    ac109Component.dashColliderHeight = dashColliderHeight;
                    ac109Component.dashDamage = dashDamage;
                    ac109Component.dashTargetType = DashTargetType.MoveWithCollision;
                    ac109Component.dashCollisionType = DashCollisionType.Rect;     

                    ac109Component.SetDashActive();

                    // BAC VFX 생성 및 시작
                    spawnedVFX = CreateAndSetupVFX(bacVFXPrefab, attack.attacker.transform.position, dashDirection);
                    PlayVFX(spawnedVFX);
                    
                    Debug.Log("<color=green>[BAC001] AC109 Dash 소환 완료!</color>");    
                }
            }
            else
            {
                Debug.LogError("<color=red>[BAC001] AC109 Dash 생성 실패!</color>");
            }
        }

        private bool ShouldFinishBAC001Attack()
        {
            // AC109 Dash가 완료되었는지 확인
            if (spawnedAC109 != null)
            {
                // AC109 Dash가 비활성화되었거나 완료되었는지 확인
                if (!spawnedAC109.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            
            // 기본적으로는 AC109 Dash가 완료될 때까지 대기
            return false;
        }

        private void ActivateBAC001Attack()
        {
            Debug.Log("<color=green>[BAC001] Roki Dash 공격 활성화!</color>");
        }

        private void FinishBAC001Attack()
        {
            // BAC VFX 정리
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }
            
            Debug.Log("<color=orange>[BAC001] Roki Dash 공격 종료!</color>");
        }

        /// <summary>
        /// BAC VFX를 생성하고 설정합니다.
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
            spawnedVFX.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            spawnedVFX.SetActive(true);
            
            return spawnedVFX;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // AC109 Dash 정리
            if (spawnedAC109 != null)
            {
                AttackFactory.Instance.Deactivate(spawnedAC109);
                spawnedAC109 = null;
            }
            
            StopAndDestroyVFX(spawnedVFX);
        }
    }
} 