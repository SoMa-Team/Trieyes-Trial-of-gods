using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardViews;
using Utils;
using PrimeTween;
using Stats;
using System.Collections.Generic;
using System.Linq;

namespace NodeStage
{
    public class StatEventStage : EventStage<StatEventStage>
    {
        [Header("UI")]
        [SerializeField] private GameObject Background;
        [SerializeField] private Button btnOption1;
        [SerializeField] private Button btnOption2;
        [SerializeField] private Button btnOption3;
        [SerializeField] private TextMeshProUGUI txtOption1;
        [SerializeField] private TextMeshProUGUI txtOption2;
        [SerializeField] private TextMeshProUGUI txtOption3;

        [Header("Popup")]
        [SerializeField] private DeckView deckViewLocal; // 부모의 deckView와 같다면 생략 가능

        private enum StatOptionType
        {
            FixedStatFixedValue,    // 지정된 스탯 + 지정된 값
            FixedStatRandomValue,   // 지정된 스탯 + 랜덤 범위
            RandomStatFixedValue,   // ??? + 지정된 값 (더 큰 값)
            RandomStatRandomValue   // ??? + 랜덤 범위 (더 큰 범위)
        }

        private struct StatOption
        {
            public StatOptionType type;
            public StatType statType;
            public int fixedValue;
            public int minValue;
            public int maxValue;
            public bool isPercentage;
            public string description;
        }

        // 정수형 스탯 (+, % 연산 모두 가능)
        private static readonly StatType[] IntegerStats = {
            StatType.AttackPower,
            StatType.MagicPower,
            StatType.Health,
            StatType.CriticalDamage,
            StatType.Defense,
            StatType.AttackSpeed,
            StatType.SkillCooldownReduction
        };

        // %형 스탯 (% 연산만 가능)
        private static readonly StatType[] PercentageStats = {
            StatType.CriticalRate,
            StatType.MoveSpeed,
            StatType.Evasion
        };

        private StatOption[] selectedOptions = new StatOption[3];
        private AllIn1SpriteShaderHandler shaderHandler;

        // 스탯 증가 값 상수 정의
        private const int FIXED_INTEGER_VALUE = 20;           // opt1. 지정된 정수 값
        private const int FIXED_PERCENTAGE_VALUE = 10;        // opt1. 지정된 % 값
        private const int RANDOM_INTEGER_MIN = -10;           // opt2. 랜덤 정수 최소값
        private const int RANDOM_INTEGER_MAX = 20;            // opt2. 랜덤 정수 최대값
        private const int RANDOM_PERCENTAGE_MIN = -5;         // opt2. 랜덤 % 최소값
        private const int RANDOM_PERCENTAGE_MAX = 10;         // opt2. 랜덤 % 최대값
        
        // 랜덤 스탯 옵션용 더 큰 값들
        private const int RANDOM_FIXED_INTEGER_VALUE = 30;    // opt3, 4. 랜덤 스탯 지정된 정수 값
        private const int RANDOM_FIXED_PERCENTAGE_VALUE = 15; // opt3, 4. 랜덤 스탯 지정된 % 값
        private const int RANDOM_RANGE_INTEGER_MIN = -15;     // opt3, 4. 랜덤 스탯 정수 최소값
        private const int RANDOM_RANGE_INTEGER_MAX = 30;      // opt3, 4. 랜덤 스탯 정수 최대값
        private const int RANDOM_RANGE_PERCENTAGE_MIN = -8;   // opt3, 4. 랜덤 스탯 % 최소값
        private const int RANDOM_RANGE_PERCENTAGE_MAX = 15;   // opt3, 4. 랜덤 스탯 % 최대값

        protected override void OnActivated()
        {
            btnOption1?.onClick.RemoveAllListeners();
            btnOption2?.onClick.RemoveAllListeners();
            btnOption3?.onClick.RemoveAllListeners();

            // 3개의 랜덤 옵션 생성
            GenerateRandomOptions();
            
            // 버튼 이벤트 등록
            btnOption1?.onClick.AddListener(() => ExecuteOption(0));
            btnOption2?.onClick.AddListener(() => ExecuteOption(1));
            btnOption3?.onClick.AddListener(() => ExecuteOption(2));

            // UI 텍스트 업데이트
            UpdateOptionTexts();

            // AllIn1SpriteShaderHandler 초기화
            shaderHandler = new AllIn1SpriteShaderHandler();
            if (Background != null)
            {
                Material mat = Background.GetComponent<Image>().material;
                shaderHandler.SetObject(mat);
                AnimationBackgroundShow();
            }
        }

