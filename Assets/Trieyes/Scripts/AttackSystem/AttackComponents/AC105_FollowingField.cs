using AttackSystem;
using CharacterSystem;
using CharacterSystem.Enemies;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 플레이어를 따라다니는 자기장 효과
    /// 플레이어 주변에 자기장을 형성하여 닿는 적에게 피해를 주고, 이동속도가 증가합니다.
    /// 플레이어가 이동하면 자기장도 함께 따라다닙니다.
    /// BattleStage 기반 적 감지로 최적화된 성능을 제공합니다.
    /// </summary>

    public class AC105_FollowingField : AttackComponent
    {
        [Header("필드 타입 및 크기 설정")]
        public float fieldRadius; // 원형일 때 반지름
        public float fieldDamage;
        public float fieldTickInterval;
        public float fieldDuration;
        
        [Header("따라다니기 설정")]
        public float followDistance = 0f; // 플레이어로부터의 거리 (0이면 플레이어 위치)
        public bool followPlayer = true;
        public Vector2 followOffset = Vector2.zero; // 추가 오프셋
        
        [Header("버프 설정")]
        
        [Header("VFX 설정")]
        [SerializeField] public GameObject fieldVFXPrefab; // 필드 VFX 프리팹 (외부에서 설정 가능)
        public float fieldVFXDuration = 0.3f;
        

        // 자기장 상태 관리
        private FollowingFieldState fieldState = FollowingFieldState.None;
        private float fieldTimer = 0f;
        private float damageTimer = 0f;
        private List<Enemy> fieldTargets = new List<Enemy>(10); // 재사용 가능한 리스트
        
        // 자기장 상태 열거형
        private enum FollowingFieldState
        {
            None,
            Starting,
            Active,
            Ending,
            Finished
        }
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fieldState = FollowingFieldState.None;
            fieldTimer = 0f;
            damageTimer = 0f;
            fieldTargets.Clear();
            
            // 자기장 시작
            StartFollowingField();
        }
        
        private void StartFollowingField()
        {
            fieldState = FollowingFieldState.Starting;
            fieldTimer = 0f;
            damageTimer = 0f;
            
            //Debug.Log("<color=cyan>[AC104] 따라다니는 자기장 시작!</color>");
        }
        
        protected override void Update()
        {
            base.Update();
            
            // 자기장 처리
            ProcessFollowingField();
        }
        
        private void ProcessFollowingField()
        {
            switch (fieldState)
            {
                case FollowingFieldState.None:
                    break;
                    
                case FollowingFieldState.Starting:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= 0.1f) // 시작 지연
                    {
                        fieldState = FollowingFieldState.Active;
                        fieldTimer = 0f;
                        ActivateField();
                    }
                    break;
                    
                case FollowingFieldState.Active:
                    fieldTimer += Time.deltaTime;
                    damageTimer += Time.deltaTime;
                    
                    // 데미지 처리
                    if (damageTimer >= fieldTickInterval)
                    {
                        ApplyFieldDamage();
                        damageTimer = 0f;
                        
                        // interval과 duration이 같을 때 1번 발동 후 바로 종료
                        if (fieldTickInterval >= fieldDuration)
                        {
                            fieldState = FollowingFieldState.Ending;
                            fieldTimer = 0f;
                            DeactivateField();
                        }
                    }
                    
                    // 지속시간 체크
                    if (fieldTimer >= fieldDuration)
                    {
                        fieldState = FollowingFieldState.Ending;
                        fieldTimer = 0f;
                        DeactivateField();
                    }
                    break;
                    
                case FollowingFieldState.Ending:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= fieldVFXDuration)
                    {
                        fieldState = FollowingFieldState.Finished;
                    }
                    break;
                    
                case FollowingFieldState.Finished:
                    fieldState = FollowingFieldState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }
        
        private void ActivateField()
        {
            // VFX 생성 (Active 상태에서 생성)
            CreateFieldVFX();
            
            //Debug.Log("<color=green>[AC104] 자기장 활성화!</color>");
        }
        
        private void ApplyFieldDamage()
        {
            // 자기장 범위 내 적 탐지 (BattleStage 기반)
            fieldTargets.Clear();
            fieldTargets = BattleStage.now.GetEnemiesInCircleRange(attack.attacker.transform.position, fieldRadius);
            
            //Debug.Log($"<color=blue>[AC104] 자기장 범위 내 적 탐지: {fieldTargets.Count}명</color>");
            
            // 탐지된 적들에게 데미지 적용
            for (int i = 0; i < fieldTargets.Count; i++)
            {
                Pawn enemy = fieldTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    attack.statSheet[StatType.AttackPower] = new IntegerStatValue((int)fieldDamage);
                    DamageProcessor.ProcessHit(attack, enemy);
                }
            }
        }
        
        private void CreateFieldVFX()
        {
            // VFX 시스템을 통해 번개 장판 VFX 생성
            spawnedVFX = CreateAndSetupVFX(fieldVFXPrefab, (Vector2)transform.position, Vector2.zero);
            spawnedVFX.SetActive(true);
            PlayVFX(spawnedVFX);
            
            //Debug.Log($"<color=blue>[AC105] 번개 장판 VFX 생성!</color>");
        }
        
        private void DeactivateField()
        {
            // VFX 정리
            if (spawnedVFX != null)
            {
                spawnedVFX.transform.SetParent(null);
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }
            
            //Debug.Log("<color=cyan>[AC105] 따라다니는 자기장 종료!</color>");
        }
        
        /// <summary>
        /// 번개 장판 VFX를 생성하고 설정합니다.
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
            GameObject vfx = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            vfx.transform.SetParent(attack.attacker.transform);
            vfx.transform.localPosition = new Vector3(0, 0, 0);
            vfx.transform.localScale = new Vector3(0.36f * fieldRadius, 0.36f * fieldRadius, 1f);
            
            return vfx;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // VFX 정리
            if (spawnedVFX != null)
            {
                Destroy(spawnedVFX);
                spawnedVFX = null;
            }
            
            fieldState = FollowingFieldState.None;
            fieldTimer = 0f;
            damageTimer = 0f;
            fieldTargets.Clear();
        }
    }
} 