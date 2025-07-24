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
    public enum AOETargetType
    {
        AreaAtPosition     // 좌표 기반 AOE
    }

    public enum AOEShapeType
    {
        Rect,    // 직사각형
        Circle   // 원형
    }

    public enum AOEMode
    {
        MultiHit   // N회 발동
    }

    /// <summary>
    /// 좌표 기반 AOE 공격 컴포넌트
    /// 지정된 좌표에 대한 Rect, Circle 범위의 지속 데미지를 구현합니다.
    /// </summary>
    public class AC100_AOE : AttackComponent
    {   
        // AOE 타겟팅 설정
        [Header("AOE 타겟팅 설정")]
        public AOETargetType aoeTargetType = AOETargetType.AreaAtPosition;

        // AOE 형태 설정
        [Header("AOE 형태 설정")]
        public AOEShapeType aoeShapeType = AOEShapeType.Circle;
        public float aoeWidth = 1f;    // Rect 형태일 때 가로
        public float aoeHeight = 1f;   // Rect 형태일 때 세로
        public float aoeRadius = 1f;   // Circle 형태일 때 반지름

        // AOE 발동 설정
        [Header("AOE 발동 설정")]
        public int aoeDamage = 20;
        public float aoeDuration = 5f;     // 지속시간
        public float aoeInterval = 1f;     // 간격

        // AOE 상태 관리
        private float aoeTimer = 0f;
        private float aoeDurationTimer = 0f;
        public List<Enemy> aoeTargets = new List<Enemy>(10);
        private Vector2 aoePosition; // AOE 위치 (바깥에서 설정)

        // AOE VFX 설정
        [Header("AOE VFX 설정")]
        public GameObject aoeVFXPrefab;
        public float aoeVFXDuration = 0.3f;

        // VFX 시스템 설정
        [Header("VFX System Settings")]
        public int vfxID = 7; // AOE VFX ID
        private GameObject aoeVFX;

        // FSM 상태 관리
        private AOEAttackState attackState = AOEAttackState.None;

        // AOE 공격 상태 열거형
        private enum AOEAttackState
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
            attackState = AOEAttackState.Preparing;
            aoeTimer = 0f;
            aoeDurationTimer = 0f;
            aoeTargets.Clear();
            
            // AOE 공격 시작
            StartAC100Attack();
        }

        /// <summary>
        /// AOE 위치를 설정합니다.
        /// </summary>
        /// <param name="position">AOE 위치 (Rect의 경우 중심 좌표)</param>
        public void SetAOEPosition(Vector2 position)
        {
            aoePosition = position;
        }

        private void StartAC100Attack()
        {
            attackState = AOEAttackState.Preparing;
            aoeTimer = 0f;
            aoeDurationTimer = 0f;
            
            Debug.Log("<color=orange>[AC100] 좌표 기반 AOE 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // AOE 공격 상태 처리
            ProcessAC100AttackState();
        }

        private void ProcessAC100AttackState()
        {
            switch (attackState)
            {
                case AOEAttackState.None:
                    break;

                case AOEAttackState.Preparing:
                    aoeTimer += Time.deltaTime;
                    
                    if (aoeTimer >= 0.1f) // 준비 시간
                    {
                        attackState = AOEAttackState.Active;
                        aoeTimer = 0f;
                        ActivateAC100Attack();
                    }
                    break;

                case AOEAttackState.Active:
                    aoeTimer += Time.deltaTime;
                    aoeDurationTimer += Time.deltaTime;
                    
                    // AOE 공격 처리
                    ProcessAC100Attack();
                    
                    // 종료 조건 체크
                    if (ShouldFinishAC100Attack())
                    {
                        attackState = AOEAttackState.Finishing;
                        aoeTimer = 0f;
                        FinishAC100Attack();
                    }
                    break;

                case AOEAttackState.Finishing:
                    aoeTimer += Time.deltaTime;
                    
                    if (aoeTimer >= 0.1f) // 종료 시간
                    {
                        attackState = AOEAttackState.Finished;
                    }
                    break;

                case AOEAttackState.Finished:
                    attackState = AOEAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessAC100Attack()
        {
            // N회 발동
            if (aoeTimer >= aoeInterval)
            {
                ExecuteAC100Attack();
                aoeTimer = 0f;
                
                // interval과 duration이 같을 때 1번 발동 후 바로 종료
                if (aoeInterval >= aoeDuration)
                {
                    attackState = AOEAttackState.Finishing;
                    aoeTimer = 0f;
                    FinishAC100Attack();
                }
            }
        }
        
        private void ExecuteAC100Attack()
        {
            ExecuteAreaAttack(aoePosition);
        }

        private void ExecuteAreaAttack(Vector2 aoePosition)
        {
            // AOE 범위 내 적 탐지
            DetectEnemiesInAOE(aoePosition);
            
            // 탐지된 적들에게 데미지 적용
            for (int i = 0; i < aoeTargets.Count; i++)
            {
                Pawn enemy = aoeTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    var attackResult = AttackResult.Create(attack, enemy);
                    attackResult.totalDamage = aoeDamage;
                    enemy.ApplyDamage(attackResult);
                }
            }
            
            // AOE VFX 생성
            CreateAC100VFX(aoePosition);

            Debug.Log($"<color=yellow>[AC100] 좌표 기반 AOE 공격으로 {aoeTargets.Count}명에게 {aoeDamage} 데미지 적용</color>");
        }

        private void DetectEnemiesInAOE(Vector2 aoePosition)
        {
            aoeTargets.Clear();
            
            switch (aoeShapeType)
            {
                case AOEShapeType.Rect:
                    // 직사각형 범위 탐지 (중심 좌표 기준)
                    Vector2 rectStart = aoePosition - new Vector2(aoeWidth * 0.5f, aoeHeight * 0.5f);
                    Vector2 rectEnd = aoePosition + new Vector2(aoeWidth * 0.5f, aoeHeight * 0.5f);
                    aoeTargets = BattleStage.now.GetEnemiesInRectRange(rectStart, rectEnd);
                    break;

                case AOEShapeType.Circle:
                    // 원형 범위 탐지
                    aoeTargets = BattleStage.now.GetEnemiesInCircleRange(aoePosition, aoeRadius);
                    break;
            }
        }

        private bool ShouldFinishAC100Attack()
        {
            // 지속시간 체크
            return aoeDurationTimer >= aoeDuration;
        }

        private void ActivateAC100Attack()
        {
            Debug.Log("<color=green>[AC100] 좌표 기반 AOE 공격 활성화!</color>");
        }

        private void FinishAC100Attack()
        {
            Debug.Log("<color=orange>[AC100] 좌표 기반 AOE 공격 종료!</color>");
        }

        private void CreateAC100VFX(Vector2 position)
        {
            // VFX 시스템을 통해 AOE VFX 생성
            aoeVFX = CreateAndSetupVFX(position, Vector2.zero);
            PlayVFX(aoeVFX);
            
            Debug.Log($"<color=blue>[AC100] AOE VFX 생성! VFX ID: {vfxID}</color>");
        }

        /// <summary>
        /// AOE VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(Vector2 position, Vector2 direction)
        {
            // VFXFactory를 통해 AOE VFX 생성
            GameObject vfx = VFXFactory.Instance.SpawnVFX(vfxID, position, direction);
            
            // 여기서 VFX의 세부 조정을 할 수 있습니다
            ParticleSystem childVFX = vfx.transform.GetChild(0).GetComponent<ParticleSystem>();

            // Render Alignment 설정
            ParticleSystemRenderer renderer = childVFX.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.alignment = ParticleSystemRenderSpace.Local;
                renderer.sortingOrder = 70;
            }

            vfx.transform.position = position;
            
            // 기본 스케일 설정 (AOE 크기에 맞춤)
            float baseScale = 1.0f;
            float finalScale = baseScale * aoeRadius * 1.5f;
            vfx.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
            
            // AOE 색상 설정 (빨간색-주황색 계열)
            var colorOverLifetime = childVFX.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), new GradientColorKey(Color.yellow, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0.4f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            return vfx;
        }
    }
}