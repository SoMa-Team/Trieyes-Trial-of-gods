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
        [SerializeField] private Button[] optionButtons = new Button[3];
        [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[3];

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
            // RandomStatRandomValue 옵션용 추가 필드
            public int negativeValue;
            public int positiveValue;
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

        // 스탯 값 상수 정의
        private static readonly StatValues FixedValues = new StatValues(20, 10);
        private static readonly StatValues RandomValues = new StatValues(-10, 20, -5, 10);
        private static readonly StatValues RandomFixedValues = new StatValues(30, 15);
        private static readonly StatValues RandomRangeValues = new StatValues(-15, 30, -8, 15);

        private struct StatValues
        {
            public readonly int IntegerValue;
            public readonly int PercentageValue;
            public readonly int IntegerMin;
            public readonly int IntegerMax;
            public readonly int PercentageMin;
            public readonly int PercentageMax;

            public StatValues(int intVal, int pctVal) : this(intVal, pctVal, 0, 0, 0, 0) { }
            public StatValues(int intMin, int intMax, int pctMin, int pctMax) : this(0, 0, intMin, intMax, pctMin, pctMax) { }
            public StatValues(int intVal, int pctVal, int intMin, int intMax, int pctMin, int pctMax)
            {
                IntegerValue = intVal;
                PercentageValue = pctVal;
                IntegerMin = intMin;
                IntegerMax = intMax;
                PercentageMin = pctMin;
                PercentageMax = pctMax;
            }
        }

        protected override void OnActivated()
        {
            // 버튼 이벤트 초기화 및 등록
            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i]?.onClick.RemoveAllListeners();
                int index = i; // 클로저를 위한 로컬 변수
                optionButtons[i]?.onClick.AddListener(() => ExecuteOption(index));
            }

            // 3개의 랜덤 옵션 생성
            GenerateRandomOptions();
            
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
            var option = new StatOption { type = type };
            var (statType, isPercentage) = SelectRandomStatAndType();
            option.statType = statType;
            option.isPercentage = isPercentage;

            switch (type)
            {
                case StatOptionType.FixedStatFixedValue:
                    option.fixedValue = isPercentage ? FixedValues.PercentageValue : FixedValues.IntegerValue;
                    break;
                case StatOptionType.FixedStatRandomValue:
                    if (isPercentage)
                    {
                        option.minValue = RandomValues.PercentageMin;
                        option.maxValue = RandomValues.PercentageMax;
                    }
                    else
                    {
                        option.minValue = RandomValues.IntegerMin;
                        option.maxValue = RandomValues.IntegerMax;
                    }
                    break;
                case StatOptionType.RandomStatFixedValue:
                    option.fixedValue = isPercentage ? RandomFixedValues.PercentageValue : RandomFixedValues.IntegerValue;
                    break;
                case StatOptionType.RandomStatRandomValue:
                    if (isPercentage)
                    {
                        option.negativeValue = Random.Range(RandomRangeValues.PercentageMin, 0);
                        option.positiveValue = Random.Range(1, RandomRangeValues.PercentageMax + 1);
                    }
                    else
                    {
                        option.negativeValue = Random.Range(RandomRangeValues.IntegerMin, 0);
                        option.positiveValue = Random.Range(1, RandomRangeValues.IntegerMax + 1);
                    }
                    break;
            }

            option.description = GenerateDescription(option);
            return option;
        }

        /// <summary>
        /// 랜덤 스탯과 연산 타입을 선택합니다.
        /// </summary>
        private (StatType statType, bool isPercentage) SelectRandomStatAndType()
        {
            var allStats = IntegerStats.Concat(PercentageStats).ToArray();
            var statType = allStats[Random.Range(0, allStats.Length)];
            var isIntegerStat = IntegerStats.Contains(statType);
            
            bool isPercentage = isIntegerStat ? Random.value < 0.5f : true;
            return (statType, isPercentage);
        }


        /// <summary>
        /// 스탯 옵션에 대한 설명 텍스트를 생성합니다.
        /// </summary>
        private string GenerateDescription(StatOption option)
        {
            string suffix = option.isPercentage ? "%" : "";
            string statName = option.type == StatOptionType.FixedStatFixedValue || option.type == StatOptionType.FixedStatRandomValue
                ? StatTypeTransformer.StatTypeToKorean(option.statType) : "???";
            
            return option.type switch
            {
                StatOptionType.FixedStatFixedValue => $"{statName}이(가) {option.fixedValue}{suffix}만큼 증가합니다.",
                StatOptionType.FixedStatRandomValue => $"{statName}이(가) {option.minValue}{suffix} ~ {option.maxValue}{suffix}만큼 증가합니다.",
                StatOptionType.RandomStatFixedValue => $"{statName}이(가) {option.fixedValue}{suffix}만큼 증가합니다.",
                StatOptionType.RandomStatRandomValue => $"{statName}이(가) {option.negativeValue}{suffix} 또는 {option.positiveValue}{suffix}만큼 변화합니다.",
                _ => "알 수 없는 옵션"
            };
        }

        /// <summary>
        /// 옵션 텍스트를 UI에 업데이트합니다.
        /// </summary>
        private void UpdateOptionTexts()
        {
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i]?.SetText(selectedOptions[i].description);
            }
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
                    valueToApply = Random.Range(option.minValue, option.maxValue + 1);
                    break;
                    
                case StatOptionType.RandomStatRandomValue:
                    // 50% 확률로 음수 또는 양수 값 선택
                    valueToApply = Random.value < 0.5f ? option.negativeValue : option.positiveValue;
                    break;
            }

            // 정수 스탯인 경우
            if (IntegerStats.Contains(option.statType))
            {     
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
            }

            // 퍼센트 스탯인 경우
            else
            {
                // 스탯 적용
                if (option.isPercentage)
                {
                    mainCharacter.statSheet[option.statType].AddToBasicValue(valueToApply);
                }
            }

        }
    }
}
