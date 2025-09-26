using UnityEngine;
using CharacterSystem;
using AttackSystem;
using AttackComponents;
using Stats;

namespace BattleSystem
{
    public enum PilliarType
    {
        Red = 0,    // 체력 회복
        Blue,   // 이동속도 증가 버프
        Yellow  // 공격력 기반 장판 설치
    }

    /// <summary>
    /// 기둥 오브젝트 - 캐릭터가 1초 이상 머물면 특별한 효과를 발동하는 컴포넌트
    /// </summary>
    public class Pilliar : MonoBehaviour
    {
        [Header("기둥 설정")]
        [SerializeField] private PilliarType pilliarType;
        [SerializeField] private Sprite[] pilliarImages;
        [SerializeField] private float activationTime = 1f;      // 발동까지 필요한 시간
        [SerializeField] private float lifetime = 10f;           // 기둥 수명
        
        [Header("효과 설정")]
        [SerializeField] private float healPercentage = 0.1f;    // 빨간 기둥: 최대 체력의 10%
        [SerializeField] private float speedBuffMultiplier = 20f; // 파란 기둥: 이동속도 20% 증가
        [SerializeField] private float speedBuffDuration = 20f;   // 파란 기둥: 버프 지속시간
        [SerializeField] private float fieldDuration = 10f;      // 노란 기둥: 장판 지속시간
        [SerializeField] private float fieldRadius = 3f;         // 노란 기둥: 장판 반경
        [SerializeField] private float fieldDamagePercentage = 0.15f; // 노란 기둥: 공격력의 15%

        [SerializeField] private GameObject fieldDamageVFX;

        private float activationTimer = 0f;
        private float lifetimeTimer = 0f;
        private float fieldTimer = 0f; // 노란 기둥 장판 지속시간 타이머
        private bool isActivated = false;
        private Pawn currentCharacter = null;
        private BoxCollider2D boxCollider;

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                Debug.LogError($"[Pilliar] {pilliarType} 기둥에 BoxCollider2D가 없습니다!");
            }
            else
            {
                boxCollider.isTrigger = true;
            }
            
            // 랜덤하게 기둥 타입 설정 (33.33% 확률)
            SetRandomPilliarType();
        }

        private void Update()
        {
            // 수명 관리 (발동되지 않은 경우에만)
            if (!isActivated)
            {
                lifetimeTimer += Time.deltaTime;
                if (lifetimeTimer >= lifetime)
                {
                    DestroyPilliar();
                    return;
                }
            }

            // 발동 타이머 관리
            if (currentCharacter != null && !isActivated)
            {
                activationTimer += Time.deltaTime;
                if (activationTimer >= activationTime)
                {
                    ActivatePilliar();
                }
            }

            // 노란 기둥의 경우 장판 지속시간 후에 사라짐
            if (isActivated && pilliarType == PilliarType.Yellow)
            {
                fieldTimer += Time.deltaTime;
                if (fieldTimer >= fieldDuration)
                {
                    DestroyPilliar();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Pawn character = other.GetComponent<Pawn>();
            if (character != null && !character.isEnemy)
            {
                currentCharacter = character;
                activationTimer = 0f;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Pawn character = other.GetComponent<Pawn>();
            if (character == currentCharacter)
            {
                currentCharacter = null;
                activationTimer = 0f;
            }
        }

        private void SetRandomPilliarType()
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            // if (randomValue < 0.333f)
            //     pilliarType = PilliarType.Red;
            // else if (randomValue < 0.666f)
            //     pilliarType = PilliarType.Blue;
            // else
            //     pilliarType = PilliarType.Yellow;

            pilliarType = PilliarType.Yellow;
            
            GetComponent<SpriteRenderer>().sprite = pilliarImages[(int)pilliarType];
        }

        private void ActivatePilliar()
        {
            if (isActivated || currentCharacter == null) return;

            isActivated = true;

            switch (pilliarType)
            {
                case PilliarType.Red:
                    ActivateRedPilliar();
                    DestroyPilliar(); // 빨간 기둥은 즉시 사라짐
                    break;
                case PilliarType.Blue:
                    ActivateBluePilliar();
                    DestroyPilliar(); // 파란 기둥은 즉시 사라짐
                    break;
                case PilliarType.Yellow:
                    ActivateYellowPilliar();
                    fieldTimer = 0f; // 장판 지속시간 타이머 초기화
                    // 노란 기둥은 장판 지속시간 후에 사라짐 (DestroyPilliar 호출하지 않음)
                    break;
            }
        }

        private void ActivateRedPilliar()
        {
            // 최대 체력의 10% 회복
            int healAmount = Mathf.RoundToInt(currentCharacter.maxHp * healPercentage);
            currentCharacter.ChangeHP(healAmount);
            
            Debug.LogWarning($"[Pilliar] 빨간 기둥 발동! {currentCharacter.pawnName}이(가) {healAmount} HP 회복");
        }

        private void ActivateBluePilliar()
        {
            // 20초간 이동속도 20% 증가 버프
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseMoveSpeed,
                attack = null, // 기둥은 공격이 아니므로 null
                target = currentCharacter,
                buffMultiplier = speedBuffMultiplier,
                buffDuration = speedBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            
            Debug.LogWarning($"[Pilliar] 파란 기둥 발동! {currentCharacter.pawnName}이(가) {speedBuffDuration}초간 이동속도 {speedBuffMultiplier:F0}% 증가");
        }

        private void ActivateYellowPilliar()
        {
            // 10초간 3radius 범위에 공격력의 15% 데미지 장판 설치
            int fieldDamage = Mathf.RoundToInt(currentCharacter.GetStatValue(StatType.AttackPower) * fieldDamagePercentage);
            
            // AC100 장판 생성
            CreateAC100Field(fieldDamage);
            
            Debug.LogWarning($"[Pilliar] 노란 기둥 발동! {currentCharacter.pawnName}이(가) {fieldDuration}초간 {fieldRadius}반경에 {fieldDamage} 데미지 장판 설치");
        }

        private void CreateAC100Field(int damage)
        {
            // AttackFactory를 사용하여 AC100 공격 생성
            var aoeAttack = AttackFactory.Instance.CreateByID(
                (int)AttackComponentID.AC100_AOE, 
                currentCharacter, 
                null, 
                Vector2.zero
            );

            // AC100 컴포넌트 설정
            var ac100 = aoeAttack.components[0].GetComponent<AC100_AOE>();
            if (ac100 != null)
            {
                ac100.aoeDamage = damage;
                ac100.aoeDuration = fieldDuration;
                ac100.aoeInterval = 1f; // 1초마다 발동
                ac100.aoeRadius = fieldRadius;
                ac100.aoeVFXPrefab = fieldDamageVFX;
                ac100.SetAOEPosition(transform.position); // 기둥 위치에 장판 설치
            }

            ac100.Activate(aoeAttack, Vector2.zero);
        }

        private void DestroyPilliar()
        {
            Destroy(gameObject);
        }
    }
}
