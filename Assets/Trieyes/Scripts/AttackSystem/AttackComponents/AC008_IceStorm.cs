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
    /// 얼음 폭풍 효과 (무기 속성 연계)
    /// 얼음 속성이 부여된 무기에서 발동되는 맵 전체 눈보라 효과를 소환합니다.
    /// AC103_GlobalBlizzard를 생성하여 맵 전체에 눈보라 효과를 적용합니다.
    /// </summary>
    public class AC008_IceStorm : AttackComponent
    {
        [Header("눈보라 소환 설정")]
        private const int AC103_GLOBAL = 15;
        public float summonDelay = 0.5f; // 소환 지연 시간
        public float vfxDuration = 0.3f; // VFX 지속 시간

        [Header("VFX 설정")]
        public GameObject summonVFXPrefab; // 소환 VFX

        // 소환 상태 관리
        private SummonState summonState = SummonState.None;
        private float summonTimer = 0f;

        // 소환 상태 열거형
        private enum SummonState
        {
            None,
            Preparing,
            Summoning,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            summonState = SummonState.None;
            summonTimer = 0f;
            
            // 눈보라 소환 시작
            StartBlizzardSummon();
        }

        private void StartBlizzardSummon()
        {
            summonState = SummonState.Preparing;
            summonTimer = 0f;
            
            // 소환 VFX 생성
            CreateSummonVFX();
            
            Debug.Log("<color=cyan>[ICE_STORM] 얼음 속성 무기 - 눈보라 소환 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // 소환 처리
            ProcessBlizzardSummon();
        }

        private void ProcessBlizzardSummon()
        {
            switch (summonState)
            {
                case SummonState.None:
                    break;

                case SummonState.Preparing:
                    summonTimer += Time.deltaTime;
                    
                    if (summonTimer >= summonDelay)
                    {
                        summonState = SummonState.Summoning;
                        summonTimer = 0f;
                        SummonGlobalBlizzard();
                    }
                    break;

                case SummonState.Summoning:
                    summonTimer += Time.deltaTime;
                    
                    if (summonTimer >= vfxDuration)
                    {
                        summonState = SummonState.Finished;
                    }
                    break;

                case SummonState.Finished:
                    summonState = SummonState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void SummonGlobalBlizzard()
        {
            Debug.Log("<color=cyan>[ICE_STORM] AC103_GLOBAL 소환!</color>");
            
            // AttackComponentFactory를 통해 AC103_GLOBAL 컴포넌트 생성
            var globalBlizzardAttack = AttackFactory.Instance.ClonePrefab(AC103_GLOBAL);
            BattleStage.now.AttachAttack(globalBlizzardAttack);
            globalBlizzardAttack.target = attack.target;

            var globalBlizzardComponent = globalBlizzardAttack.components[0] as AC103_GLOBAL;
            globalBlizzardComponent.globalDamage = attack.statSheet[StatType.AttackPower];
            globalBlizzardComponent.globalDuration = 6f;
            globalBlizzardComponent.damageInterval = 0.5f;

            globalBlizzardComponent.additionalDebuffType = DEBUFFType.Slow;
            globalBlizzardComponent.additionalDebuffDuration = 2f;
            globalBlizzardComponent.additionalDebuffChance = 0.3f;
            globalBlizzardComponent.additionalDebuffMultiplier = 2f;

            globalBlizzardAttack.Activate(attack.attacker, Vector2.zero);
            
            // 소환 완료 VFX 생성
            CreateSummonCompleteVFX();
        }

        private void CreateSummonVFX()
        {
            if (summonVFXPrefab != null)
            {
                GameObject summonVFX = Instantiate(summonVFXPrefab);
                summonVFX.transform.position = attacker.transform.position;
                
                // 소환 VFX 설정
                SummonVFX summonComponent = summonVFX.GetComponent<SummonVFX>();
                if (summonComponent != null)
                {
                    summonComponent.Initialize(summonDelay);
                }
                
                Destroy(summonVFX, summonDelay + 0.1f);
            }
        }

        private void CreateSummonCompleteVFX()
        {
            if (summonVFXPrefab != null)
            {
                GameObject completeVFX = Instantiate(summonVFXPrefab);
                completeVFX.transform.position = Vector3.zero; // 맵 중앙
                
                // 완료 VFX 설정
                SummonCompleteVFX completeComponent = completeVFX.GetComponent<SummonCompleteVFX>();
                if (completeComponent != null)
                {
                    completeComponent.Initialize(vfxDuration);
                }
                
                Destroy(completeVFX, vfxDuration + 0.1f);
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            summonState = SummonState.None;
            summonTimer = 0f;
        }
    }

    /// <summary>
    /// 소환 VFX 컴포넌트
    /// </summary>
    public class SummonVFX : MonoBehaviour
    {
        [Header("소환 VFX 설정")]
        public float summonDuration = 0.5f;
        public Color summonColor = Color.cyan;

        private float currentDuration = 0f;
        private bool isActive = false;

        public void Initialize(float duration)
        {
            summonDuration = duration;
            currentDuration = 0f;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;
            
            currentDuration += Time.deltaTime;
            
            if (currentDuration >= summonDuration)
            {
                isActive = false;
            }
        }
    }

    /// <summary>
    /// 소환 완료 VFX 컴포넌트
    /// </summary>
    public class SummonCompleteVFX : MonoBehaviour
    {
        [Header("소환 완료 VFX 설정")]
        public float completeDuration = 0.3f;
        public Color completeColor = Color.white;

        private float currentDuration = 0f;
        private bool isActive = false;

        public void Initialize(float duration)
        {
            completeDuration = duration;
            currentDuration = 0f;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;
            
            currentDuration += Time.deltaTime;
            
            if (currentDuration >= completeDuration)
            {
                isActive = false;
            }
        }
    }
} 