using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using CharacterSystem.Enemies;
using System;
using VFXSystem;

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
        [SerializeField] public GameObject globalVFXPrefab; // GLOBAL VFX 프리팹 (외부에서 설정 가능)
        public float globalVFXDuration = 0.3f;
        

        // 공격 효과 상태 관리
        private GlobalDamageState globalDamageState = GlobalDamageState.None;

        public DEBUFFType additionalDebuffType;
        private float globalDamageTimer = 0f;
        private float damageTimer = 0f;
        public List<Enemy> affectedEnemies = new List<Enemy>(10); // 재사용 가능한 리스트

        // 공격 효과 상태 열거형
        private enum GlobalDamageState
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

        private void DetectAllEnemies()
        {
            affectedEnemies.Clear();
            
            // BattleStage를 통해 모든 적을 가져오기 
            var allEnemies = BattleStage.now.enemies;
            
            foreach (var enemy in allEnemies)
            {
                // TO-DO: BattleStage Remove가 정상 동작하면은 Checking 지우기
                if (enemy.Value is not null && enemy.Value.transform != null && enemy.Value.gameObject.activeInHierarchy)
                {
                    affectedEnemies.Add(enemy.Value as Enemy);
                }
            }
        }

        private void ApplyGlobalDamage()
        {
            // 현재 활성화된 모든 적에게 데미지 적용
            for (int i = affectedEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = affectedEnemies[i];
                
                // 파괴된 객체 체크
                if (enemy is null || enemy.transform == null || !enemy.gameObject.activeInHierarchy)
                {
                    affectedEnemies.RemoveAt(i);
                    continue;
                }
                
                ApplyDamageToEnemy(enemy);
            }
        }

        private void ApplyDamageToEnemy(Enemy enemy)
        {
            attack.statSheet[StatType.AttackPower] = new IntegerStatValue(globalDamage);
            DamageProcessor.ProcessHit(attack, enemy);
            
            // 슬로우 효과 적용
            var debuffInfo = new DebuffInfo();
            debuffInfo.debuffType = additionalDebuffType;
            debuffInfo.attack = attack;
            debuffInfo.target = enemy;
            debuffInfo.debuffDuration = additionalDebuffDuration;
            debuffInfo.debuffMultiplier = additionalDebuffMultiplier;
            debuffInfo.debuffInterval = 1f;
            
            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);
        }

        private void CreateGlobalDamageVFX()
        {
            // VFX 시스템을 통해 얼음 폭풍 VFX 생성
            spawnedVFX = CreateAndSetupVFX(globalVFXPrefab, Vector2.zero, Vector2.zero);
            PlayVFX(spawnedVFX);
        }

        /// <summary>
        /// 얼음 폭풍 VFX를 생성하고 설정합니다.
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
            vfx.transform.localPosition = Vector3.zero;

            // 프리팹 및 자식 ParticleSystem까지 포함하여, duration 시간을 공격 지속 시간으로 설정
            var particleSystem = vfx.GetComponentInChildren<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.duration = globalDuration-0.5f;
                main.loop = false;

                foreach(var subVFX in vfx.transform.GetComponentsInChildren<ParticleSystem>())
                {
                    var subMain = subVFX.main;
                    subMain.duration = globalDuration-0.5f;
                }
            }
            
            vfx.SetActive(true);
            return vfx;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // VFX 정리
            if (spawnedVFX != null)
            {
                Destroy(spawnedVFX);
            }
            
            globalDamageState = GlobalDamageState.None;
            globalDamageTimer = 0f;
            damageTimer = 0f;
            affectedEnemies.Clear();
        }
    }
} 