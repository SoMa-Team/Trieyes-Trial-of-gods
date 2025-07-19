using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using CharacterSystem.Enemies;
using System;

namespace AttackComponents
{
    /// <summary>
    /// 맵 전체 공격 효과
    /// 맵 전체에 걸쳐 지속적으로 공격 효과를 입히는 컴포넌트입니다.
    /// GC 최적화를 위해 재사용 가능한 리스트를 사용합니다.
    /// </summary>
    
    public class AC104_GLOBAL : AttackComponent
    {
        [Header("공격 효과 설정")]
        public int globalDamage = 25; // 공격 효과
        public float globalDuration = 6f; // 공격 효과 지속 시간
        public float damageInterval = 0.5f; // 데미지 적용 간격
        public float additionalDebuffDuration = 2f; // 추가 디버프 지속 시간
        public float additionalDebuffChance = 0.3f; // 추가 디버프 확률
        public float additionalDebuffMultiplier = 2f; // 추가 디버프 배율

        [Header("VFX 설정")]
        public GameObject globalVFXPrefab;
        public float globalVFXDuration = 0.3f;

        // 공격 효과 상태 관리
        private GlobalDamageState globalDamageState = GlobalDamageState.None;

        public DEBUFFType additionalDebuffType;
        private float globalDamageTimer = 0f;
        private float damageTimer = 0f;
        private List<Enemy> affectedEnemies = new List<Enemy>(10); // 재사용 가능한 리스트

        // 공격 효과 상태 열거형
        private enum GlobalDamageState
        {
            None,
            Starting,
            Active,
            Ending,
            Finished
        }

        private enum AdditionalDebuffState
        {
            None,
            AdditionalDebuff,
            AdditionalDebuffEnd
        }

        private const int AC100_SINGLE_AOE = 10;

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(100);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            globalDamageState = GlobalDamageState.Starting;
            globalDamageTimer = 0f;
            damageTimer = 0f;
            affectedEnemies.Clear();
            
            // 눈보라 시작
            StartGlobalDamage();
        }

