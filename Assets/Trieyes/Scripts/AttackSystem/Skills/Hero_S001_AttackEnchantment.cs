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

        private Character001_Hero character;
        private float lastEnchantmentTime = 0f;
        private float generationInterval = 1f;

        // 강화 효과 상태 관리
        private float enchantmentTimer = 0f;
        private bool isActive = false;

        // 강화 효과 ID 상수
        private const int FIRE_ENCHANTMENT_ID = 3;    // AC003_HeroSwordEnchantmentFire
        private const int ICE_ENCHANTMENT_ID = 4;     // AC004_HeroSwordEnchantmentIce
        private const int LIGHTNING_ENCHANTMENT_ID = 5; // AC005_HeroSwordEnchantmentLightning
        private const int LIGHT_ENCHANTMENT_ID = 6;   // AC006_HeroSwordEnchantmentLight

        public override void Activate(Attack attack, Vector2 direction)
        {
            character = attack.attacker as Character001_Hero;
            enchantmentTimer = 0f;
            isActive = true;
            character.lockBasicAttack = true;

            // 랜덤 강화 효과 선택
            randomEnchantmentID = GetRandomEnchantmentID();
            SetHeroWeaponElementState(randomEnchantmentID);
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            // 강화 효과 타이머 업데이트
            enchantmentTimer += Time.deltaTime;

            // 1초에 1번 강화 효과 생성
            if (Time.time - lastEnchantmentTime >= generationInterval)
            {
                TriggerRandomEnchantment();
                lastEnchantmentTime = Time.time;
            }

            // 강화 지속 시간 체크 & 공격 속도에 따른 공격 속도 조절
            if (enchantmentTimer >= enchantmentDuration)
            {
                // 강화 효과 종료
                isActive = false;
                AttackFactory.Instance.Deactivate(attack);
            }
        }

        public override void Deactivate()
        {
            character.weaponElementState = HeroWeaponElementState.None;
            character.lockBasicAttack = false;
            base.Deactivate();
            AttackFactory.Instance.Deactivate(attack);
        }

        private void TriggerRandomEnchantment()
        {
            // 강화 효과 Attack 생성
            Attack enchantmentAttack = AttackFactory.Instance.ClonePrefab(randomEnchantmentID);
            BattleStage.now.AttachAttack(enchantmentAttack);

            enchantmentAttack.Activate(character, Vector2.zero);
            character.ExecuteSkillAttack(enchantmentAttack.attackData);

            Debug.Log($"<color=yellow>[ENCHANTMENT] Random enchantment generated: ID {randomEnchantmentID}</color>");
        }

        private int GetRandomEnchantmentID()
        {
            // 1-4 사이의 랜덤 숫자 생성
            // int randomValue = Random.Range(1, 5);
            int randomValue = 4;
            
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalDamage = 0
            };

            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            Debug.Log("<color=yellow>[S001] 천상 버프 적용 완료!</color>");
        }
    }
} 