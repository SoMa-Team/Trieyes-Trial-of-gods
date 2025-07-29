using System.Collections.Generic;
using AttackSystem;
using CharacterSystem;
using UnityEngine;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 영웅 소드 공격 강화 효과
    /// Activate 이후 7초 동안 계속해서 AC003, AC004, AC005, AC006 중 랜덤으로 하나를 생성합니다.
    /// 기본 공격 로직은 각 강화 효과에 포함되어 있으므로 단순히 강화 효과만 생성합니다.
    /// </summary>
    public class Hero_S001_AttackEnchantment : AttackComponent
    {
        [Header("강화 설정")]
        public int randomEnchantmentID = 1;
        public float enchantmentDuration = 7f; // 강화 지속 시간
        public float generationInterval = 1f; // 강화 효과 생성 간격

        private Character001_Hero character;
        private float lastEnchantmentTime = 0f;

        // FSM 상태 관리
        private EnchantmentState enchantmentState = EnchantmentState.None;
        private float enchantmentTimer = 0f;

        // 강화 효과 상태 열거형
        private enum EnchantmentState
        {
            None,
            Preparing,
            Active,
            Finishing,
            Finished
        }

        // 강화 효과 ID 상수
        private const int FIRE_ENCHANTMENT_ID = 1;    // AC003_HeroSwordEnchantmentFire
        private const int ICE_ENCHANTMENT_ID = 2;     // AC004_HeroSwordEnchantmentIce
        private const int LIGHTNING_ENCHANTMENT_ID = 3; // AC005_HeroSwordEnchantmentLightning
        private const int LIGHT_ENCHANTMENT_ID = 4;   // AC006_HeroSwordEnchantmentLight

        [SerializeField] public List<AttackData> attackDatas;

        private int currentKilledCount = 0;
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            character = attack.attacker as Character001_Hero;
            
            // 초기 상태 설정
            enchantmentState = EnchantmentState.Preparing;
            enchantmentTimer = 0f;
            lastEnchantmentTime = 0f;
            
            // Lock 상태 설정
            attack.SetLock(true);
            
            // 강화 효과 시작
            StartEnchantmentEffect();
        }

        public override void PerformLockedSetup()
        {
            base.PerformLockedSetup();
            
            // Lock 상태에서 실행해야 하는 초기 설정
            // 속성 결정 및 Hero에 부여
            DetermineAndSetEnchantment();
        }

        private void DetermineAndSetEnchantment()
        {
            // 랜덤 강화 효과 선택
            randomEnchantmentID = GetRandomEnchantmentID();
            SetHeroWeaponElementState(randomEnchantmentID);
            
            Debug.Log($"[S001] Lock 상태에서 {randomEnchantmentID}번 속성 결정됨!");
        }

        private void StartEnchantmentEffect()
        {
            enchantmentState = EnchantmentState.Preparing;
            enchantmentTimer = 0f;
            lastEnchantmentTime = 0f;
            
            character.lockBasicAttack = true;
            
            Debug.Log("<color=yellow>[S001] 영웅 소드 강화 효과 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // 강화 효과 상태 처리
            ProcessEnchantmentState();
        }

        private void ProcessEnchantmentState()
        {
            switch (enchantmentState)
            {
                case EnchantmentState.None:
                    break;

                case EnchantmentState.Preparing:
                    enchantmentTimer += Time.deltaTime;
                    
                    if (enchantmentTimer >= 0.1f) // 준비 시간
                    {
                        enchantmentState = EnchantmentState.Active;
                        enchantmentTimer = 0f;
                        ActivateEnchantment();
                    }
                    break;

                case EnchantmentState.Active:
                    enchantmentTimer += Time.deltaTime;
                    //UpdateEnchantmentDuration(); Relic001 활성화 시 이것 발동
                    
                    // 1초에 1번 강화 효과 생성
                    if (Time.time - lastEnchantmentTime >= generationInterval)
                    {
                        TriggerRandomEnchantment();
                        lastEnchantmentTime = Time.time;
                    }
                    
                    if (enchantmentTimer >= enchantmentDuration)
                    {
                        enchantmentState = EnchantmentState.Finishing;
                        enchantmentTimer = 0f;
                        FinishEnchantment();
                    }
                    break;

                case EnchantmentState.Finishing:
                    enchantmentTimer += Time.deltaTime;
                    
                    if (enchantmentTimer >= 0.1f) // 종료 시간
                    {
                        enchantmentState = EnchantmentState.Finished;
                    }
                    break;

                case EnchantmentState.Finished:
                    enchantmentState = EnchantmentState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateEnchantment()
        {
            // Lock 해제
            attack.SetLock(false);
            
            Debug.Log("<color=yellow>[S001] 영웅 소드 강화 효과 활성화!</color>");
        }

        private void FinishEnchantment()
        {
            character.weaponElementState = HeroWeaponElementState.None;
            character.lockBasicAttack = false;
            
            Debug.Log("<color=yellow>[S001] 영웅 소드 강화 효과 종료!</color>");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            character.weaponElementState = HeroWeaponElementState.None;
            character.lockBasicAttack = false;
            
            enchantmentState = EnchantmentState.None;
            enchantmentTimer = 0f;
            lastEnchantmentTime = 0f;
            character.killedDuringSkill001 = 0;
            character.killedDuringSkill002 = 0;
        }

        private void TriggerRandomEnchantment()
        {
            // 강화 효과 Attack 생성
            AttackFactory.Instance.Create(attackDatas[randomEnchantmentID], character, null, character.LastMoveDirection);
            Debug.Log($"<color=yellow>[S001] Random enchantment generated: ID {randomEnchantmentID}</color>");
            Debug.Log($"<color=yellow>[S001] {attack.gameObject.name} attackDatas: {attackDatas[randomEnchantmentID].attackId}, attacker: {character.gameObject.name}</color>");
        }

        private void UpdateEnchantmentDuration()
        {
            var killCountDiff = character.killedDuringSkill001 - currentKilledCount;
            currentKilledCount = character.killedDuringSkill001;
            enchantmentDuration += killCountDiff * 0.1f;
        }

        private int GetRandomEnchantmentID()
        {
            // 1-5 사이의 랜덤 숫자 생성
            // TO-DO : 유물 들어왔을 때 값이 최대를 4에서 5로 늘리는 로직 구현해야 함
            int randomValue = Random.Range(character.minRandomEnchantmentID, character.maxRandomEnchantmentID);

            switch (randomValue)
            {
                case 1:
                    return FIRE_ENCHANTMENT_ID;
                case 2:
                    return ICE_ENCHANTMENT_ID;
                case 3:
                    return LIGHTNING_ENCHANTMENT_ID;
                case 4:
                    return LIGHT_ENCHANTMENT_ID;
                default:
                    return FIRE_ENCHANTMENT_ID;
            }
        }
        
        private void SetHeroWeaponElementState(int enchantmentID)
        {
            switch (enchantmentID)
            {
                case FIRE_ENCHANTMENT_ID:
                    character.weaponElementState = HeroWeaponElementState.Fire;
                    break;
                case ICE_ENCHANTMENT_ID:
                    character.weaponElementState = HeroWeaponElementState.Ice;
                    break;
                case LIGHTNING_ENCHANTMENT_ID:
                    character.weaponElementState = HeroWeaponElementState.Lightning;
                    break;
                case LIGHT_ENCHANTMENT_ID:
                    character.weaponElementState = HeroWeaponElementState.Light;
                    // Light 속성일 때 천상 버프 적용 (1회만)
                    ApplyHeavenBuffs();
                    break;
                default:
                    character.weaponElementState = HeroWeaponElementState.None;
                    break;
            }
        }

        /// <summary>
        /// 천상 버프를 적용합니다 (Light 속성일 때 1회만)
        /// </summary>
        private void ApplyHeavenBuffs()
        {
            // 새로운 BUFF 클래스 사용 - Haste 효과 (이동속도 + 공격속도 증가)
            var hasteBuffInfo = new BuffInfo
            {
                buffType = BUFFType.Haste,
                attack = attack,
                target = character, // 자신에게 버프
                buffMultiplier = 100f,
                buffDuration = 7f,
                buffInterval = 7f,
            };

            var hasteBuff = new BUFF();
            hasteBuff.Activate(hasteBuffInfo);

            // 새로운 BUFF 클래스 사용 - 공격범위 증가
            var rangeBuffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackRangeAdd,
                attack = attack,
                target = character, // 자신에게 버프
                buffMultiplier = 100f,
                buffDuration = 7f,
                buffInterval = 7f,
            };

            var rangeBuff = new BUFF();
            rangeBuff.Activate(rangeBuffInfo);

            // 새로운 DEBUFF 클래스 사용 - 방어력 감소
            var debuffInfo = new DebuffInfo
            {
                debuffType = DEBUFFType.DecreaseDefense,
                attack = attack,
                target = character, // 자신에게 디버프
                debuffMultiplier = 50f,
                debuffDuration = 7f,
                debuffInterval = 7f,
            };

            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            Debug.Log("<color=yellow>[S001] 천상 버프 적용 완료!</color>");
        }
    }
} 