        private void StartGlobalDamage()
        {
            globalDamageState = GlobalDamageState.Starting;
            globalDamageTimer = 0f;
            damageTimer = 0f;
            
            // VFX 생성
            CreateGlobalDamageVFX();
            
            Debug.Log("<color=cyan>[GLOBAL_BLIZZARD] 공격 효과 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // 공격 효과 처리
            ProcessGlobalDamage();
        }

        private void ProcessGlobalDamage()
        {
            switch (globalDamageState)
            {
                case GlobalDamageState.None:
                    break;

                case GlobalDamageState.Starting:
                    globalDamageTimer += Time.deltaTime;
                    
                    if (globalDamageTimer >= 0.1f) // 시작 지연
                    {
                        globalDamageState = GlobalDamageState.Active;
                        globalDamageTimer = 0f;
                        StartActiveGlobalDamage();
                    }
                    break;

                case GlobalDamageState.Active:
                    globalDamageTimer += Time.deltaTime;
                    damageTimer += Time.deltaTime;
                    
                    // 주기적으로 데미지 적용
                    if (damageTimer >= damageInterval)
                    {
                        DetectAllEnemies();
                        ApplyGlobalDamage();
                        damageTimer = 0f;
                    }
                    
                    // 공격 효과 지속 시간 종료
                    if (globalDamageTimer >= globalDuration)
                    {
                        globalDamageState = GlobalDamageState.Ending;
                        globalDamageTimer = 0f;
                        EndGlobalDamage();
                    }
                    break;

                case GlobalDamageState.Ending:
                    globalDamageTimer += Time.deltaTime;
                    
                    if (globalDamageTimer >= globalVFXDuration)
                    {
                        globalDamageState = GlobalDamageState.Finished;
                    }
                    break;

                case GlobalDamageState.Finished:
                    globalDamageState = GlobalDamageState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void StartActiveGlobalDamage()
        {
            Debug.Log("<color=cyan>[GLOBAL_BLIZZARD] 공격 효과 활성화!</color>");
            
            // 맵 전체 범위에서 모든 적 탐지
            DetectAllEnemies();
        }

        private void DetectAllEnemies()
        {
            affectedEnemies.Clear();
            
            // BattleStage를 통해 모든 적을 가져오기 
            var allEnemies = BattleStage.now.enemies;
            
            foreach (var enemy in allEnemies)
            {
                // TO-DO: BattleStage Remove가 정상 동작하면은 Checking 지우기
                if (enemy != null && enemy.transform != null && enemy.gameObject.activeInHierarchy)
                {
                    affectedEnemies.Add(enemy as Enemy);
                }
            }
            
            Debug.Log($"<color=cyan>[GLOBAL_BLIZZARD] 영향받는 적 수: {affectedEnemies.Count}명</color>");
        }

        private void ApplyGlobalDamage()
        {
            // 현재 활성화된 모든 적에게 데미지 적용
            for (int i = affectedEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = affectedEnemies[i];
                
                // 파괴된 객체 체크
                if (enemy == null || enemy.transform == null || !enemy.gameObject.activeInHierarchy)
                {
                    affectedEnemies.RemoveAt(i);
                    continue;
                }
                
                ApplyDamageToEnemy(enemy);
            }
        }

        private void ApplyDamageToEnemy(Enemy enemy)
        {
            // AttackResult 생성 및 데미지 처리
            var attackResult = AttackResult.Create(attack, enemy);
            
            // 기본 데미지 설정
            attackResult.attacker = attack.attacker;
            attackResult.target = enemy;
            attackResult.isCritical = false;
            attackResult.isEvaded = false;
            attackResult.totalDamage = globalDamage;

            // 데미지 적용
            enemy.ApplyDamage(attackResult);
            
            // 슬로우 효과 적용
            var debuffInfo = new DebuffInfo();
            debuffInfo.debuffType = additionalDebuffType;
            debuffInfo.attack = attack;
            debuffInfo.target = enemy;
            debuffInfo.debuffDuration = additionalDebuffDuration;
            debuffInfo.debuffMultiplier = additionalDebuffMultiplier;
            debuffInfo.debuffInterval = 1f;
            debuffInfo.globalDamage = 0;
            
            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            Debug.Log($"<color=blue>[GLOBAL_BLIZZARD] {enemy.pawnName}에게 {attackResult.totalDamage} 데미지 적용</color>");
        }

        private void EndGlobalDamage()
        {
            Debug.Log("<color=cyan>[GLOBAL_BLIZZARD] 공격 효과 종료!</color>");
            
            // 종료 VFX 생성
            CreateEndGlobalDamageVFX();
        }

        private void CreateGlobalDamageVFX()
        {
            if (globalVFXPrefab != null)
            {
                GameObject globalVFX = Instantiate(globalVFXPrefab);
                globalVFX.transform.position = Vector3.zero; // 맵 중앙

                GlobalDamageVFX globalVFXComponent = globalVFX.GetComponent<GlobalDamageVFX>();
                
                // GlobalVFX 컴포넌트 찾아서 공격 효과 시작
                if (globalVFXComponent != null)
                {
                    globalVFXComponent.Initialize(globalVFXDuration);
                }
                
                // 지속 시간 후 VFX 제거
                Destroy(globalVFX, globalVFXDuration + 1f);
            }
        }

        private void CreateEndGlobalDamageVFX()
        {
            if (globalVFXPrefab != null)
            {
                GameObject endVFX = Instantiate(globalVFXPrefab);
                endVFX.transform.position = Vector3.zero;
                
                // 종료 VFX 설정
                GlobalDamageEndVFX endComponent = endVFX.GetComponent<GlobalDamageEndVFX>();
                if (endComponent != null)
                {
                    endComponent.Initialize(globalVFXDuration);
                }
                
                Destroy(endVFX, globalVFXDuration + 0.1f);
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            globalDamageState = GlobalDamageState.None;
            globalDamageTimer = 0f;
            damageTimer = 0f;
            affectedEnemies.Clear();
        }
    }

    /// <summary>
    /// 눈보라 VFX 컴포넌트
    /// </summary>
    public class GlobalDamageVFX : MonoBehaviour
    {
        [Header("공격 효과 VFX 설정")]
        public float globalDamageDuration = 6f;
        public Color globalDamageColor = Color.cyan;

        public ParticleSystem globalDamageParticles;

        private float currentDuration = 0f;
        private bool isActive = false;

        public void Initialize(float duration)
        {
            globalDamageDuration = duration;
            currentDuration = 0f;
            isActive = true;
            
            // 파티클 시스템 설정
            if (globalDamageParticles != null)
            {
                globalDamageParticles.Play();
                var main = globalDamageParticles.main;
                main.duration = duration;
                main.startColor = globalDamageColor;
            }
        }

        private void Update()
        {
            if (!isActive) return;
            
            currentDuration += Time.deltaTime;
            
            if (currentDuration >= globalDamageDuration)
            {
                isActive = false;
                if (globalDamageParticles != null)
                {
                    globalDamageParticles.Stop();
                }
            }
        }
    }

    /// <summary>
    /// 공격 효과 종료 VFX 컴포넌트
    /// </summary>
    public class GlobalDamageEndVFX : MonoBehaviour
    {
        [Header("공격 효과 종료 VFX 설정")]
        public float endDuration = 0.3f;
        public Color endColor = Color.white;

        private float currentDuration = 0f;
        private bool isActive = false;

        public void Initialize(float duration)
        {
            endDuration = duration;
            currentDuration = 0f;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;
            
            currentDuration += Time.deltaTime;
            
            if (currentDuration >= endDuration)
            {
                isActive = false;
            }
        }
    }
} 