        private void AnimationBackgroundShow()
        {
            if (Background != null)
            {
                Background.SetActive(true);
            }
        }
        
        private void AnimationBackgroundHide(System.Action onComplete = null)
        {
            if (shaderHandler?.mat == null)
            {
                onComplete?.Invoke();
                return;
            }

            shaderHandler.mat.EnableKeyword("FADE_ON");
            shaderHandler.mat.SetFloat("_FadeAmount", -0.1f);

            Tween.Custom(-0.1f, 1f, 1f, (value) =>
                {
                    shaderHandler.mat.SetFloat("_FadeAmount", value);
                })
                .OnComplete(() =>
                {
                    shaderHandler.mat.DisableKeyword("FADE_ON");
                    shaderHandler.mat.SetFloat("_FadeAmount", -0.1f);
                    onComplete?.Invoke(); // 애니메이션 끝난 후 콜백 실행
                });
        }

        /// <summary>
        /// 4가지 옵션 중 3개를 랜덤으로 선택하여 생성합니다.
        /// </summary>
        private void GenerateRandomOptions()
        {
            var allOptionTypes = new List<StatOptionType>
            {
                StatOptionType.FixedStatFixedValue,
                StatOptionType.FixedStatRandomValue,
                StatOptionType.RandomStatFixedValue,
                StatOptionType.RandomStatRandomValue
            };

            // 4개 중 3개 랜덤 선택
            var selectedTypes = allOptionTypes.OrderBy(x => Random.value).Take(3).ToList();

            for (int i = 0; i < 3; i++)
            {
                selectedOptions[i] = CreateStatOption(selectedTypes[i]);
            }
        }

        /// <summary>
        /// 지정된 타입에 따라 스탯 옵션을 생성합니다.
        /// </summary>
        private StatOption CreateStatOption(StatOptionType type)
        {
            StatOption option = new StatOption();
            option.type = type;

            switch (type)
            {
                case StatOptionType.FixedStatFixedValue:
                    option = CreateFixedStatFixedValueOption();
                    break;
                case StatOptionType.FixedStatRandomValue:
                    option = CreateFixedStatRandomValueOption();
                    break;
                case StatOptionType.RandomStatFixedValue:
                    option = CreateRandomStatFixedValueOption();
                    break;
                case StatOptionType.RandomStatRandomValue:
                    option = CreateRandomStatRandomValueOption();
                    break;
            }

            return option;
        }

        /// <summary>
        /// 지정된 스탯 + 지정된 값 옵션 생성
        /// </summary>
        private StatOption CreateFixedStatFixedValueOption()
        {
            var option = new StatOption();
            option.type = StatOptionType.FixedStatFixedValue;
            
            // ??? 선택
            var allStats = IntegerStats.Concat(PercentageStats).ToArray();
            option.statType = allStats[Random.Range(0, allStats.Length)];
            
            // 정수형 스탯인지 확인
            bool isIntegerStat = IntegerStats.Contains(option.statType);
            
            if (isIntegerStat)
            {
                // 50% 확률로 정수 또는 % 연산 선택
                option.isPercentage = Random.value < 0.5f;
                option.fixedValue = option.isPercentage ? FIXED_PERCENTAGE_VALUE : FIXED_INTEGER_VALUE;
            }
            else
            {
                // %형 스탯은 % 연산만 가능
                option.isPercentage = true;
                option.fixedValue = FIXED_PERCENTAGE_VALUE;
            }

            option.description = GenerateDescription(option);
            return option;
        }

        /// <summary>
        /// 지정된 스탯 + 랜덤 범위 옵션 생성
        /// </summary>
        private StatOption CreateFixedStatRandomValueOption()
        {
            var option = new StatOption();
            option.type = StatOptionType.FixedStatRandomValue;
            
            // ??? 선택
            var allStats = IntegerStats.Concat(PercentageStats).ToArray();
            option.statType = allStats[Random.Range(0, allStats.Length)];
            
            // 정수형 스탯인지 확인
            bool isIntegerStat = IntegerStats.Contains(option.statType);
            
            if (isIntegerStat)
            {
                // 50% 확률로 정수 또는 % 연산 선택
                option.isPercentage = Random.value < 0.5f;
                if (option.isPercentage)
                {
                    option.minValue = RANDOM_PERCENTAGE_MIN;
                    option.maxValue = RANDOM_PERCENTAGE_MAX;
                }
                else
                {
                    option.minValue = RANDOM_INTEGER_MIN;
                    option.maxValue = RANDOM_INTEGER_MAX;
                }
            }
            else
            {
                // %형 스탯은 % 연산만 가능
                option.isPercentage = true;
                option.minValue = RANDOM_PERCENTAGE_MIN;
                option.maxValue = RANDOM_PERCENTAGE_MAX;
            }

            option.description = GenerateDescription(option);
            return option;
        }

        /// <summary>
        /// ??? + 지정된 값 옵션 생성 (더 큰 값)
        /// </summary>
        private StatOption CreateRandomStatFixedValueOption()
        {
            var option = new StatOption();
            option.type = StatOptionType.RandomStatFixedValue;
            
            // ??? 선택
            var allStats = IntegerStats.Concat(PercentageStats).ToArray();
            option.statType = allStats[Random.Range(0, allStats.Length)];
            
            // 정수형 스탯인지 확인
            bool isIntegerStat = IntegerStats.Contains(option.statType);
            
            if (isIntegerStat)
            {
                // 50% 확률로 정수 또는 % 연산 선택
                option.isPercentage = Random.value < 0.5f;
                option.fixedValue = option.isPercentage ? RANDOM_FIXED_PERCENTAGE_VALUE : RANDOM_FIXED_INTEGER_VALUE;
            }
            else
            {
                // %형 스탯은 % 연산만 가능
                option.isPercentage = true;
                option.fixedValue = RANDOM_FIXED_PERCENTAGE_VALUE;
            }

            option.description = GenerateDescription(option);
            return option;
        }

        /// <summary>
        /// ??? + 랜덤 범위 옵션 생성 (더 큰 범위)
        /// </summary>
        private StatOption CreateRandomStatRandomValueOption()
        {
            var option = new StatOption();
            option.type = StatOptionType.RandomStatRandomValue;
            
            // ??? 선택
            var allStats = IntegerStats.Concat(PercentageStats).ToArray();
            option.statType = allStats[Random.Range(0, allStats.Length)];
            
            // 정수형 스탯인지 확인
            bool isIntegerStat = IntegerStats.Contains(option.statType);
            
            if (isIntegerStat)
            {
                // 50% 확률로 정수 또는 % 연산 선택
                option.isPercentage = Random.value < 0.5f;
                if (option.isPercentage)
                {
                    option.minValue = RANDOM_RANGE_PERCENTAGE_MIN;
                    option.maxValue = RANDOM_RANGE_PERCENTAGE_MAX;
                }
                else
                {
                    option.minValue = RANDOM_RANGE_INTEGER_MIN;
                    option.maxValue = RANDOM_RANGE_INTEGER_MAX;
                }
            }
            else
            {
                // %형 스탯은 % 연산만 가능
                option.isPercentage = true;
                option.minValue = RANDOM_RANGE_PERCENTAGE_MIN;
                option.maxValue = RANDOM_RANGE_PERCENTAGE_MAX;
            }

            option.description = GenerateDescription(option);
            return option;
        }

        /// <summary>
        /// 스탯 옵션에 대한 설명 텍스트를 생성합니다.
        /// </summary>
        private string GenerateDescription(StatOption option)
        {
            switch (option.type)
            {
                case StatOptionType.FixedStatFixedValue:
                    string statName1 = StatTypeTransformer.StatTypeToKorean(option.statType);
                    string valueText1 = option.isPercentage ? $"{option.fixedValue}%" : $"{option.fixedValue}";
                    return $"[{statName1}]을 [{valueText1}] 올립니다.";
                    
                case StatOptionType.FixedStatRandomValue:
                    string statName2 = StatTypeTransformer.StatTypeToKorean(option.statType);
                    string rangeText2 = option.isPercentage ? 
                        $"{option.minValue}% ~ {option.maxValue}%" : 
                        $"{option.minValue} ~ {option.maxValue}";
                    return $"[{statName2}]을 [{rangeText2}] 사이로 올립니다.";
                    
                case StatOptionType.RandomStatFixedValue:
                    string valueText3 = option.isPercentage ? $"{option.fixedValue}%" : $"{option.fixedValue}";
                    return $"???을 [{valueText3}] 올립니다.";
                    
                case StatOptionType.RandomStatRandomValue:
                    string rangeText4 = option.isPercentage ? 
                        $"{option.minValue}% ~ {option.maxValue}%" : 
                        $"{option.minValue} ~ {option.maxValue}";
                    return $"???을 [{rangeText4}] 사이로 올립니다.";
                    
                default:
                    return "알 수 없는 옵션";
            }
        }

        /// <summary>
        /// 옵션 텍스트를 UI에 업데이트합니다.
        /// </summary>
        private void UpdateOptionTexts()
        {
            if (txtOption1 != null) txtOption1.text = selectedOptions[0].description;
            if (txtOption2 != null) txtOption2.text = selectedOptions[1].description;
            if (txtOption3 != null) txtOption3.text = selectedOptions[2].description;
        }

        /// <summary>
        /// 선택된 옵션을 실행합니다.
        /// </summary>
        private void ExecuteOption(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= selectedOptions.Length) return;

            var option = selectedOptions[optionIndex];
            ApplyStatModification(option);

            if (Background != null)
            {
                AnimationBackgroundHide(() => base.NextStage());
            }
        }

        /// <summary>
        /// 스탯 수정을 적용합니다.
        /// </summary>
        private void ApplyStatModification(StatOption option)
        {
            // 변경 전 스탯 값 저장
            int beforeValue = mainCharacter.GetRawStatValue(option.statType);
            string statName = StatTypeTransformer.StatTypeToKorean(option.statType);
            
            int valueToApply = 0;

            switch (option.type)
            {
                case StatOptionType.FixedStatFixedValue:
                case StatOptionType.RandomStatFixedValue:
                    valueToApply = option.fixedValue;
                    break;
                    
                case StatOptionType.FixedStatRandomValue:
                case StatOptionType.RandomStatRandomValue:
                    valueToApply = Random.Range(option.minValue, option.maxValue + 1);
                    break;
            }

            // 스탯 적용
            if (option.isPercentage)
            {
                // % 연산 적용 (MultiplyToBasicValue 사용)
                int factor = 100 + valueToApply; // 10% 증가 = 110, -10% 감소 = 90
                mainCharacter.statSheet[option.statType].MultiplyToBasicValue(factor);
                mainCharacter.statSheet[option.statType].SetBasicValue(mainCharacter.GetRawStatValue(option.statType) / 100f); // 100으로 나누어 원래 스케일로 복원
            }
            else
            {
                // 정수 연산 적용
                mainCharacter.statSheet[option.statType].AddToBasicValue(valueToApply);
            }

            // 변경 후 스탯 값 확인, TODO: 문제 없을 시 주석 혹은 삭제 
            int afterValue = mainCharacter.GetRawStatValue(option.statType);
            int actualChange = afterValue - beforeValue;
            
            // 로그 출력
            string valueType = option.isPercentage ? "%" : "";
            string optionTypeName = GetOptionTypeName(option.type);
            
            Debug.LogWarning($"[StatEventStage] {optionTypeName} - {statName}: {beforeValue} → {afterValue} (변화량: {actualChange:+0;-0;0}{valueType})");
        }


        // 변경 후 스탯 값 확인, TODO: 문제 없을 시 주석 혹은 삭제 
        private string GetOptionTypeName(StatOptionType type)
        {
            switch (type)
            {
                case StatOptionType.FixedStatFixedValue: return "지정된 스탯 + 지정된 값";
                case StatOptionType.FixedStatRandomValue: return "지정된 스탯 + 랜덤 범위";
                case StatOptionType.RandomStatFixedValue: return "??? + 지정된 값";
                case StatOptionType.RandomStatRandomValue: return "??? + 랜덤 범위";
                default: return "알 수 없는 옵션";
            }
        }
    }
}
